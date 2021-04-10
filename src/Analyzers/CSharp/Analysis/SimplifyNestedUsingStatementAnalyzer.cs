// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SimplifyNestedUsingStatementAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.SimplifyNestedUsingStatement,
                        DiagnosticRules.SimplifyNestedUsingStatementFadeOut);
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
                    if (DiagnosticRules.SimplifyNestedUsingStatement.IsEffective(c))
                        AnalyzeUsingStatement(c);
                },
                SyntaxKind.UsingStatement);
        }

        private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var usingStatement = (UsingStatementSyntax)context.Node;

            if (!ContainsEmbeddableUsingStatement(usingStatement))
                return;

            for (SyntaxNode parent = usingStatement.Parent; parent != null; parent = parent.Parent)
            {
                if (parent.IsKind(SyntaxKind.UsingStatement)
                    && ContainsEmbeddableUsingStatement((UsingStatementSyntax)parent))
                {
                    return;
                }
            }

            var block = (BlockSyntax)usingStatement.Statement;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.SimplifyNestedUsingStatement, block);

            CSharpDiagnosticHelpers.ReportBraces(context, DiagnosticRules.SimplifyNestedUsingStatementFadeOut, block);
        }

        public static bool ContainsEmbeddableUsingStatement(UsingStatementSyntax usingStatement)
        {
            return usingStatement.Statement is BlockSyntax block
                && block.Statements.SingleOrDefault(shouldThrow: false) is UsingStatementSyntax usingStatement2
                && block.OpenBraceToken.TrailingTrivia.IsEmptyOrWhitespace()
                && block.CloseBraceToken.LeadingTrivia.IsEmptyOrWhitespace()
                && usingStatement2.GetLeadingTrivia().IsEmptyOrWhitespace()
                && usingStatement2.GetTrailingTrivia().IsEmptyOrWhitespace();
        }
    }
}
