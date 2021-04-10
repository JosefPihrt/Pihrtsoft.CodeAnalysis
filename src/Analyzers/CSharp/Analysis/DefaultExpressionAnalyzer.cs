// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DefaultExpressionAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.SimplifyDefaultExpression);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (((CSharpCompilation)startContext.Compilation).LanguageVersion >= LanguageVersion.CSharp7_1)
                    startContext.RegisterSyntaxNodeAction(f => AnalyzeDefaultExpression(f), SyntaxKind.DefaultExpression);
            });
        }

        public static void AnalyzeDefaultExpression(SyntaxNodeAnalysisContext context)
        {
            var defaultExpression = (DefaultExpressionSyntax)context.Node;

            ExpressionSyntax expression = defaultExpression.WalkUpParentheses();

            SyntaxNode parent = expression.Parent;

            switch (parent.Kind())
            {
                case SyntaxKind.EqualsValueClause:
                    {
                        parent = parent.Parent;

                        switch (parent.Kind())
                        {
                            case SyntaxKind.Parameter:
                                {
                                    ReportDiagnostic();
                                    return;
                                }
                            case SyntaxKind.VariableDeclarator:
                                {
                                    return;
                                }
                            default:
                                {
                                    Debug.WriteLine($"{parent.Kind()} {parent}");
                                    return;
                                }
                        }
                    }
                case SyntaxKind.ConditionalExpression:
                    {
                        var conditionalExpression = (ConditionalExpressionSyntax)parent;

                        ExpressionSyntax expression2 = (conditionalExpression.WhenTrue == expression)
                            ? conditionalExpression.WhenFalse
                            : conditionalExpression.WhenTrue;

                        if (expression2.IsKind(SyntaxKind.ThrowExpression))
                            return;

                        TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken);

                        ITypeSymbol type = typeInfo.Type;
                        ITypeSymbol convertedType = typeInfo.ConvertedType;

                        if (!SymbolEqualityComparer.Default.Equals(type, convertedType))
                            return;

                        ITypeSymbol type2 = context.SemanticModel.GetTypeSymbol(expression2, context.CancellationToken);

                        if (!SymbolEqualityComparer.Default.Equals(type, type2))
                            return;

                        ReportDiagnostic();
                        return;
                    }
                case SyntaxKind.ArrowExpressionClause:
                case SyntaxKind.CoalesceExpression:
                case SyntaxKind.ReturnStatement:
                case SyntaxKind.YieldReturnStatement:
                case SyntaxKind.SimpleAssignmentExpression:
                    {
                        if (parent.IsParentKind(SyntaxKind.ObjectInitializerExpression))
                            return;

                        TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken);

                        if (!SymbolEqualityComparer.Default.Equals(typeInfo.Type, typeInfo.ConvertedType))
                            return;

                        if (parent.IsKind(SyntaxKind.ReturnStatement)
                            && IsTypeInferredFromDefaultExpression(context, parent))
                        {
                            return;
                        }

                        ReportDiagnostic();
                        return;
                    }
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                    {
                        TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken);

                        if (!SymbolEqualityComparer.Default.Equals(typeInfo.Type, typeInfo.ConvertedType))
                            return;

                        ReportDiagnostic();
                        return;
                    }
                case SyntaxKind.Argument:
                case SyntaxKind.ConstantPattern:
                case SyntaxKind.CaseSwitchLabel:
                    {
                        return;
                    }
                default:
                    {
                        Debug.WriteLine($"{parent.Kind()} {parent}");
                        return;
                    }
            }

            void ReportDiagnostic()
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.SimplifyDefaultExpression, Location.Create(defaultExpression.SyntaxTree, defaultExpression.ParenthesesSpan()));
            }
        }

        private static bool IsTypeInferredFromDefaultExpression(
            SyntaxNodeAnalysisContext context,
            SyntaxNode node)
        {
            while (node != null)
            {
                SyntaxKind kind = node.Kind();

                if (CSharpFacts.IsAnonymousFunctionExpression(kind))
                {
                    if (IsTypeInferredFromDefaultExpression2(context, node))
                        return true;
                }
                else
                {
                    switch (kind)
                    {
                        case SyntaxKind.LocalFunctionStatement:
                        case SyntaxKind.MethodDeclaration:
                        case SyntaxKind.OperatorDeclaration:
                        case SyntaxKind.ConversionOperatorDeclaration:
                        case SyntaxKind.PropertyDeclaration:
                        case SyntaxKind.IndexerDeclaration:
                        case SyntaxKind.GetAccessorDeclaration:
                            return false;
                    }
                }

                node = node.Parent;
            }

            return false;
        }

        private static bool IsTypeInferredFromDefaultExpression2(
            SyntaxNodeAnalysisContext context,
            SyntaxNode anonymousFunction)
        {
            if (context.SemanticModel.GetSymbol(anonymousFunction, context.CancellationToken) is IMethodSymbol methodSymbol
                && methodSymbol.MethodKind == MethodKind.AnonymousFunction)
            {
                SyntaxNode node = ((ExpressionSyntax)anonymousFunction).WalkUpParentheses().Parent;

                if (node is ArgumentSyntax argument
                    && node.IsParentKind(SyntaxKind.ArgumentList)
                    && node.Parent.IsParentKind(SyntaxKind.InvocationExpression)
                    && !IsGenericInvocation((InvocationExpressionSyntax)node.Parent.Parent))
                {
                    IParameterSymbol parameterSymbol = context.SemanticModel.DetermineParameter(argument);

                    if (parameterSymbol?.OriginalDefinition.Type is INamedTypeSymbol namedTypeSymbol)
                    {
                        ITypeParameterSymbol typeParameterSymbol = namedTypeSymbol.TypeParameters.LastOrDefault();

                        if (typeParameterSymbol != null
                            && parameterSymbol.ContainingSymbol.OriginalDefinition is IMethodSymbol methodSymbol2)
                        {
                            foreach (ITypeParameterSymbol typeParameterSymbol2 in methodSymbol2.TypeParameters)
                            {
                                if (string.Equals(typeParameterSymbol.Name, typeParameterSymbol2.Name, StringComparison.Ordinal))
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsGenericInvocation(InvocationExpressionSyntax invocationExpression)
        {
            if (invocationExpression.Expression.IsKind(SyntaxKind.GenericName))
                return true;

            if (invocationExpression.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                && ((MemberAccessExpressionSyntax)invocationExpression.Expression).Name.IsKind(SyntaxKind.GenericName))
            {
                return true;
            }

            return false;
        }
    }
}
