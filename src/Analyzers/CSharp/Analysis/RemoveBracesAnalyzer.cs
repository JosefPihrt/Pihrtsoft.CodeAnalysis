// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Roslynator.CSharp.Analysis.EmbeddedStatementAnalysis;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RemoveBracesAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.RemoveBraces,
                        DiagnosticRules.RemoveBracesFadeOut);
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
                    if (DiagnosticRules.RemoveBraces.IsEffective(c))
                        AnalyzeIfStatement(c);
                },
                SyntaxKind.IfStatement);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.RemoveBraces.IsEffective(c))
                        AnalyzeCommonForEachStatement(c);
                },
                SyntaxKind.ForEachStatement);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.RemoveBraces.IsEffective(c))
                        AnalyzeCommonForEachStatement(c);
                },
                SyntaxKind.ForEachVariableStatement);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.RemoveBraces.IsEffective(c))
                        AnalyzeForStatement(c);
                },
                SyntaxKind.ForStatement);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.RemoveBraces.IsEffective(c))
                        AnalyzeUsingStatement(c);
                },
                SyntaxKind.UsingStatement);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.RemoveBraces.IsEffective(c))
                        AnalyzeWhileStatement(c);
                },
                SyntaxKind.WhileStatement);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.RemoveBraces.IsEffective(c))
                        AnalyzeDoStatement(c);
                },
                SyntaxKind.DoStatement);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.RemoveBraces.IsEffective(c))
                        AnalyzeLockStatement(c);
                },
                SyntaxKind.LockStatement);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.RemoveBraces.IsEffective(c))
                        AnalyzeFixedStatement(c);
                },
                SyntaxKind.FixedStatement);
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            if (!ifStatement.IsSimpleIf())
                return;

            BlockSyntax block = GetFixableBlock(ifStatement.Statement);

            if (block == null)
                return;

            if (!FormattingSupportsEmbeddedStatement(ifStatement))
                return;

            Analyze(context, block);
        }

        private static void AnalyzeCommonForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (CommonForEachStatementSyntax)context.Node;

            BlockSyntax block = GetFixableBlock(forEachStatement.Statement);

            if (block == null)
                return;

            if (!FormattingSupportsEmbeddedStatement(forEachStatement))
                return;

            Analyze(context, block);
        }

        private static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
        {
            var forStatement = (ForStatementSyntax)context.Node;

            BlockSyntax block = GetFixableBlock(forStatement.Statement);

            if (block == null)
                return;

            if (!FormattingSupportsEmbeddedStatement(forStatement))
                return;

            Analyze(context, block);
        }

        private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var usingStatement = (UsingStatementSyntax)context.Node;

            BlockSyntax block = GetFixableBlock(usingStatement.Statement);

            if (block == null)
                return;

            if (!FormattingSupportsEmbeddedStatement(usingStatement))
                return;

            Analyze(context, block);
        }

        private static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
        {
            var whileStatement = (WhileStatementSyntax)context.Node;

            BlockSyntax block = GetFixableBlock(whileStatement.Statement);

            if (block == null)
                return;

            if (!FormattingSupportsEmbeddedStatement(whileStatement))
                return;

            Analyze(context, block);
        }

        private static void AnalyzeDoStatement(SyntaxNodeAnalysisContext context)
        {
            var doStatement = (DoStatementSyntax)context.Node;

            BlockSyntax block = GetFixableBlock(doStatement.Statement);

            if (block == null)
                return;

            if (!FormattingSupportsEmbeddedStatement(doStatement))
                return;

            Analyze(context, block);
        }

        private static void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
        {
            var lockStatement = (LockStatementSyntax)context.Node;

            BlockSyntax block = GetFixableBlock(lockStatement.Statement);

            if (block == null)
                return;

            if (!FormattingSupportsEmbeddedStatement(lockStatement))
                return;

            Analyze(context, block);
        }

        private static void AnalyzeFixedStatement(SyntaxNodeAnalysisContext context)
        {
            var fixedStatement = (FixedStatementSyntax)context.Node;

            BlockSyntax block = GetFixableBlock(fixedStatement.Statement);

            if (block == null)
                return;

            if (!FormattingSupportsEmbeddedStatement(fixedStatement))
                return;

            Analyze(context, block);
        }

        private static BlockSyntax GetFixableBlock(StatementSyntax statement)
        {
            if (!(statement is BlockSyntax block))
                return null;

            statement = block.Statements.SingleOrDefault(shouldThrow: false);

            if (statement == null)
                return null;

            if (statement.IsKind(SyntaxKind.LocalDeclarationStatement, SyntaxKind.LabeledStatement))
                return null;

            if (!statement.IsSingleLine())
                return null;

            return block;
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, BlockSyntax block)
        {
            SyntaxToken openBrace = block.OpenBraceToken;
            SyntaxToken closeBrace = block.CloseBraceToken;

            if (openBrace.IsMissing
                || closeBrace.IsMissing
                || !openBrace.LeadingTrivia.IsEmptyOrWhitespace()
                || !openBrace.TrailingTrivia.IsEmptyOrWhitespace()
                || !closeBrace.LeadingTrivia.IsEmptyOrWhitespace()
                || !closeBrace.TrailingTrivia.IsEmptyOrWhitespace())
            {
                return;
            }

            string title = CSharpFacts.GetTitle(block.Parent);

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveBraces, block, title);

            CSharpDiagnosticHelpers.ReportBraces(context, DiagnosticRules.RemoveBracesFadeOut, block, title);
        }
    }
}
