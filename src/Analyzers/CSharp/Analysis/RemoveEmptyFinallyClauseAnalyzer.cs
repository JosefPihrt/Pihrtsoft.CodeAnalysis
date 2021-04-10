// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RemoveEmptyFinallyClauseAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                {
                    Immutable.InterlockedInitialize(
                        ref _supportedDiagnostics,
                        DiagnosticRules.RemoveEmptyFinallyClause,
                        DiagnosticRules.RemoveEmptyFinallyClauseFadeOut);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeFinallyClause(f), SyntaxKind.FinallyClause);
        }

        private static void AnalyzeFinallyClause(SyntaxNodeAnalysisContext context)
        {
            var finallyClause = (FinallyClauseSyntax)context.Node;

            if (!(finallyClause.Parent is TryStatementSyntax tryStatement))
                return;

            BlockSyntax finallyBlock = finallyClause.Block;

            if (finallyBlock?.Statements.Any() != false)
                return;

            if (!finallyClause.FinallyKeyword.TrailingTrivia.IsEmptyOrWhitespace())
                return;

            if (!SyntaxTriviaAnalysis.IsExteriorTriviaEmptyOrWhitespace(finallyBlock.OpenBraceToken))
                return;

            if (!finallyBlock.CloseBraceToken.LeadingTrivia.IsEmptyOrWhitespace())
                return;

            if (tryStatement.Catches.Any())
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveEmptyFinallyClause, finallyClause);
            }
            else
            {
                BlockSyntax tryBlock = tryStatement.Block;

                if (tryBlock?.Statements.Any() != true)
                    return;

                if (!SyntaxTriviaAnalysis.IsExteriorTriviaEmptyOrWhitespace(tryStatement.TryKeyword))
                    return;

                if (!SyntaxTriviaAnalysis.IsExteriorTriviaEmptyOrWhitespace(tryBlock.OpenBraceToken))
                    return;

                if (!SyntaxTriviaAnalysis.IsExteriorTriviaEmptyOrWhitespace(tryBlock.CloseBraceToken))
                    return;

                if (!finallyClause.FinallyKeyword.LeadingTrivia.IsEmptyOrWhitespace())
                    return;

                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveEmptyFinallyClause, finallyClause);

                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveEmptyFinallyClauseFadeOut, tryStatement.TryKeyword);
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveEmptyFinallyClauseFadeOut, tryBlock.OpenBraceToken);
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveEmptyFinallyClauseFadeOut, tryBlock.CloseBraceToken);
            }
        }
    }
}
