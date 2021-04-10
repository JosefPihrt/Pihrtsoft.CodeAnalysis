// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.CSharp.Analysis
{
    internal static class SimplifyBooleanComparisonAnalysis
    {
        public static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            BinaryExpressionSyntax binaryExpression,
            ExpressionSyntax left,
            ExpressionSyntax right,
            bool fadeOut)
        {
            if (!DiagnosticRules.SimplifyBooleanComparison.IsEffective(context))
                return;

            if (binaryExpression.SpanContainsDirectives())
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.SimplifyBooleanComparison, binaryExpression);

            if (!fadeOut)
                return;

            DiagnosticDescriptor fadeOutDescriptor = DiagnosticRules.SimplifyBooleanComparisonFadeOut;

            DiagnosticHelpers.ReportToken(context, fadeOutDescriptor, binaryExpression.OperatorToken);

            switch (binaryExpression.Kind())
            {
                case SyntaxKind.EqualsExpression:
                    {
                        if (left.IsKind(SyntaxKind.FalseLiteralExpression))
                        {
                            DiagnosticHelpers.ReportNode(context, fadeOutDescriptor, left);

                            if (right.IsKind(SyntaxKind.LogicalNotExpression))
                                DiagnosticHelpers.ReportToken(context, fadeOutDescriptor, ((PrefixUnaryExpressionSyntax)right).OperatorToken);
                        }
                        else if (right.IsKind(SyntaxKind.FalseLiteralExpression))
                        {
                            DiagnosticHelpers.ReportNode(context, fadeOutDescriptor, right);

                            if (left.IsKind(SyntaxKind.LogicalNotExpression))
                                DiagnosticHelpers.ReportToken(context, fadeOutDescriptor, ((PrefixUnaryExpressionSyntax)left).OperatorToken);
                        }

                        break;
                    }
                case SyntaxKind.NotEqualsExpression:
                    {
                        if (left.IsKind(SyntaxKind.TrueLiteralExpression))
                        {
                            DiagnosticHelpers.ReportNode(context, fadeOutDescriptor, left);

                            if (right.IsKind(SyntaxKind.LogicalNotExpression))
                                DiagnosticHelpers.ReportToken(context, fadeOutDescriptor, ((PrefixUnaryExpressionSyntax)right).OperatorToken);
                        }
                        else if (right.IsKind(SyntaxKind.TrueLiteralExpression))
                        {
                            DiagnosticHelpers.ReportNode(context, fadeOutDescriptor, right);

                            if (left.IsKind(SyntaxKind.LogicalNotExpression))
                                DiagnosticHelpers.ReportToken(context, fadeOutDescriptor, ((PrefixUnaryExpressionSyntax)left).OperatorToken);
                        }

                        break;
                    }
            }
        }
    }
}
