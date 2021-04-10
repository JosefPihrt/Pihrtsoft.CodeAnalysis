// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ElseClauseCodeFixProvider))]
    [Shared]
    public sealed class ElseClauseCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.RemoveEmptyElseClause,
                    DiagnosticIdentifiers.MergeElseWithNestedIf);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ElseClauseSyntax elseClause))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.RemoveEmptyElseClause:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove empty 'else' clause",
                                ct => RemoveEmptyElseClauseAsync(context.Document, elseClause, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.MergeElseWithNestedIf:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove braces",
                                ct => MergeElseWithNestedIfAsync(context.Document, elseClause, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static async Task<Document> RemoveEmptyElseClauseAsync(
            Document document,
            ElseClauseSyntax elseClause,
            CancellationToken cancellationToken)
        {
            if (elseClause.IsParentKind(SyntaxKind.IfStatement))
            {
                var ifStatement = (IfStatementSyntax)elseClause.Parent;
                StatementSyntax statement = ifStatement.Statement;

                if (statement?.GetTrailingTrivia().IsEmptyOrWhitespace() == true)
                {
                    IfStatementSyntax newIfStatement = ifStatement
                        .WithStatement(statement.WithTrailingTrivia(elseClause.GetTrailingTrivia()))
                        .WithElse(null);

                    return await document.ReplaceNodeAsync(ifStatement, newIfStatement, cancellationToken).ConfigureAwait(false);
                }
            }

            return await document.RemoveNodeAsync(elseClause, SyntaxRemoveOptions.KeepExteriorTrivia, cancellationToken).ConfigureAwait(false);
        }

        private static Task<Document> MergeElseWithNestedIfAsync(
            Document document,
            ElseClauseSyntax elseClause,
            CancellationToken cancellationToken = default)
        {
            var block = (BlockSyntax)elseClause.Statement;

            var ifStatement = (IfStatementSyntax)block.Statements[0];

            ElseClauseSyntax newElseClause = elseClause
                .WithStatement(ifStatement)
                .WithElseKeyword(elseClause.ElseKeyword.WithoutTrailingTrivia())
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(elseClause, newElseClause, cancellationToken);
        }
    }
}
