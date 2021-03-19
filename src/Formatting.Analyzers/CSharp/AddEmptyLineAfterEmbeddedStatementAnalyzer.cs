// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AddEmptyLineAfterEmbeddedStatementAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddEmptyLineAfterEmbeddedStatement); }
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
            context.RegisterSyntaxNodeAction(f => AnalyzeLockStatement(f), SyntaxKind.LockStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeFixedStatement(f), SyntaxKind.FixedStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeElseClause(f), SyntaxKind.ElseClause);
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            if (ifStatement.Else != null)
                return;

            Analyze(context, ifStatement.GetTopmostIf(), ifStatement.CloseParenToken, ifStatement.Statement);
        }

        private static void AnalyzeCommonForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (CommonForEachStatementSyntax)context.Node;

            Analyze(context, forEachStatement, forEachStatement.CloseParenToken, forEachStatement.Statement);
        }

        private static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
        {
            var forStatement = (ForStatementSyntax)context.Node;

            Analyze(context, forStatement, forStatement.CloseParenToken, forStatement.Statement);
        }

        private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var usingStatement = (UsingStatementSyntax)context.Node;

            Analyze(context, usingStatement, usingStatement.CloseParenToken, usingStatement.Statement);
        }

        private static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
        {
            var whileStatement = (WhileStatementSyntax)context.Node;

            Analyze(context, whileStatement, whileStatement.CloseParenToken, whileStatement.Statement);
        }

        private static void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
        {
            var lockStatement = (LockStatementSyntax)context.Node;

            Analyze(context, lockStatement, lockStatement.CloseParenToken, lockStatement.Statement);
        }

        private static void AnalyzeFixedStatement(SyntaxNodeAnalysisContext context)
        {
            var fixedStatement = (FixedStatementSyntax)context.Node;

            Analyze(context, fixedStatement, fixedStatement.CloseParenToken, fixedStatement.Statement);
        }

        private static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
        {
            var elseClause = (ElseClauseSyntax)context.Node;

            StatementSyntax statement = elseClause.Statement;
            SyntaxToken elseKeyword = elseClause.ElseKeyword;

            if (statement?.IsKind(SyntaxKind.Block, SyntaxKind.IfStatement) == false
                && elseClause.SyntaxTree.IsMultiLineSpan(TextSpan.FromBounds(elseKeyword.SpanStart, statement.SpanStart)))
            {
                IfStatementSyntax topmostIf = elseClause.GetTopmostIf();

                if (topmostIf != null)
                    Analyze(context, topmostIf, elseKeyword, statement);
            }
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            StatementSyntax containingStatement,
            SyntaxToken token,
            StatementSyntax statement)
        {
            if (token.IsMissing)
                return;

            if (statement?.IsKind(SyntaxKind.Block, SyntaxKind.EmptyStatement) != false)
                return;

            StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(containingStatement);

            if (!statementsInfo.Success)
                return;

            SyntaxTree syntaxTree = containingStatement.SyntaxTree;

            if (!syntaxTree.IsMultiLineSpan(TextSpan.FromBounds(token.SpanStart, statement.SpanStart)))
                return;

            StatementSyntax nextStatement = containingStatement.NextStatement();

            if (nextStatement == null)
                return;

            if (syntaxTree.GetLineCount(TextSpan.FromBounds(statement.Span.End, nextStatement.SpanStart)) > 2)
                return;

            SyntaxTrivia trivia = statement
                .GetTrailingTrivia()
                .FirstOrDefault(f => f.IsEndOfLineTrivia());

            if (!trivia.IsEndOfLineTrivia())
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticDescriptors.AddEmptyLineAfterEmbeddedStatement,
                Location.Create(syntaxTree, trivia.Span.WithLength(0)));
        }
    }
}
