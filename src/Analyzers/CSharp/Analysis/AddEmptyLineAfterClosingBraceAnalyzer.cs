// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AddEmptyLineAfterClosingBraceAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddEmptyLineAfterClosingBrace); }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.Initialize(context);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
            context.RegisterSyntaxNodeAction(AnalyzeWhileStatement, SyntaxKind.WhileStatement);
            context.RegisterSyntaxNodeAction(AnalyzeForStatement, SyntaxKind.ForStatement);
            context.RegisterSyntaxNodeAction(AnalyzeCommonForEachStatement, SyntaxKind.ForEachStatement);
            context.RegisterSyntaxNodeAction(AnalyzeCommonForEachStatement, SyntaxKind.ForEachVariableStatement);
            context.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);
            context.RegisterSyntaxNodeAction(AnalyzeFixedStatement, SyntaxKind.FixedStatement);
            context.RegisterSyntaxNodeAction(AnalyzeCheckedStatement, SyntaxKind.CheckedStatement);
            context.RegisterSyntaxNodeAction(AnalyzeCheckedStatement, SyntaxKind.UncheckedStatement);
            context.RegisterSyntaxNodeAction(AnalyzeUnsafeStatement, SyntaxKind.UnsafeStatement);
            context.RegisterSyntaxNodeAction(AnalyzeLockStatement, SyntaxKind.LockStatement);
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
            context.RegisterSyntaxNodeAction(AnalyzeTryStatement, SyntaxKind.TryStatement);
            context.RegisterSyntaxNodeAction(AnalyzeElseClause, SyntaxKind.ElseClause);
        }

        private static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
        {
            var block = (BlockSyntax)context.Node;

            if (block.IsParentKind(SyntaxKind.Block))
                AnalyzeStatement(context, block, block);
        }

        private static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
        {
            var whileStatement = (WhileStatementSyntax)context.Node;

            if (whileStatement.IsParentKind(SyntaxKind.Block))
            {
                StatementSyntax statement = whileStatement.Statement;

                if (statement?.Kind() == SyntaxKind.Block)
                    AnalyzeStatement(context, whileStatement, (BlockSyntax)statement);
            }
        }

        private static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
        {
            var forStatement = (ForStatementSyntax)context.Node;

            if (forStatement.IsParentKind(SyntaxKind.Block))
            {
                StatementSyntax statement = forStatement.Statement;

                if (statement?.Kind() == SyntaxKind.Block)
                    AnalyzeStatement(context, forStatement, (BlockSyntax)statement);
            }
        }

        private static void AnalyzeCommonForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (CommonForEachStatementSyntax)context.Node;

            if (forEachStatement.IsParentKind(SyntaxKind.Block))
            {
                StatementSyntax statement = forEachStatement.Statement;

                if (statement?.Kind() == SyntaxKind.Block)
                    AnalyzeStatement(context, forEachStatement, (BlockSyntax)statement);
            }
        }

        private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var usingStatement = (UsingStatementSyntax)context.Node;

            if (usingStatement.IsParentKind(SyntaxKind.Block))
            {
                StatementSyntax statement = usingStatement.Statement;

                if (statement?.Kind() == SyntaxKind.Block)
                    AnalyzeStatement(context, usingStatement, (BlockSyntax)statement);
            }
        }

        private static void AnalyzeFixedStatement(SyntaxNodeAnalysisContext context)
        {
            var fixedStatement = (FixedStatementSyntax)context.Node;

            if (fixedStatement.IsParentKind(SyntaxKind.Block))
            {
                StatementSyntax statement = fixedStatement.Statement;

                if (statement?.Kind() == SyntaxKind.Block)
                    AnalyzeStatement(context, fixedStatement, (BlockSyntax)statement);
            }
        }

        private static void AnalyzeCheckedStatement(SyntaxNodeAnalysisContext context)
        {
            var checkedStatement = (CheckedStatementSyntax)context.Node;

            if (checkedStatement.IsParentKind(SyntaxKind.Block))
                AnalyzeStatement(context, checkedStatement, checkedStatement.Block);
        }

        private static void AnalyzeUnsafeStatement(SyntaxNodeAnalysisContext context)
        {
            var unsafeStatement = (UnsafeStatementSyntax)context.Node;

            if (unsafeStatement.IsParentKind(SyntaxKind.Block))
                AnalyzeStatement(context, unsafeStatement, unsafeStatement.Block);
        }

        private static void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
        {
            var lockStatement = (LockStatementSyntax)context.Node;

            if (lockStatement.IsParentKind(SyntaxKind.Block))
            {
                StatementSyntax statement = lockStatement.Statement;

                if (statement?.Kind() == SyntaxKind.Block)
                    AnalyzeStatement(context, lockStatement, (BlockSyntax)statement);
            }
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            if (ifStatement.IsParentKind(SyntaxKind.Block))
            {
                StatementSyntax statement = ifStatement.Statement;

                if (statement?.Kind() == SyntaxKind.Block)
                    AnalyzeStatement(context, ifStatement, (BlockSyntax)statement);
            }
        }

        private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var switchStatement = (SwitchStatementSyntax)context.Node;

            if (switchStatement.IsParentKind(SyntaxKind.Block))
                AnalyzeStatement(context, switchStatement, switchStatement.OpenBraceToken, switchStatement.CloseBraceToken);
        }

        private static void AnalyzeTryStatement(SyntaxNodeAnalysisContext context)
        {
            var tryStatement = (TryStatementSyntax)context.Node;

            if (tryStatement.IsParentKind(SyntaxKind.Block))
            {
                FinallyClauseSyntax finallyClause = tryStatement.Finally;

                if (finallyClause != null)
                {
                    AnalyzeStatement(context, tryStatement, finallyClause.Block);
                }
                else
                {
                    CatchClauseSyntax catchClause = tryStatement.Catches.LastOrDefault();

                    if (catchClause != null)
                        AnalyzeStatement(context, tryStatement, catchClause.Block);
                }
            }
        }

        private static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
        {
            var elseClause = (ElseClauseSyntax)context.Node;

            if (elseClause.Statement?.Kind() == SyntaxKind.Block)
            {
                IfStatementSyntax ifStatement = elseClause.GetTopmostIf();

                if (ifStatement.IsParentKind(SyntaxKind.Block))
                {
                    AnalyzeStatement(context, ifStatement, (BlockSyntax)elseClause.Statement);
                }
            }
        }

        private static void AnalyzeStatement(SyntaxNodeAnalysisContext context, StatementSyntax statement, BlockSyntax block)
        {
            if (block?.IsMissing == false)
                AnalyzeStatement(context, statement, block.OpenBraceToken, block.CloseBraceToken);
        }

        private static void AnalyzeStatement(SyntaxNodeAnalysisContext context, StatementSyntax statement, SyntaxToken openBrace, SyntaxToken closeBrace)
        {
            var block = (BlockSyntax)statement.Parent;

            SyntaxList<StatementSyntax> statements = block.Statements;

            int index = statements.IndexOf(statement);

            Debug.Assert(index != -1, "");

            if (index != -1
                && index < statements.Count - 1)
            {
                int startLine = openBrace.GetSpanStartLine(context.CancellationToken);

                int endLine = closeBrace.GetSpanEndLine(context.CancellationToken);

                if (startLine < endLine)
                {
                    StatementSyntax nextStatement = statements[index + 1];

                    if (nextStatement.GetSpanStartLine(context.CancellationToken) - endLine == 1)
                    {
                        SyntaxTrivia trivia = closeBrace
                            .TrailingTrivia
                            .FirstOrDefault(f => f.IsEndOfLineTrivia());

                        if (trivia.IsEndOfLineTrivia())
                        {
                            DiagnosticHelpers.ReportDiagnostic(context,
                                DiagnosticDescriptors.AddEmptyLineAfterClosingBrace,
                                trivia);
                        }
                    }
                }
            }
        }
    }
}
