// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnreachableCodeCodeFixProvider))]
    [Shared]
    public sealed class UnreachableCodeCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Remove unreachable code";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.UnreachableCodeDetected); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveUnreachableCode))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out StatementSyntax statement))
                return;

            Debug.Assert(context.Span.Start == statement.SpanStart, statement.ToString());

            if (context.Span.Start != statement.SpanStart)
                return;

            CodeAction codeAction = CreateCodeActionForIfElse(context.Document, diagnostic, statement.Parent);

            if (codeAction != null)
            {
                context.RegisterCodeFix(codeAction, diagnostic);
                return;
            }

            StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(statement);
            if (statementsInfo.Success)
            {
                codeAction = CodeAction.Create(
                    Title,
                    cancellationToken =>
                    {
                        SyntaxList<StatementSyntax> statements = statementsInfo.Statements;

                        int index = statements.IndexOf(statement);

                        if (index == statements.Count - 1)
                        {
                            return context.Document.RemoveStatementAsync(statement, cancellationToken);
                        }
                        else
                        {
                            int lastIndex = statements.LastIndexOf(f => !f.IsKind(SyntaxKind.LocalFunctionStatement));

                            SyntaxList<StatementSyntax> nodes = RemoveRange(statements, index, lastIndex - index + 1, f => !f.IsKind(SyntaxKind.LocalFunctionStatement));

                            return context.Document.ReplaceStatementsAsync(
                                statementsInfo,
                                nodes,
                                cancellationToken);
                        }
                    },
                    base.GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        private CodeAction CreateCodeActionForIfElse(Document document, Diagnostic diagnostic, SyntaxNode node)
        {
            switch (node?.Kind())
            {
                case SyntaxKind.IfStatement:
                    {
                        var ifStatement = (IfStatementSyntax)node;

                        ElseClauseSyntax elseClause = ifStatement.Else;

                        if (elseClause != null
                            && ifStatement.IsParentKind(SyntaxKind.ElseClause))
                        {
                            return CodeAction.Create(
                                Title,
                                cz => document.ReplaceNodeAsync(ifStatement.Parent, elseClause, cz),
                                GetEquivalenceKey(diagnostic));
                        }

                        StatementSyntax statement = elseClause?.Statement;

                        if (statement != null)
                        {
                            if (statement is BlockSyntax block)
                            {
                                SyntaxList<StatementSyntax> statements = block.Statements;

                                if (statements.Any())
                                {
                                    return CreateCodeAction(document, diagnostic, ifStatement, statements);
                                }
                            }
                            else
                            {
                                return CreateCodeAction(document, diagnostic, ifStatement, statement);
                            }
                        }

                        return CodeAction.Create(
                            Title,
                            cancellationToken => document.RemoveStatementAsync(ifStatement, cancellationToken),
                            GetEquivalenceKey(diagnostic));
                    }
                case SyntaxKind.ElseClause:
                    {
                        var elseClause = (ElseClauseSyntax)node;

                        if (elseClause.IsParentKind(SyntaxKind.IfStatement))
                        {
                            var ifStatement = (IfStatementSyntax)elseClause.Parent;

                            if (ifStatement.IsTopmostIf())
                            {
                                StatementSyntax statement = ifStatement.Statement;

                                if (statement != null)
                                {
                                    if (statement is BlockSyntax block)
                                    {
                                        SyntaxList<StatementSyntax> statements = block.Statements;

                                        if (statements.Any())
                                            return CreateCodeAction(document, diagnostic, ifStatement, statements);
                                    }
                                    else
                                    {
                                        return CreateCodeAction(document, diagnostic, ifStatement, statement);
                                    }
                                }
                            }
                        }

                        return CodeAction.Create(
                            Title,
                            cancellationToken => document.RemoveNodeAsync(elseClause, cancellationToken),
                            GetEquivalenceKey(diagnostic));
                    }
                case SyntaxKind.Block:
                    {
                        return CreateCodeActionForIfElse(document, diagnostic, node.Parent);
                    }
            }

            return null;
        }

        private CodeAction CreateCodeAction(Document document, Diagnostic diagnostic, IfStatementSyntax ifStatement, SyntaxList<StatementSyntax> statements)
        {
            return CodeAction.Create(
                Title,
                cancellationToken =>
                {
                    StatementSyntax firstStatement = statements[0];

                    StatementSyntax newFirstStatement = firstStatement
                        .WithLeadingTrivia(ifStatement.GetLeadingTrivia().AddRange(firstStatement.GetLeadingTrivia().EmptyIfWhitespace()));

                    statements = statements.Replace(firstStatement, newFirstStatement);

                    StatementSyntax lastStatement = statements.Last();

                    StatementSyntax newLastStatement = lastStatement
                        .WithTrailingTrivia(lastStatement.GetTrailingTrivia().EmptyIfWhitespace().AddRange(ifStatement.GetTrailingTrivia()));

                    statements = statements.Replace(lastStatement, newLastStatement);

                    return document.ReplaceNodeAsync(ifStatement, statements, cancellationToken);
                },
                GetEquivalenceKey(diagnostic));
        }

        private CodeAction CreateCodeAction(Document document, Diagnostic diagnostic, IfStatementSyntax ifStatement, StatementSyntax statement)
        {
            return CodeAction.Create(
                Title,
                cancellationToken =>
                {
                    StatementSyntax newNode = statement
                        .WithLeadingTrivia(ifStatement.GetLeadingTrivia().AddRange(statement.GetLeadingTrivia().EmptyIfWhitespace()))
                        .WithTrailingTrivia(statement.GetTrailingTrivia().EmptyIfWhitespace().AddRange(ifStatement.GetTrailingTrivia()));

                    return document.ReplaceNodeAsync(ifStatement, newNode, cancellationToken);
                },
                GetEquivalenceKey(diagnostic));
        }

        private SyntaxList<TNode> RemoveRange<TNode>(
            SyntaxList<TNode> list,
            int index,
            int count,
            Func<TNode, bool> predicate) where TNode : SyntaxNode
        {
            return SyntaxFactory.List(RemoveRange());

            IEnumerable<TNode> RemoveRange()
            {
                SyntaxList<TNode>.Enumerator en = list.GetEnumerator();

                int i = 0;

                while (i < index
                    && en.MoveNext())
                {
                    yield return en.Current;
                    i++;
                }

                int endIndex = index + count;

                while (i < endIndex
                    && en.MoveNext())
                {
                    if (!predicate(en.Current))
                        yield return en.Current;

                    i++;
                }

                while (en.MoveNext())
                    yield return en.Current;
            }
        }
    }
}
