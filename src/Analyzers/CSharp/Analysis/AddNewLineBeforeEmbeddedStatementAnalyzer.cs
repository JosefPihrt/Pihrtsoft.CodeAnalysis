// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AddNewLineBeforeEmbeddedStatementAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddNewLineBeforeEmbeddedStatement); }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.Initialize(context);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(AnalyzeCommonForEachStatement, SyntaxKind.ForEachStatement);
            context.RegisterSyntaxNodeAction(AnalyzeCommonForEachStatement, SyntaxKind.ForEachVariableStatement);
            context.RegisterSyntaxNodeAction(AnalyzeForStatement, SyntaxKind.ForStatement);
            context.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);
            context.RegisterSyntaxNodeAction(AnalyzeWhileStatement, SyntaxKind.WhileStatement);
            context.RegisterSyntaxNodeAction(AnalyzeDoStatement, SyntaxKind.DoStatement);
            context.RegisterSyntaxNodeAction(AnalyzeLockStatement, SyntaxKind.LockStatement);
            context.RegisterSyntaxNodeAction(AnalyzeFixedStatement, SyntaxKind.FixedStatement);
            context.RegisterSyntaxNodeAction(AnalyzeElseClause, SyntaxKind.ElseClause);
        }

        internal static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            Analyze(context, ifStatement.CloseParenToken, ifStatement.Statement);
        }

        internal static void AnalyzeCommonForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (CommonForEachStatementSyntax)context.Node;

            Analyze(context, forEachStatement.CloseParenToken, forEachStatement.Statement);
        }

        internal static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
        {
            var forStatement = (ForStatementSyntax)context.Node;

            Analyze(context, forStatement.CloseParenToken, forStatement.Statement);
        }

        internal static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var usingStatement = (UsingStatementSyntax)context.Node;

            Analyze(context, usingStatement.CloseParenToken, usingStatement.Statement);
        }

        internal static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
        {
            var whileStatement = (WhileStatementSyntax)context.Node;

            Analyze(context, whileStatement.CloseParenToken, whileStatement.Statement);
        }

        internal static void AnalyzeDoStatement(SyntaxNodeAnalysisContext context)
        {
            var doStatement = (DoStatementSyntax)context.Node;

            Analyze(context, doStatement.DoKeyword, doStatement.Statement);
        }

        internal static void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
        {
            var lockStatement = (LockStatementSyntax)context.Node;

            Analyze(context, lockStatement.CloseParenToken, lockStatement.Statement);
        }

        internal static void AnalyzeFixedStatement(SyntaxNodeAnalysisContext context)
        {
            var fixedStatement = (FixedStatementSyntax)context.Node;

            Analyze(context, fixedStatement.CloseParenToken, fixedStatement.Statement);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, SyntaxToken token, StatementSyntax statement)
        {
            if (!token.IsMissing
                && statement?.IsKind(SyntaxKind.Block, SyntaxKind.EmptyStatement) == false
                && statement.SyntaxTree.IsSingleLineSpan(TextSpan.FromBounds(token.SpanStart, statement.SpanStart), context.CancellationToken))
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.AddNewLineBeforeEmbeddedStatement, statement);
            }
        }

        public static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
        {
            var elseClause = (ElseClauseSyntax)context.Node;

            StatementSyntax statement = elseClause.Statement;

            if (statement?.IsKind(SyntaxKind.Block, SyntaxKind.IfStatement) == false
                && elseClause.SyntaxTree.IsSingleLineSpan(TextSpan.FromBounds(elseClause.ElseKeyword.SpanStart, statement.SpanStart), context.CancellationToken))
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.AddNewLineBeforeEmbeddedStatement, statement);
            }
        }
    }
}
