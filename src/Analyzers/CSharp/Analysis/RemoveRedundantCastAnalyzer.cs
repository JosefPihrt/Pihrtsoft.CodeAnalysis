// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RemoveRedundantCastAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveRedundantCast);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeCastExpression(f), SyntaxKind.CastExpression);
        }

        private static void AnalyzeCastExpression(SyntaxNodeAnalysisContext context)
        {
            var castExpression = (CastExpressionSyntax)context.Node;

            if (castExpression.ContainsDiagnostics)
                return;

            if (!(castExpression.Parent is ParenthesizedExpressionSyntax parenthesizedExpression))
                return;

            ExpressionSyntax accessedExpression = GetAccessedExpression(parenthesizedExpression.Parent);

            if (accessedExpression == null)
                return;

            TypeSyntax type = castExpression.Type;

            if (type == null)
                return;

            ExpressionSyntax expression = castExpression.Expression;

            if (expression == null)
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(type, cancellationToken);

            if (typeSymbol?.IsErrorType() != false)
                return;

            ITypeSymbol expressionTypeSymbol = semanticModel.GetTypeSymbol(expression, cancellationToken);

            if (expressionTypeSymbol?.IsErrorType() != false)
                return;

            if (expressionTypeSymbol.TypeKind == TypeKind.Interface)
                return;

            if (expressionTypeSymbol.SpecialType == SpecialType.System_Object
                || expressionTypeSymbol.TypeKind == TypeKind.Dynamic
                || typeSymbol.TypeKind != TypeKind.Interface)
            {
                if (!typeSymbol.EqualsOrInheritsFrom(expressionTypeSymbol, includeInterfaces: true))
                    return;
            }

            ISymbol accessedSymbol = semanticModel.GetSymbol(accessedExpression, cancellationToken);

            INamedTypeSymbol containingType = accessedSymbol?.ContainingType;

            if (containingType == null)
                return;

            if (typeSymbol.TypeKind == TypeKind.Interface)
            {
                if (accessedSymbol.IsAbstract
                    && !CheckExplicitImplementation(expressionTypeSymbol, accessedSymbol))
                {
                    return;
                }
            }
            else
            {
                if (!CheckAccessibility(expressionTypeSymbol.OriginalDefinition, accessedSymbol, expression.SpanStart, semanticModel, cancellationToken))
                    return;

                if (!expressionTypeSymbol.EqualsOrInheritsFrom(containingType, includeInterfaces: true))
                    return;
            }

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveRedundantCast,
                Location.Create(castExpression.SyntaxTree, castExpression.ParenthesesSpan()));
        }

        private static bool CheckExplicitImplementation(ITypeSymbol typeSymbol, ISymbol symbol)
        {
            ISymbol implementation = typeSymbol.FindImplementationForInterfaceMember(symbol);

            if (implementation == null)
                return false;

            switch (implementation.Kind)
            {
                case SymbolKind.Property:
                    {
                        foreach (IPropertySymbol propertySymbol in ((IPropertySymbol)implementation).ExplicitInterfaceImplementations)
                        {
                            if (SymbolEqualityComparer.Default.Equals(propertySymbol.OriginalDefinition, symbol.OriginalDefinition))
                                return false;
                        }

                        break;
                    }
                case SymbolKind.Method:
                    {
                        foreach (IMethodSymbol methodSymbol in ((IMethodSymbol)implementation).ExplicitInterfaceImplementations)
                        {
                            if (SymbolEqualityComparer.Default.Equals(methodSymbol.OriginalDefinition, symbol.OriginalDefinition))
                                return false;
                        }

                        break;
                    }
                default:
                    {
                        Debug.Fail(implementation.Kind.ToString());
                        return false;
                    }
            }

            return true;
        }

        private static bool CheckAccessibility(
            ITypeSymbol expressionTypeSymbol,
            ISymbol accessedSymbol,
            int position,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            Accessibility accessibility = accessedSymbol.DeclaredAccessibility;

            if (accessibility == Accessibility.Protected
                || accessibility == Accessibility.ProtectedAndInternal)
            {
                INamedTypeSymbol containingType = semanticModel.GetEnclosingNamedType(position, cancellationToken);

                while (containingType != null)
                {
                    if (SymbolEqualityComparer.Default.Equals(containingType, expressionTypeSymbol))
                        return true;

                    containingType = containingType.ContainingType;
                }

                return false;
            }
            else if (accessibility == Accessibility.ProtectedOrInternal)
            {
                INamedTypeSymbol containingType = semanticModel.GetEnclosingNamedType(position, cancellationToken);

                if (SymbolEqualityComparer.Default.Equals(containingType?.ContainingAssembly, expressionTypeSymbol.ContainingAssembly))
                    return true;

                while (containingType != null)
                {
                    if (SymbolEqualityComparer.Default.Equals(containingType, expressionTypeSymbol))
                        return true;

                    containingType = containingType.ContainingType;
                }

                return false;
            }

            return true;
        }

        private static ExpressionSyntax GetAccessedExpression(SyntaxNode node)
        {
            switch (node?.Kind())
            {
                case SyntaxKind.SimpleMemberAccessExpression:
                case SyntaxKind.ElementAccessExpression:
                    return (ExpressionSyntax)node;
                case SyntaxKind.ConditionalAccessExpression:
                    return ((ConditionalAccessExpressionSyntax)node).WhenNotNull;
                default:
                    return null;
            }
        }

        public static void Analyze(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            ExtensionMethodSymbolInfo extensionInfo = semanticModel.GetReducedExtensionMethodInfo(invocationExpression, cancellationToken);

            if (extensionInfo.Symbol == null)
                return;

            if (!SymbolUtility.IsLinqCast(extensionInfo.Symbol))
                return;

            ITypeSymbol typeArgument = extensionInfo.ReducedSymbol.TypeArguments.SingleOrDefault(shouldThrow: false);

            if (typeArgument == null)
                return;

            var memberAccessExpressionType = semanticModel.GetTypeSymbol(invocationInfo.Expression, cancellationToken) as INamedTypeSymbol;

            if (memberAccessExpressionType?.OriginalDefinition.IsIEnumerableOfT() != true)
                return;

            if (!SymbolEqualityComparer.IncludeNullability.Equals(typeArgument, memberAccessExpressionType.TypeArguments[0]))
                return;

            if (invocationExpression.ContainsDirectives(TextSpan.FromBounds(invocationInfo.Expression.Span.End, invocationExpression.Span.End)))
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveRedundantCast,
                Location.Create(invocationExpression.SyntaxTree, TextSpan.FromBounds(invocationInfo.Name.SpanStart, invocationInfo.ArgumentList.Span.End)));
        }
    }
}
