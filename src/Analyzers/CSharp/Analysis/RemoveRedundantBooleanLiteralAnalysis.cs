// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using static Roslynator.CSharp.CSharpFacts;

namespace Roslynator.CSharp.Analysis
{
    internal static class RemoveRedundantBooleanLiteralAnalysis
    {
        public static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            BinaryExpressionSyntax binaryExpression,
            ExpressionSyntax left,
            ExpressionSyntax right)
        {
            if (!DiagnosticRules.RemoveRedundantBooleanLiteral.IsEffective(context))
                return;

            if (binaryExpression.SpanContainsDirectives())
                return;

            TextSpan span = GetSpanToRemove(binaryExpression, left, right);

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveRedundantBooleanLiteral,
                Location.Create(binaryExpression.SyntaxTree, span),
                binaryExpression.ToString(span));
        }

        public static TextSpan GetSpanToRemove(BinaryExpressionSyntax binaryExpression, ExpressionSyntax left, ExpressionSyntax right)
        {
            SyntaxToken operatorToken = binaryExpression.OperatorToken;

            if (IsBooleanLiteralExpression(left.Kind()))
            {
                return TextSpan.FromBounds(left.SpanStart, operatorToken.Span.End);
            }
            else
            {
                return TextSpan.FromBounds(operatorToken.SpanStart, right.Span.End);
            }
        }
    }
}
