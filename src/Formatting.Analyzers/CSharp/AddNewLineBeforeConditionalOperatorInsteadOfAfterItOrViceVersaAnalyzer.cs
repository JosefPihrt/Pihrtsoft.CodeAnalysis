// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AddNewLineBeforeConditionalOperatorInsteadOfAfterItOrViceVersaAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AddNewLineBeforeConditionalOperatorInsteadOfAfterItOrViceVersa);

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

            ExpressionSyntax condition = conditionalExpression.Condition;

            if (condition.IsMissing)
                return;

            ExpressionSyntax whenTrue = conditionalExpression.WhenTrue;

            if (whenTrue.IsMissing)
                return;

            if (SyntaxTriviaAnalysis.IsTokenFollowedWithNewLineAndNotPrecededWithNewLine(condition, conditionalExpression.QuestionToken, whenTrue))
            {
                if (!AnalyzerOptions.AddNewLineAfterConditionalOperatorInsteadOfBeforeIt.IsEnabled(context))
                {
                    ReportDiagnostic(DiagnosticRules.AddNewLineBeforeConditionalOperatorInsteadOfAfterItOrViceVersa, conditionalExpression.QuestionToken, ImmutableDictionary<string, string>.Empty);
                    return;
                }
            }
            else if (SyntaxTriviaAnalysis.IsTokenPrecededWithNewLineAndNotFollowedWithNewLine(condition, conditionalExpression.QuestionToken, whenTrue))
            {
                if (AnalyzerOptions.AddNewLineAfterConditionalOperatorInsteadOfBeforeIt.IsEnabled(context))
                {
                    ReportDiagnostic(DiagnosticRules.ReportOnly.AddNewLineAfterConditionalOperatorInsteadOfBeforeIt, conditionalExpression.QuestionToken, DiagnosticProperties.AnalyzerOption_Invert);
                    return;
                }
            }

            ExpressionSyntax whenFalse = conditionalExpression.WhenFalse;

            if (!whenFalse.IsMissing)
            {
                if (SyntaxTriviaAnalysis.IsTokenFollowedWithNewLineAndNotPrecededWithNewLine(whenTrue, conditionalExpression.ColonToken, whenFalse))
                {
                    if (!AnalyzerOptions.AddNewLineAfterConditionalOperatorInsteadOfBeforeIt.IsEnabled(context))
                        ReportDiagnostic(DiagnosticRules.AddNewLineBeforeConditionalOperatorInsteadOfAfterItOrViceVersa, conditionalExpression.ColonToken, ImmutableDictionary<string, string>.Empty);
                }
                else if (SyntaxTriviaAnalysis.IsTokenPrecededWithNewLineAndNotFollowedWithNewLine(whenTrue, conditionalExpression.ColonToken, whenFalse))
                {
                    if (AnalyzerOptions.AddNewLineAfterConditionalOperatorInsteadOfBeforeIt.IsEnabled(context))
                        ReportDiagnostic(DiagnosticRules.ReportOnly.AddNewLineAfterConditionalOperatorInsteadOfBeforeIt, conditionalExpression.ColonToken, DiagnosticProperties.AnalyzerOption_Invert);
                }
            }

            void ReportDiagnostic(DiagnosticDescriptor descriptor, SyntaxToken token, ImmutableDictionary<string, string> properties)
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    descriptor,
                    Location.Create(token.SyntaxTree, token.Span.WithLength(0)),
                    properties: properties);
            }
        }
    }
}
