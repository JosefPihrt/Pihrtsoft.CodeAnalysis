// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ParenthesizeConditionOfConditionalExpressionAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.ParenthesizeConditionOfConditionalExpression, CommonDiagnosticRules.AnalyzerOptionIsObsolete);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeConditionalExpression(f), SyntaxKind.ConditionalExpression);
        }

        private static void AnalyzeConditionalExpression(SyntaxNodeAnalysisContext context)
        {
            var conditionalExpression = (ConditionalExpressionSyntax)context.Node;

            if (conditionalExpression.ContainsDiagnostics)
                return;

            ExpressionSyntax condition = conditionalExpression.Condition;

            if (condition == null)
                return;

            SyntaxKind kind = condition.Kind();

            if (kind == SyntaxKind.ParenthesizedExpression)
            {
                if (AnalyzerOptions.RemoveParenthesesFromConditionOfConditionalExpressionWhenExpressionIsSingleToken.IsEnabled(context))
                {
                    var parenthesizedExpression = (ParenthesizedExpressionSyntax)condition;

                    ExpressionSyntax expression = parenthesizedExpression.Expression;

                    if (!expression.IsMissing
                        && CSharpFacts.IsSingleTokenExpression(expression.Kind()))
                    {
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.RemoveParenthesesFromConditionOfConditionalExpressionWhenExpressionIsSingleToken, condition);
                    }
                }
            }
            else if (!CSharpFacts.IsSingleTokenExpression(kind)
                || !AnalyzerOptions.RemoveParenthesesFromConditionOfConditionalExpressionWhenExpressionIsSingleToken.IsEnabled(context))
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ParenthesizeConditionOfConditionalExpression, condition);
            }
        }
    }
}
