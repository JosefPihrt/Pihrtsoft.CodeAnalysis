// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;

namespace Roslynator.Formatting.CSharp
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
            context.RegisterSyntaxNodeAction(f => AnalyzeElseClause(f), SyntaxKind.ElseClause);
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
                && statement.SyntaxTree.IsSingleLineSpan(TextSpan.FromBounds(token.SpanStart, statement.SpanStart)))
            {
                ReportDiagnostic(context, statement);
            }
        }

        private static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
        {
            var elseClause = (ElseClauseSyntax)context.Node;

            StatementSyntax statement = elseClause.Statement;

            if (statement?.IsKind(SyntaxKind.Block, SyntaxKind.IfStatement) == false
                && elseClause.SyntaxTree.IsSingleLineSpan(TextSpan.FromBounds(elseClause.ElseKeyword.SpanStart, statement.SpanStart)))
            {
                ReportDiagnostic(context, statement);
            }
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, StatementSyntax statement)
        {
            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticDescriptors.AddNewLineBeforeEmbeddedStatement,
                Location.Create(statement.SyntaxTree, statement.Span.WithLength(0)));
        }
    }
}
