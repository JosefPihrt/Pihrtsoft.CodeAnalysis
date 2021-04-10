// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Analysis.If;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class IfStatementAnalyzer : BaseDiagnosticAnalyzer
    {
        internal static IfAnalysisOptions AnalysisOptions { get; } = new IfAnalysisOptions(
            useCoalesceExpression: true,
            useConditionalExpression: false,
            useBooleanExpression: false,
            useExpression: true);

        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                {
                    Immutable.InterlockedInitialize(
                        ref _supportedDiagnostics,
                        DiagnosticRules.UseCoalesceExpressionInsteadOfIf,
                        DiagnosticRules.ConvertIfToReturnStatement,
                        DiagnosticRules.ConvertIfToReturnStatementFadeOut,
                        DiagnosticRules.ConvertIfToAssignment);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticHelpers.IsAnyEffective(
                        c,
                        DiagnosticRules.UseCoalesceExpressionInsteadOfIf,
                        DiagnosticRules.ConvertIfToReturnStatement,
                        DiagnosticRules.ConvertIfToAssignment))
                    {
                        AnalyzeIfStatement(c);
                    }
                },
                SyntaxKind.IfStatement);
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            foreach (IfAnalysis analysis in IfAnalysis.Analyze(ifStatement, AnalysisOptions, context.SemanticModel, context.CancellationToken))
            {
                Debug.Assert(
                    analysis.Kind == IfAnalysisKind.IfElseToAssignmentWithCoalesceExpression
                        || analysis.Kind == IfAnalysisKind.IfElseToAssignmentWithExpression
                        || analysis.Kind == IfAnalysisKind.IfElseToAssignmentWithCondition
                        || analysis.Kind == IfAnalysisKind.IfElseToReturnWithCoalesceExpression
                        || analysis.Kind == IfAnalysisKind.IfElseToYieldReturnWithCoalesceExpression
                        || analysis.Kind == IfAnalysisKind.IfReturnToReturnWithCoalesceExpression
                        || analysis.Kind == IfAnalysisKind.IfElseToReturnWithExpression
                        || analysis.Kind == IfAnalysisKind.IfElseToYieldReturnWithExpression
                        || analysis.Kind == IfAnalysisKind.IfReturnToReturnWithExpression,
                    analysis.Kind.ToString());

                switch (analysis.Kind)
                {
                    case IfAnalysisKind.IfElseToAssignmentWithCoalesceExpression:
                    case IfAnalysisKind.IfElseToReturnWithCoalesceExpression:
                    case IfAnalysisKind.IfElseToYieldReturnWithCoalesceExpression:
                    case IfAnalysisKind.IfReturnToReturnWithCoalesceExpression:
                        {
                            if (DiagnosticRules.UseCoalesceExpressionInsteadOfIf.IsEffective(context))
                                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseCoalesceExpressionInsteadOfIf, ifStatement);

                            break;
                        }
                    case IfAnalysisKind.IfElseToReturnWithExpression:
                    case IfAnalysisKind.IfElseToYieldReturnWithExpression:
                    case IfAnalysisKind.IfReturnToReturnWithExpression:
                        {
                            if (DiagnosticRules.ConvertIfToReturnStatement.IsEffective(context))
                                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ConvertIfToReturnStatement, ifStatement);

                            break;
                        }
                    case IfAnalysisKind.IfElseToAssignmentWithExpression:
                    case IfAnalysisKind.IfElseToAssignmentWithCondition:
                        {
                            if (DiagnosticRules.ConvertIfToAssignment.IsEffective(context))
                                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ConvertIfToAssignment, ifStatement);

                            break;
                        }
                }
            }
        }
    }
}
