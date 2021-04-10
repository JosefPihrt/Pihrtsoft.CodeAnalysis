// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    internal static class RemoveRedundantStringToCharArrayCallAnalysis
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            if (invocationInfo.OperatorToken.ContainsDirectives)
                return;

            if (invocationInfo.Name.ContainsDirectives)
                return;

            if (invocationInfo.ArgumentList.ContainsDirectives)
                return;

            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            if (!ParentIsElementAccessOrForEachExpression(invocationExpression.WalkUpParentheses()))
                return;

            IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(invocationExpression, context.CancellationToken);

            if (!SymbolUtility.IsPublicInstanceNonGeneric(methodSymbol, "ToCharArray"))
                return;

            if (methodSymbol.ContainingType?.SpecialType != SpecialType.System_String)
                return;

            if (methodSymbol.Parameters.Any())
                return;

            if (!(methodSymbol.ReturnType is IArrayTypeSymbol arrayType))
                return;

            if (arrayType.ElementType.SpecialType != SpecialType.System_Char)
                return;

            TextSpan span = TextSpan.FromBounds(invocationInfo.MemberAccessExpression.OperatorToken.SpanStart, invocationExpression.Span.End);

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveRedundantStringToCharArrayCall,
                Location.Create(invocationExpression.SyntaxTree, span));
        }

        private static bool ParentIsElementAccessOrForEachExpression(ExpressionSyntax expression)
        {
            if (expression.IsParentKind(SyntaxKind.ElementAccessExpression))
                return true;

            if (expression.IsParentKind(SyntaxKind.ForEachStatement))
            {
                var forEachStatement = (ForEachStatementSyntax)expression.Parent;

                if (expression == forEachStatement.Expression)
                    return true;
            }

            return false;
        }
    }
}