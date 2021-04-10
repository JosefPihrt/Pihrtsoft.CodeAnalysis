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
    public sealed class AddBracesWhenMultilineAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AddBracesWhenExpressionSpansOverMultipleLines);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeIfStatement(f), SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeCommonForEachStatement(f), SyntaxKind.ForEachStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeCommonForEachStatement(f), SyntaxKind.ForEachVariableStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeForStatement(f), SyntaxKind.ForStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeUsingStatement(f), SyntaxKind.UsingStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeWhileStatement(f), SyntaxKind.WhileStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeDoStatement(f), SyntaxKind.DoStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeLockStatement(f), SyntaxKind.LockStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeFixedStatement(f), SyntaxKind.FixedStatement);
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            if (!ifStatement.IsSimpleIf())
                return;

            StatementSyntax statement = ifStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            if (statement.IsSingleLine() && FormattingSupportsEmbeddedStatement(ifStatement))
                return;

            ReportDiagnostic(context, ifStatement, statement);
        }

        private static void AnalyzeCommonForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (CommonForEachStatementSyntax)context.Node;

            StatementSyntax statement = forEachStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            if (statement.IsSingleLine() && FormattingSupportsEmbeddedStatement(forEachStatement))
                return;

            ReportDiagnostic(context, forEachStatement, statement);
        }

        private static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
        {
            var forStatement = (ForStatementSyntax)context.Node;

            StatementSyntax statement = forStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            if (statement.IsSingleLine() && FormattingSupportsEmbeddedStatement(forStatement))
                return;

            ReportDiagnostic(context, forStatement, statement);
        }

        private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var usingStatement = (UsingStatementSyntax)context.Node;

            StatementSyntax statement = usingStatement.EmbeddedStatement(allowUsingStatement: false);

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            if (statement.IsSingleLine() && FormattingSupportsEmbeddedStatement(usingStatement))
                return;

            ReportDiagnostic(context, usingStatement, statement);
        }

        private static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
        {
            var whileStatement = (WhileStatementSyntax)context.Node;

            StatementSyntax statement = whileStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            if (statement.IsSingleLine() && FormattingSupportsEmbeddedStatement(whileStatement))
                return;

            ReportDiagnostic(context, whileStatement, statement);
        }

        private static void AnalyzeDoStatement(SyntaxNodeAnalysisContext context)
        {
            var doStatement = (DoStatementSyntax)context.Node;

            StatementSyntax statement = doStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            if (statement.IsSingleLine() && FormattingSupportsEmbeddedStatement(doStatement))
                return;

            ReportDiagnostic(context, doStatement, statement);
        }

        private static void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
        {
            var lockStatement = (LockStatementSyntax)context.Node;

            StatementSyntax statement = lockStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            if (statement.IsSingleLine() && FormattingSupportsEmbeddedStatement(lockStatement))
                return;

            ReportDiagnostic(context, lockStatement, statement);
        }

        private static void AnalyzeFixedStatement(SyntaxNodeAnalysisContext context)
        {
            var fixedStatement = (FixedStatementSyntax)context.Node;

            StatementSyntax statement = fixedStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            if (statement.IsSingleLine() && FormattingSupportsEmbeddedStatement(fixedStatement))
                return;

            ReportDiagnostic(context, fixedStatement, statement);
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, StatementSyntax statement, StatementSyntax embeddedStatement)
        {
            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.AddBracesWhenExpressionSpansOverMultipleLines,
                embeddedStatement,
                CSharpFacts.GetTitle(statement));
        }
    }
}
