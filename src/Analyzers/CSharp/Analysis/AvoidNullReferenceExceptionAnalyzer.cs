// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
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
    public sealed class AvoidNullReferenceExceptionAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AvoidNullReferenceException);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeAsExpression(f), SyntaxKind.AsExpression);
        }

        public static void Analyze(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            ExpressionSyntax expression = invocationExpression.WalkUpParentheses();

            ExpressionSyntax accessExpression = GetAccessExpression(expression);

            if (accessExpression == null)
                return;

            if (accessExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                IMethodSymbol methodSymbol2 = context.SemanticModel.GetMethodSymbol(accessExpression, context.CancellationToken);

                if (methodSymbol2 == null)
                    return;

                if (methodSymbol2.IsExtensionMethod
                    && !methodSymbol2.ContainingType.HasMetadataName(MetadataNames.System_Linq_Enumerable))
                {
                    return;
                }
            }

            IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(invocationExpression, context.CancellationToken);

            if (methodSymbol?.ReturnType.IsReferenceType != true)
                return;

            methodSymbol = methodSymbol.OriginalDefinition.ReducedFromOrSelf();

            INamedTypeSymbol containingType = methodSymbol.ContainingType;

            if (containingType == null)
                return;

            string methodName = methodSymbol.Name.Remove(methodSymbol.Name.Length - "OrDefault".Length);

            if (!ContainsComplementMethod(methodSymbol, containingType.GetMembers(methodName)))
                return;

            ReportDiagnostic(context, expression);

            static bool ContainsComplementMethod(IMethodSymbol methodSymbol, ImmutableArray<ISymbol> symbols)
            {
                foreach (ISymbol symbol in symbols)
                {
                    if (symbol.Kind != SymbolKind.Method)
                        continue;

                    var methodSymbol2 = (IMethodSymbol)symbol;

                    if (methodSymbol.TypeParameters.Length != methodSymbol2.TypeParameters.Length)
                        continue;

                    if (!TypeSymbolEquals(methodSymbol.ReturnType, methodSymbol2.ReturnType))
                        continue;

                    ImmutableArray<IParameterSymbol> parameters = methodSymbol.Parameters;
                    ImmutableArray<IParameterSymbol> parameters2 = methodSymbol2.Parameters;

                    if (parameters.Length != parameters2.Length)
                        continue;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (!ParameterSymbolEquals(parameters[i], parameters2[i]))
                            continue;
                    }

                    return true;
                }

                return false;

                static bool TypeSymbolEquals(ITypeSymbol symbol1, ITypeSymbol symbol2)
                {
                    if (symbol1.TypeKind == TypeKind.TypeParameter
                        && symbol2.TypeKind == TypeKind.TypeParameter
                        && SymbolEqualityComparer.Default.Equals(symbol1.ContainingType, symbol2.ContainingType)
                        && string.Equals(symbol1.Name, symbol2.Name, StringComparison.Ordinal))
                    {
                        return true;
                    }

                    return SymbolEqualityComparer.Default.Equals(symbol1.OriginalDefinition, symbol2.OriginalDefinition);
                }

                static bool ParameterSymbolEquals(IParameterSymbol x, IParameterSymbol y)
                {
                    if (object.ReferenceEquals(x, y))
                        return true;

                    if (x == null)
                        return false;

                    if (y == null)
                        return false;

                    if (x.RefKind != y.RefKind)
                        return false;

                    if (x.IsParams != y.IsParams)
                        return false;

                    if (x.IsOptional != y.IsOptional)
                        return false;

                    if (x.IsThis != y.IsThis)
                        return false;

                    if (x.IsDiscard != y.IsDiscard)
                        return false;

                    if (x.HasExplicitDefaultValue != y.HasExplicitDefaultValue)
                        return false;

                    if (!TypeSymbolEquals(x.Type, y.Type))
                        return false;

                    return true;
                }
            }
        }

        private static void AnalyzeAsExpression(SyntaxNodeAnalysisContext context)
        {
            var asExpression = (BinaryExpressionSyntax)context.Node;

            if (asExpression.ContainsDiagnostics)
                return;

            ExpressionSyntax expression = asExpression.WalkUpParentheses();

            if (asExpression == expression)
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            if (asExpression.Left.IsKind(SyntaxKind.ThisExpression))
            {
                var interfaceSymbol = semanticModel.GetTypeSymbol(asExpression.Right, cancellationToken) as INamedTypeSymbol;

                if (interfaceSymbol?.TypeKind == TypeKind.Interface)
                {
                    ITypeSymbol thisTypeSymbol = semanticModel.GetTypeSymbol(asExpression.Left, cancellationToken);

                    if (thisTypeSymbol.Implements(interfaceSymbol, allInterfaces: true))
                        return;
                }
            }

            SyntaxNode topExpression = GetAccessExpression(expression)?.WalkUp(f => f.IsKind(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxKind.ElementAccessExpression,
                SyntaxKind.InvocationExpression,
                SyntaxKind.ParenthesizedExpression));

            if (topExpression == null)
                return;

            if (semanticModel
                .GetTypeSymbol(asExpression, cancellationToken)?
                .IsReferenceType != true)
            {
                return;
            }

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(topExpression, cancellationToken);

            if (typeSymbol == null)
                return;

            if (!typeSymbol.IsReferenceType && !typeSymbol.IsValueType)
                return;

            if (semanticModel.GetSymbol(topExpression, cancellationToken) is IMethodSymbol methodSymbol
                && methodSymbol.IsExtensionMethod)
            {
                return;
            }

            ReportDiagnostic(context, expression);
        }

        private static ExpressionSyntax GetAccessExpression(ExpressionSyntax expression)
        {
            SyntaxNode parent = expression.Parent;

            switch (parent?.Kind())
            {
                case SyntaxKind.SimpleMemberAccessExpression:
                    {
                        var memberAccessExpression = (MemberAccessExpressionSyntax)parent;

                        if (expression == memberAccessExpression.Expression)
                            return memberAccessExpression;

                        break;
                    }
                case SyntaxKind.ElementAccessExpression:
                    {
                        var elementAccess = (ElementAccessExpressionSyntax)parent;

                        if (expression == elementAccess.Expression)
                            return elementAccess;

                        break;
                    }
            }

            return null;
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
        {
            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.AvoidNullReferenceException,
                Location.Create(expression.SyntaxTree, new TextSpan(expression.Span.End, 1)));
        }
    }
}
