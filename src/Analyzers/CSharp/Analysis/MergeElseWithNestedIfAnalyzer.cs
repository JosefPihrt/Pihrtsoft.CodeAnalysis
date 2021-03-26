// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MergeElseWithNestedIfAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.MergeElseWithNestedIf,
                    DiagnosticDescriptors.MergeElseWithNestedIfFadeOut);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticDescriptors.MergeElseWithNestedIf.IsEffective(c))
                        AnalyzeElseClause(c);
                },
                SyntaxKind.ElseClause);
        }

        private static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
        {
            var elseClause = (ElseClauseSyntax)context.Node;

            if (!(elseClause.Statement is BlockSyntax block))
                return;

            if (!(block.Statements.SingleOrDefault(shouldThrow: false) is IfStatementSyntax ifStatement))
                return;

            if (!elseClause.ElseKeyword.TrailingTrivia.IsEmptyOrWhitespace()
                || !block.OpenBraceToken.LeadingTrivia.IsEmptyOrWhitespace()
                || !block.OpenBraceToken.TrailingTrivia.IsEmptyOrWhitespace()
                || !ifStatement.IfKeyword.LeadingTrivia.IsEmptyOrWhitespace()
                || !ifStatement.GetTrailingTrivia().IsEmptyOrWhitespace()
                || !block.CloseBraceToken.LeadingTrivia.IsEmptyOrWhitespace())
            {
                return;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.MergeElseWithNestedIf, block);
            CSharpDiagnosticHelpers.ReportBraces(context, DiagnosticDescriptors.MergeElseWithNestedIfFadeOut, block);
        }
    }
}
