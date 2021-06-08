// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RemoveRedundantEmptyLineAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveRedundantEmptyLine, CommonDiagnosticRules.AnalyzerOptionIsObsolete);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeClassDeclaration(f), SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeStructDeclaration(f), SyntaxKind.StructDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeInterfaceDeclaration(f), SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeNamespaceDeclaration(f), SyntaxKind.NamespaceDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeSwitchStatement(f), SyntaxKind.SwitchStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeTryStatement(f), SyntaxKind.TryStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeElseClause(f), SyntaxKind.ElseClause);

            context.RegisterSyntaxNodeAction(f => AnalyzeIfStatement(f), SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeCommonForEachStatement(f), SyntaxKind.ForEachStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeCommonForEachStatement(f), SyntaxKind.ForEachVariableStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeForStatement(f), SyntaxKind.ForStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeUsingStatement(f), SyntaxKind.UsingStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeWhileStatement(f), SyntaxKind.WhileStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeDoStatement(f), SyntaxKind.DoStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeLockStatement(f), SyntaxKind.LockStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeFixedStatement(f), SyntaxKind.FixedStatement);

            context.RegisterSyntaxNodeAction(f => AnalyzeAccessorList(f), SyntaxKind.AccessorList);
            context.RegisterSyntaxNodeAction(f => AnalyzeBlock(f), SyntaxKind.Block);
            context.RegisterSyntaxNodeAction(f => AnalyzeSingleLineDocumentationCommentTrivia(f), SyntaxKind.SingleLineDocumentationCommentTrivia);

            context.RegisterSyntaxNodeAction(f => AnalyzeInitializer(f), SyntaxKind.ArrayInitializerExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeInitializer(f), SyntaxKind.CollectionInitializerExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeInitializer(f), SyntaxKind.ObjectInitializerExpression);
        }

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            AnalyzeDeclaration(
                context,
                classDeclaration.Members,
                classDeclaration.OpenBraceToken,
                classDeclaration.CloseBraceToken);
        }

        private static void AnalyzeStructDeclaration(SyntaxNodeAnalysisContext context)
        {
            var structDeclaration = (StructDeclarationSyntax)context.Node;

            AnalyzeDeclaration(
                context,
                structDeclaration.Members,
                structDeclaration.OpenBraceToken,
                structDeclaration.CloseBraceToken);
        }

        private static void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;

            AnalyzeDeclaration(
                context,
                interfaceDeclaration.Members,
                interfaceDeclaration.OpenBraceToken,
                interfaceDeclaration.CloseBraceToken);
        }

        private static void AnalyzeNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var namespaceDeclaration = (NamespaceDeclarationSyntax)context.Node;

            SyntaxList<MemberDeclarationSyntax> members = namespaceDeclaration.Members;
            SyntaxList<ExternAliasDirectiveSyntax> externs = namespaceDeclaration.Externs;

            if (externs.Any())
            {
                AnalyzeStart(context, externs[0], namespaceDeclaration.OpenBraceToken);
            }
            else
            {
                SyntaxList<UsingDirectiveSyntax> usings = namespaceDeclaration.Usings;

                if (usings.Any())
                {
                    AnalyzeStart(context, usings[0], namespaceDeclaration.OpenBraceToken);
                }
                else if (members.Any())
                {
                    AnalyzeStart(context, members[0], namespaceDeclaration.OpenBraceToken);
                }
            }

            if (members.Any())
                AnalyzeEnd(context, members.Last(), namespaceDeclaration.CloseBraceToken);
        }

        private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var switchStatement = (SwitchStatementSyntax)context.Node;

            SyntaxList<SwitchSectionSyntax> sections = switchStatement.Sections;

            if (sections.Any())
            {
                AnalyzeStart(context, sections[0], switchStatement.OpenBraceToken);
                AnalyzeEnd(context, sections.Last(), switchStatement.CloseBraceToken);

                if (sections.Count > 1
                    && AnalyzerOptions.RemoveEmptyLineBetweenClosingBraceAndSwitchSection.IsEnabled(context))
                {
                    SwitchSectionSyntax prevSection = sections[0];

                    for (int i = 1; i < sections.Count; i++)
                    {
                        SwitchSectionSyntax section = sections[i];

                        if (prevSection.Statements.LastOrDefault() is BlockSyntax block)
                            Analyze(context, block.CloseBraceToken, section, AnalyzerOptions.RemoveEmptyLineBetweenClosingBraceAndSwitchSection);

                        prevSection = section;
                    }
                }
            }
        }

        private static void AnalyzeTryStatement(SyntaxNodeAnalysisContext context)
        {
            var tryStatement = (TryStatementSyntax)context.Node;

            BlockSyntax block = tryStatement.Block;

            if (block != null)
            {
                SyntaxList<CatchClauseSyntax> catches = tryStatement.Catches;

                if (catches.Any())
                {
                    SyntaxNode previousNode = block;

                    foreach (CatchClauseSyntax catchClause in catches)
                    {
                        Analyze(context, previousNode, catchClause);

                        previousNode = catchClause;
                    }

                    FinallyClauseSyntax finallyClause = tryStatement.Finally;

                    if (finallyClause != null)
                        Analyze(context, previousNode, finallyClause);
                }
            }
        }

        private static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
        {
            var elseClause = (ElseClauseSyntax)context.Node;

            SyntaxNode parent = elseClause.Parent;

            if (parent is IfStatementSyntax ifStatement)
            {
                StatementSyntax statement = ifStatement.Statement;

                if (statement != null)
                    Analyze(context, statement, elseClause);

                statement = elseClause.Statement;

                if (statement != null)
                    Analyze(context, elseClause.ElseKeyword, statement);
            }
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            AnalyzeEmbeddedStatement(context, ifStatement.CloseParenToken, ifStatement.Statement);
        }

        private static void AnalyzeCommonForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (CommonForEachStatementSyntax)context.Node;

            AnalyzeEmbeddedStatement(context, forEachStatement.CloseParenToken, forEachStatement.Statement);
        }

        private static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
        {
            var forStatement = (ForStatementSyntax)context.Node;

            AnalyzeEmbeddedStatement(context, forStatement.CloseParenToken, forStatement.Statement);
        }

        private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var usingStatement = (UsingStatementSyntax)context.Node;

            AnalyzeEmbeddedStatement(context, usingStatement.CloseParenToken, usingStatement.Statement);
        }

        private static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
        {
            var whileStatement = (WhileStatementSyntax)context.Node;

            AnalyzeEmbeddedStatement(context, whileStatement.CloseParenToken, whileStatement.Statement);
        }

        private static void AnalyzeDoStatement(SyntaxNodeAnalysisContext context)
        {
            var doStatement = (DoStatementSyntax)context.Node;

            AnalyzeEmbeddedStatement(context, doStatement.DoKeyword, doStatement.Statement);
        }

        private static void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
        {
            var lockStatement = (LockStatementSyntax)context.Node;

            AnalyzeEmbeddedStatement(context, lockStatement.CloseParenToken, lockStatement.Statement);
        }

        private static void AnalyzeFixedStatement(SyntaxNodeAnalysisContext context)
        {
            var fixedStatement = (FixedStatementSyntax)context.Node;

            AnalyzeEmbeddedStatement(context, fixedStatement.CloseParenToken, fixedStatement.Statement);
        }

        private static void AnalyzeEmbeddedStatement(SyntaxNodeAnalysisContext context, SyntaxToken token, StatementSyntax statement)
        {
            if (statement?.IsEmbedded() == true)
                Analyze(context, token, statement);
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            SyntaxNode node1,
            SyntaxNode node2)
        {
            SyntaxTriviaList trailingTrivia = node1.GetTrailingTrivia();
            SyntaxTriviaList leadingTrivia = node2.GetLeadingTrivia();

            if (IsStandardTriviaBetweenLines(trailingTrivia, leadingTrivia))
                return;

            if (node1
                .SyntaxTree
                .GetLineSpan(TextSpan.FromBounds(node1.Span.End, node2.SpanStart), context.CancellationToken)
                .GetLineCount() != 3)
            {
                return;
            }

            var trivia = default(SyntaxTrivia);

            foreach (SyntaxTrivia t in leadingTrivia)
            {
                if (!t.IsWhitespaceTrivia())
                {
                    trivia = t;
                    break;
                }
            }

            if (!trivia.IsEndOfLineTrivia())
                return;

            if (!trailingTrivia.IsEmptyOrWhitespace())
                return;

            if (!leadingTrivia.IsEmptyOrWhitespace())
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveRedundantEmptyLine,
                Location.Create(node1.SyntaxTree, TextSpan.FromBounds(node2.FullSpan.Start, trivia.Span.End)));
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            SyntaxToken token,
            SyntaxNode node,
            AnalyzerOptionDescriptor obsoleteAnalyzerOption = default)
        {
            SyntaxTriviaList trailingTrivia = token.TrailingTrivia;
            SyntaxTriviaList leadingTrivia = node.GetLeadingTrivia();

            if (IsStandardTriviaBetweenLines(trailingTrivia, leadingTrivia))
                return;

            if (token
                .SyntaxTree
                .GetLineSpan(TextSpan.FromBounds(token.Span.End, node.SpanStart), context.CancellationToken)
                .GetLineCount() != 3)
            {
                return;
            }

            var trivia = default(SyntaxTrivia);

            foreach (SyntaxTrivia t in leadingTrivia)
            {
                if (!t.IsWhitespaceTrivia())
                {
                    trivia = t;
                    break;
                }
            }

            if (!trivia.IsEndOfLineTrivia())
                return;

            if (!trailingTrivia.IsEmptyOrWhitespace())
                return;

            if (!leadingTrivia.IsEmptyOrWhitespace())
                return;

            Location location = Location.Create(token.SyntaxTree, TextSpan.FromBounds(node.FullSpan.Start, trivia.Span.End));

            if (obsoleteAnalyzerOption.IsDefault)
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveRedundantEmptyLine, location);
            }
            else
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveRedundantEmptyLine, location, obsoleteAnalyzerOption);
            }
        }

        private static void AnalyzeDeclaration(
            SyntaxNodeAnalysisContext context,
            SyntaxList<MemberDeclarationSyntax> members,
            SyntaxToken openBrace,
            SyntaxToken closeBrace)
        {
            if (members.Any())
            {
                AnalyzeStart(context, members[0], openBrace);
                AnalyzeEnd(context, members.Last(), closeBrace);
            }
            else
            {
                AnalyzeEmptyBraces(context, openBrace, closeBrace);
            }
        }

        private static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
        {
            var block = (BlockSyntax)context.Node;

            SyntaxList<StatementSyntax> statements = block.Statements;

            if (statements.Any())
            {
                AnalyzeStart(context, statements[0], block.OpenBraceToken);
                AnalyzeEnd(context, statements.Last(), block.CloseBraceToken);
            }
            else
            {
                AnalyzeEmptyBraces(context, block.OpenBraceToken, block.CloseBraceToken);
            }
        }

        private static void AnalyzeSingleLineDocumentationCommentTrivia(SyntaxNodeAnalysisContext context)
        {
            var comment = (DocumentationCommentTriviaSyntax)context.Node;

            if (comment is IStructuredTriviaSyntax structuredTrivia)
            {
                SyntaxTrivia trivia = structuredTrivia.ParentTrivia;
                SyntaxTriviaList leadingTrivia = trivia.Token.LeadingTrivia;

                int index = leadingTrivia.IndexOf(trivia);

                if (index >= 0
                    && index < leadingTrivia.Count - 1
                    && leadingTrivia[index + 1].IsEndOfLineTrivia())
                {
                    DiagnosticHelpers.ReportDiagnostic(
                        context,
                        DiagnosticRules.RemoveRedundantEmptyLine,
                        leadingTrivia[index + 1].GetLocation());
                }
            }
        }

        private static void AnalyzeInitializer(SyntaxNodeAnalysisContext context)
        {
            var initializer = (InitializerExpressionSyntax)context.Node;

            SeparatedSyntaxList<ExpressionSyntax> expressions = initializer.Expressions;

            ExpressionSyntax first = expressions.FirstOrDefault();

            if (first == null)
                return;

            if (IsExpectedTrailingTrivia(initializer.OpenBraceToken.TrailingTrivia))
            {
                SyntaxTriviaList leading = first.GetLeadingTrivia();

                if (leading.Any())
                {
                    TextSpan? span = GetEmptyLineSpan(leading, isEnd: false);

                    if (span != null)
                        ReportDiagnostic(context, span.Value);
                }
            }

            if (IsExpectedTrailingTrivia(expressions.GetTrailingTrivia()))
            {
                SyntaxTriviaList leading = initializer.CloseBraceToken.LeadingTrivia;

                if (leading.Any())
                {
                    TextSpan? span = GetEmptyLineSpan(leading, isEnd: true);

                    if (span != null)
                        ReportDiagnostic(context, span.Value);
                }
            }

            static bool IsExpectedTrailingTrivia(SyntaxTriviaList triviaList)
            {
                foreach (SyntaxTrivia trivia in triviaList)
                {
                    switch (trivia.Kind())
                    {
                        case SyntaxKind.WhitespaceTrivia:
                            break;
                        case SyntaxKind.EndOfLineTrivia:
                            return true;
                        default:
                            return false;
                    }
                }

                return false;
            }
        }

        private static void AnalyzeAccessorList(SyntaxNodeAnalysisContext context)
        {
            var accessorList = (AccessorListSyntax)context.Node;

            SyntaxList<AccessorDeclarationSyntax> accessors = accessorList.Accessors;

            if (accessors.Any())
            {
                AnalyzeStart(context, accessors[0], accessorList.OpenBraceToken);
                AnalyzeEnd(context, accessors.Last(), accessorList.CloseBraceToken);
            }
        }

        private static void AnalyzeStart(
            SyntaxNodeAnalysisContext context,
            SyntaxNode node,
            SyntaxToken brace)
        {
            if (brace.IsMissing)
                return;

            if ((node.GetSpanStartLine() - brace.GetSpanEndLine()) <= 1)
                return;

            TextSpan? span = GetEmptyLineSpan(node.GetLeadingTrivia(), isEnd: false);

            if (span == null)
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveRedundantEmptyLine,
                Location.Create(context.Node.SyntaxTree, span.Value));
        }

        private static void AnalyzeEnd(
            SyntaxNodeAnalysisContext context,
            SyntaxNode node,
            SyntaxToken brace)
        {
            if (brace.IsMissing)
                return;

            int braceLine = brace.GetSpanStartLine();

            if (braceLine - node.GetSpanEndLine() <= 1)
                return;

            TextSpan? span = GetEmptyLineSpan(brace.LeadingTrivia, isEnd: true);

            if (span == null)
                return;

            if (IsEmptyLastLineInDoStatement(node, braceLine, span.Value))
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveRedundantEmptyLine,
                Location.Create(context.Node.SyntaxTree, span.Value));
        }

        private static void AnalyzeEmptyBraces(
            SyntaxNodeAnalysisContext context,
            SyntaxToken openBrace,
            SyntaxToken closeBrace)
        {
            if (openBrace.IsMissing)
                return;

            if (closeBrace.IsMissing)
                return;

            SyntaxTree tree = context.Node.SyntaxTree;

            if (tree.GetLineCount(TextSpan.FromBounds(openBrace.SpanStart, closeBrace.Span.End)) <= 2)
                return;

            TextSpan? span = GetEmptyLineSpan(closeBrace.LeadingTrivia, isEnd: true);

            if (span == null)
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveRedundantEmptyLine,
                Location.Create(tree, span.Value));
        }

        private static bool IsEmptyLastLineInDoStatement(
            SyntaxNode node,
            int closeBraceLine,
            TextSpan span)
        {
            SyntaxNode parent = node.Parent;

            if (parent?.Kind() != SyntaxKind.Block)
                return false;

            parent = parent.Parent;

            if (!(parent is DoStatementSyntax doStatement))
                return false;

            int emptyLine = doStatement.SyntaxTree.GetLineSpan(span).EndLine();

            if (emptyLine != closeBraceLine)
                return false;

            int whileKeywordLine = doStatement.WhileKeyword.GetSpanStartLine();

            return closeBraceLine == whileKeywordLine;
        }

        private static TextSpan? GetEmptyLineSpan(
            SyntaxTriviaList triviaList,
            bool isEnd)
        {
            SyntaxTriviaList.Enumerator en = triviaList.GetEnumerator();
            while (en.MoveNext())
            {
                switch (en.Current.Kind())
                {
                    case SyntaxKind.EndOfLineTrivia:
                        {
                            SyntaxTrivia endOfLine = en.Current;

                            if (isEnd)
                            {
                                while (en.MoveNext())
                                {
                                    if (!en.Current.IsWhitespaceOrEndOfLineTrivia())
                                        return null;
                                }
                            }

                            return TextSpan.FromBounds(triviaList.Span.Start, endOfLine.Span.End);
                        }
                    case SyntaxKind.WhitespaceTrivia:
                        {
                            break;
                        }
                    default:
                        {
                            return null;
                        }
                }
            }

            return null;
        }

        private static bool IsStandardTriviaBetweenLines(SyntaxTriviaList trailingTrivia, SyntaxTriviaList leadingTrivia)
        {
            if (leadingTrivia.Any()
                && leadingTrivia.All(f => f.IsWhitespaceTrivia()))
            {
                SyntaxTriviaList.Enumerator en = trailingTrivia.GetEnumerator();

                while (en.MoveNext())
                {
                    SyntaxKind kind = en.Current.Kind();

                    if (kind == SyntaxKind.WhitespaceTrivia)
                        continue;

                    return kind == SyntaxKind.EndOfLineTrivia
                        && !en.MoveNext();
                }
            }

            return false;
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, TextSpan span)
        {
            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveRedundantEmptyLine,
                Location.Create(context.Node.SyntaxTree, span));
        }
    }
}
