// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidMultilineExpressionBodyAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AvoidMultilineExpressionBody, CommonDiagnosticRules.AnalyzerOptionIsObsolete);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine.IsEnabled(c, checkParent: true) == false)
                        AnalyzeArrowExpressionClause(c);
                },
                SyntaxKind.ArrowExpressionClause);
        }

        private static void AnalyzeArrowExpressionClause(SyntaxNodeAnalysisContext context)
        {
            var arrowExpressionClause = (ArrowExpressionClauseSyntax)context.Node;

            if (arrowExpressionClause.ContainsDiagnostics)
                return;

            if (arrowExpressionClause.ContainsDirectives)
                return;

            ExpressionSyntax expression = arrowExpressionClause.Expression;

            if (expression?.IsMultiLine() == true)
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AvoidMultilineExpressionBody, expression, AnalyzerOptions.ConvertExpressionBodyToBlockBodyWhenExpressionIsMultiLine);
        }
    }
}
