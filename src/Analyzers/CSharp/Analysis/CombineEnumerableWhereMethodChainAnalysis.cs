// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    internal static class CombineEnumerableWhereMethodChainAnalysis
    {
        public static void Analyze(
            SyntaxNodeAnalysisContext context,
            in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            SimpleMemberInvocationExpressionInfo invocationInfo2 = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocationInfo.Expression);

            if (!invocationInfo2.Success)
                return;

            if (invocationInfo2.Arguments.Count != 1)
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            if (!string.Equals(invocationInfo2.NameText, "Where", StringComparison.Ordinal))
                return;

            IMethodSymbol methodSymbol2 = semanticModel.GetReducedExtensionMethodInfo(invocationInfo2.InvocationExpression, cancellationToken).Symbol;

            if (methodSymbol2 == null)
                return;

            if (!SymbolUtility.IsLinqExtensionOfIEnumerableOfT(methodSymbol2, "Where", parameterCount: 2))
                return;

            if (SymbolUtility.IsPredicateFunc(
                methodSymbol2.Parameters[1].Type,
                methodSymbol2.TypeArguments[0]))
            {
                IMethodSymbol methodSymbol = semanticModel.GetReducedExtensionMethodInfo(invocationInfo.InvocationExpression, cancellationToken).Symbol;

                if (methodSymbol != null
                    && SymbolUtility.IsLinqWhere(methodSymbol))
                {
                    Analyze(context, invocationInfo, invocationInfo2);
                }
            }
            else if (SymbolUtility.IsPredicateFunc(
                methodSymbol2.Parameters[1].Type,
                methodSymbol2.TypeArguments[0],
                semanticModel.Compilation.GetSpecialType(SpecialType.System_Int32)))
            {
                IMethodSymbol methodSymbol = semanticModel.GetReducedExtensionMethodInfo(invocationInfo.InvocationExpression, cancellationToken).Symbol;

                if (methodSymbol != null
                    && SymbolUtility.IsLinqWhereWithIndex(methodSymbol))
                {
                    Analyze(context, invocationInfo, invocationInfo2);
                }
            }
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            in SimpleMemberInvocationExpressionInfo invocationInfo2)
        {
            ExpressionSyntax expression = invocationInfo.Arguments[0].Expression;

            if (!AreEquivalentLambdas(expression, invocationInfo2.Arguments[0].Expression))
                return;

            InvocationExpressionSyntax invocation = invocationInfo.InvocationExpression;

            TextSpan span = TextSpan.FromBounds(invocationInfo2.Name.SpanStart, invocation.Span.End);

            if (invocation.ContainsDirectives(span))
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.CombineEnumerableWhereMethodChain,
                Location.Create(invocation.SyntaxTree, span));

            TextSpan fadeOutSpan = TextSpan.FromBounds(
                invocationInfo.OperatorToken.SpanStart,
                ((LambdaExpressionSyntax)expression).ArrowToken.Span.End);

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.CombineEnumerableWhereMethodChainFadeOut, Location.Create(invocation.SyntaxTree, fadeOutSpan));
            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.CombineEnumerableWhereMethodChainFadeOut, invocation.ArgumentList.CloseParenToken);
        }

        private static bool AreEquivalentLambdas(ExpressionSyntax expression1, ExpressionSyntax expression2)
        {
            if (expression1 == null)
                return false;

            if (expression2 == null)
                return false;

            SyntaxKind kind = expression1.Kind();

            if (kind != expression2.Kind())
                return false;

            if (kind == SyntaxKind.SimpleLambdaExpression)
            {
                var lambda1 = (SimpleLambdaExpressionSyntax)expression1;
                var lambda2 = (SimpleLambdaExpressionSyntax)expression2;

                return ParameterIdentifierEquals(lambda1.Parameter, lambda2.Parameter)
                    && lambda1.Body is ExpressionSyntax
                    && lambda2.Body is ExpressionSyntax;
            }
            else if (kind == SyntaxKind.ParenthesizedLambdaExpression)
            {
                var lambda1 = (ParenthesizedLambdaExpressionSyntax)expression1;
                var lambda2 = (ParenthesizedLambdaExpressionSyntax)expression2;

                ParameterListSyntax parameterList1 = lambda1.ParameterList;
                ParameterListSyntax parameterList2 = lambda2.ParameterList;

                if (parameterList1 != null
                    && parameterList2 != null)
                {
                    SeparatedSyntaxList<ParameterSyntax> parameters1 = parameterList1.Parameters;
                    SeparatedSyntaxList<ParameterSyntax> parameters2 = parameterList2.Parameters;

                    if (parameters1.Count == parameters2.Count)
                    {
                        for (int i = 0; i < parameters1.Count; i++)
                        {
                            if (!ParameterIdentifierEquals(parameters1[i], parameters2[i]))
                                return false;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private static bool ParameterIdentifierEquals(ParameterSyntax parameter1, ParameterSyntax parameter2)
        {
            return parameter1?.Identifier.ValueText.Equals(parameter2.Identifier.ValueText, StringComparison.Ordinal) == true;
        }
    }
}
