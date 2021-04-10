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
    public sealed class AddBracesAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AddBraces);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeIfStatement(f), SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeElseClause(f), SyntaxKind.ElseClause);
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

            StatementSyntax statement = ifStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AddBraces, statement, CSharpFacts.GetTitle(ifStatement));
        }

        private static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
        {
            var elseClause = (ElseClauseSyntax)context.Node;

            StatementSyntax statement = elseClause.EmbeddedStatement(allowIfStatement: false);

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AddBraces, statement, CSharpFacts.GetTitle(elseClause));
        }

        private static void AnalyzeCommonForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (CommonForEachStatementSyntax)context.Node;

            StatementSyntax statement = forEachStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AddBraces, statement, CSharpFacts.GetTitle(forEachStatement));
        }

        private static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
        {
            var forStatement = (ForStatementSyntax)context.Node;

            StatementSyntax statement = forStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AddBraces, statement, CSharpFacts.GetTitle(forStatement));
        }

        private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var usingStatement = (UsingStatementSyntax)context.Node;

            StatementSyntax statement = usingStatement.EmbeddedStatement(allowUsingStatement: false);

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AddBraces, statement, CSharpFacts.GetTitle(usingStatement));
        }

        private static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
        {
            var whileStatement = (WhileStatementSyntax)context.Node;

            StatementSyntax statement = whileStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AddBraces, statement, CSharpFacts.GetTitle(whileStatement));
        }

        private static void AnalyzeDoStatement(SyntaxNodeAnalysisContext context)
        {
            var doStatement = (DoStatementSyntax)context.Node;

            StatementSyntax statement = doStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AddBraces, statement, CSharpFacts.GetTitle(doStatement));
        }

        private static void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
        {
            var lockStatement = (LockStatementSyntax)context.Node;

            StatementSyntax statement = lockStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AddBraces, statement, CSharpFacts.GetTitle(lockStatement));
        }

        private static void AnalyzeFixedStatement(SyntaxNodeAnalysisContext context)
        {
            var fixedStatement = (FixedStatementSyntax)context.Node;

            StatementSyntax statement = fixedStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AddBraces, statement, CSharpFacts.GetTitle(fixedStatement));
        }
    }
}
