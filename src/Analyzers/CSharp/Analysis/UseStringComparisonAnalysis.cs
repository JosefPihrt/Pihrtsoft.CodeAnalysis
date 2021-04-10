// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    internal static class UseStringComparisonAnalysis
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            ExpressionSyntax expression = invocationInfo.InvocationExpression.WalkUpParentheses();

            SyntaxNode parent = expression.Parent;

            SyntaxKind kind = parent.Kind();

            if (kind == SyntaxKind.SimpleMemberAccessExpression)
            {
                SimpleMemberInvocationExpressionInfo invocationInfo2 = SyntaxInfo.SimpleMemberInvocationExpressionInfo(parent.Parent);

                if (!invocationInfo2.Success)
                    return;

                Analyze(context, invocationInfo, invocationInfo2);
            }
            else if (kind == SyntaxKind.Argument)
            {
                Analyze(context, invocationInfo, (ArgumentSyntax)parent);
            }
            else if (kind == SyntaxKind.EqualsExpression
                || kind == SyntaxKind.NotEqualsExpression)
            {
                Analyze(context, invocationInfo, expression, (BinaryExpressionSyntax)parent);
            }
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            in SimpleMemberInvocationExpressionInfo invocationInfo2)
        {
            if (invocationInfo2.InvocationExpression.SpanContainsDirectives())
                return;

            string name2 = invocationInfo2.NameText;

            if (name2 != "Equals"
                && name2 != "StartsWith"
                && name2 != "EndsWith"
                && name2 != "IndexOf"
                && name2 != "LastIndexOf"
                && name2 != "Contains")
            {
                return;
            }

            SeparatedSyntaxList<ArgumentSyntax> arguments = invocationInfo2.Arguments;

            if (arguments.Count != 1)
                return;

            ExpressionSyntax argumentExpression = arguments[0].Expression.WalkDownParentheses();

            string name = invocationInfo.NameText;

            SimpleMemberInvocationExpressionInfo invocationInfo3;

            bool isStringLiteral = argumentExpression.IsKind(SyntaxKind.StringLiteralExpression);

            if (!isStringLiteral)
            {
                invocationInfo3 = SyntaxInfo.SimpleMemberInvocationExpressionInfo(argumentExpression);

                if (!invocationInfo3.Success)
                    return;

                string name3 = invocationInfo3.NameText;

                if (name != name3)
                    return;
            }

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            if (!CheckSymbol(invocationInfo, semanticModel, cancellationToken))
                return;

            IMethodSymbol methodSymbol = semanticModel.GetMethodSymbol(invocationInfo2.InvocationExpression, cancellationToken);

            if (!SymbolUtility.IsPublicInstanceNonGeneric(methodSymbol, name2))
                return;

            if (!methodSymbol.IsContainingType(SpecialType.System_String))
                return;

            SpecialType returnType = (name2.EndsWith("IndexOf", StringComparison.Ordinal))
                ? SpecialType.System_Int32
                : SpecialType.System_Boolean;

            if (!methodSymbol.IsReturnType(returnType))
                return;

            if (!methodSymbol.HasSingleParameter(SpecialType.System_String))
                return;

            if (!isStringLiteral
                && !CheckSymbol(invocationInfo3, semanticModel, cancellationToken))
            {
                return;
            }

            ReportDiagnostic(context, invocationInfo2);
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            ArgumentSyntax argument)
        {
            if (!(argument.Parent is ArgumentListSyntax argumentList))
                return;

            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;

            if (arguments.Count != 2)
                return;

            SimpleMemberInvocationExpressionInfo equalsInvocation = SyntaxInfo.SimpleMemberInvocationExpressionInfo(argumentList.Parent);

            if (!equalsInvocation.Success)
                return;

            if (equalsInvocation.NameText != "Equals")
                return;

            if (!IsFixable(context, invocationInfo, argument, arguments))
                return;

            IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(equalsInvocation.InvocationExpression, context.CancellationToken);

            if (!SymbolUtility.IsPublicStaticNonGeneric(methodSymbol, "Equals"))
                return;

            if (!methodSymbol.IsContainingType(SpecialType.System_String))
                return;

            if (!methodSymbol.IsReturnType(SpecialType.System_Boolean))
                return;

            if (!methodSymbol.HasTwoParameters(SpecialType.System_String, SpecialType.System_String))
                return;

            ReportDiagnostic(context, equalsInvocation);
        }

        private static bool IsFixable(
            SyntaxNodeAnalysisContext context,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            ArgumentSyntax argument,
            SeparatedSyntaxList<ArgumentSyntax> arguments)
        {
            if (object.ReferenceEquals(argument, arguments[0]))
            {
                ExpressionSyntax expression = arguments[1].Expression?.WalkDownParentheses();

                if (expression != null)
                {
                    SyntaxKind kind = expression.Kind();

                    if (kind == SyntaxKind.InvocationExpression)
                    {
                        return TryCreateCaseChangingInvocation(expression, out SimpleMemberInvocationExpressionInfo invocationInfo2)
                            && invocationInfo.NameText == invocationInfo2.NameText
                            && CheckSymbol(invocationInfo, context.SemanticModel, context.CancellationToken)
                            && CheckSymbol(invocationInfo2, context.SemanticModel, context.CancellationToken);
                    }
                    else if (kind == SyntaxKind.StringLiteralExpression)
                    {
                        return CheckSymbol(invocationInfo, context.SemanticModel, context.CancellationToken);
                    }
                }
            }
            else
            {
                return arguments[0].Expression?.WalkDownParentheses().Kind() == SyntaxKind.StringLiteralExpression
                    && CheckSymbol(invocationInfo, context.SemanticModel, context.CancellationToken);
            }

            return false;
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            ExpressionSyntax leftOrRight,
            BinaryExpressionSyntax binaryExpression)
        {
            if (object.ReferenceEquals(leftOrRight, binaryExpression.Left))
            {
                ExpressionSyntax right = binaryExpression.Right?.WalkDownParentheses();

                if (right != null)
                {
                    SyntaxKind kind = right.Kind();

                    if (kind == SyntaxKind.InvocationExpression)
                    {
                        if (TryCreateCaseChangingInvocation(right, out SimpleMemberInvocationExpressionInfo invocationInfo2)
                            && invocationInfo.NameText == invocationInfo2.NameText
                            && CheckSymbol(invocationInfo, context.SemanticModel, context.CancellationToken)
                            && CheckSymbol(invocationInfo2, context.SemanticModel, context.CancellationToken))
                        {
                            ReportDiagnostic(context, binaryExpression);
                        }
                    }
                    else if (kind == SyntaxKind.StringLiteralExpression)
                    {
                        if (CheckSymbol(invocationInfo, context.SemanticModel, context.CancellationToken))
                        {
                            ReportDiagnostic(context, binaryExpression);
                        }
                    }
                }
            }
            else if (binaryExpression.Left?.WalkDownParentheses().Kind() == SyntaxKind.StringLiteralExpression
                && CheckSymbol(invocationInfo, context.SemanticModel, context.CancellationToken))
            {
                ReportDiagnostic(context, binaryExpression);
            }
        }

        private static bool TryCreateCaseChangingInvocation(ExpressionSyntax expression, out SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(expression);

            if (invocationInfo.Success
                && !invocationInfo.Arguments.Any())
            {
                string name = invocationInfo.NameText;

                return name == "ToLower"
                    || name == "ToLowerInvariant"
                    || name == "ToUpper"
                    || name == "ToUpperInvariant";
            }

            invocationInfo = default;
            return false;
        }

        private static bool CheckSymbol(
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            IMethodSymbol methodSymbol = semanticModel.GetMethodSymbol(invocationInfo.InvocationExpression, cancellationToken);

            return SymbolUtility.IsPublicInstanceNonGeneric(methodSymbol)
                && methodSymbol.IsContainingType(SpecialType.System_String)
                && methodSymbol.IsReturnType(SpecialType.System_String)
                && !methodSymbol.Parameters.Any();
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            ReportDiagnostic(context, invocationInfo.InvocationExpression);
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, SyntaxNode node)
        {
            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseStringComparison, node);
        }
    }
}
