// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SimplifyConditionalExpressionAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.SimplifyConditionalExpression, CommonDiagnosticRules.AnalyzerOptionIsObsolete);

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
            if (context.Node.ContainsDiagnostics)
                return;

            if (context.Node.SpanContainsDirectives())
                return;

            var conditionalExpression = (ConditionalExpressionSyntax)context.Node;

            ConditionalExpressionInfo info = SyntaxInfo.ConditionalExpressionInfo(conditionalExpression);

            if (!info.Success)
                return;

            SyntaxKind trueKind = info.WhenTrue.Kind();
            SyntaxKind falseKind = info.WhenFalse.Kind();

            if (trueKind == SyntaxKind.TrueLiteralExpression)
            {
                // a ? true : false >>> a
                // a ? true : b >>> a || b
                if (falseKind == SyntaxKind.FalseLiteralExpression
                    || (falseKind != SyntaxKind.ThrowExpression
                        && context.SemanticModel.GetTypeInfo(info.WhenFalse, context.CancellationToken).ConvertedType?.SpecialType == SpecialType.System_Boolean))
                {
                    ReportDiagnostic();
                }
            }
            else if (trueKind == SyntaxKind.FalseLiteralExpression)
            {
                /// a ? false : true >>> !a
                if (falseKind == SyntaxKind.TrueLiteralExpression)
                {
                    ReportDiagnostic();
                }
                /// a ? false : b >>> !a && b
                else if (falseKind != SyntaxKind.ThrowExpression
                    && !AnalyzerOptions.DoNotSimplifyConditionalExpressionWhenConditionIsInverted.IsEnabled(context)
                    && context.SemanticModel.GetTypeInfo(info.WhenFalse, context.CancellationToken).ConvertedType?.SpecialType == SpecialType.System_Boolean)
                {
                    DiagnosticHelpers.ReportDiagnosticIfNotSuppressed(context, DiagnosticRules.SimplifyConditionalExpression, conditionalExpression, AnalyzerOptions.DoNotSimplifyConditionalExpressionWhenConditionIsInverted);
                    ReportDiagnostic();
                }
            }
            else if (falseKind == SyntaxKind.TrueLiteralExpression)
            {
                // a ? b : true >>> !a || b
                if (trueKind != SyntaxKind.ThrowExpression
                    && !AnalyzerOptions.DoNotSimplifyConditionalExpressionWhenConditionIsInverted.IsEnabled(context)
                    && context.SemanticModel.GetTypeInfo(info.WhenTrue, context.CancellationToken).ConvertedType?.SpecialType == SpecialType.System_Boolean)
                {
                    DiagnosticHelpers.ReportDiagnosticIfNotSuppressed(context, DiagnosticRules.SimplifyConditionalExpression, conditionalExpression, AnalyzerOptions.DoNotSimplifyConditionalExpressionWhenConditionIsInverted);
                    ReportDiagnostic();
                }
            }
            else if (falseKind == SyntaxKind.FalseLiteralExpression)
            {
                // a ? b : false >>> a && b
                if (trueKind != SyntaxKind.ThrowExpression
                    && context.SemanticModel.GetTypeInfo(info.WhenTrue, context.CancellationToken).ConvertedType?.SpecialType == SpecialType.System_Boolean)
                {
                    ReportDiagnostic();
                }
            }

            void ReportDiagnostic()
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.SimplifyConditionalExpression, conditionalExpression);
            }
        }
    }
}
