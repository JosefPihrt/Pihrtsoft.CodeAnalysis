// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FinallyClauseCodeFixProvider))]
    [Shared]
    public sealed class FinallyClauseCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.RemoveEmptyFinallyClause); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out FinallyClauseSyntax finallyClause))
                return;

            CodeAction codeAction = CodeAction.Create(
                "Remove empty 'finally' clause",
                ct => RemoveEmptyFinallyClauseAsync(context.Document, finallyClause, ct),
                GetEquivalenceKey(DiagnosticIdentifiers.RemoveEmptyFinallyClause));

            context.RegisterCodeFix(codeAction, context.Diagnostics[0]);
        }

        private static async Task<Document> RemoveEmptyFinallyClauseAsync(
            Document document,
            FinallyClauseSyntax finallyClause,
            CancellationToken cancellationToken)
        {
            var tryStatement = (TryStatementSyntax)finallyClause.Parent;

            SyntaxList<CatchClauseSyntax> catches = tryStatement.Catches;

            if (catches.Any())
            {
                if (finallyClause.GetLeadingTrivia().IsEmptyOrWhitespace())
                {
                    CatchClauseSyntax lastCatch = catches.Last();

                    if (lastCatch.GetTrailingTrivia().IsEmptyOrWhitespace())
                    {
                        TryStatementSyntax newTryStatement = tryStatement
                            .WithCatches(catches.Replace(lastCatch, lastCatch.WithTrailingTrivia(finallyClause.GetTrailingTrivia())))
                            .WithFinally(null);

                        return await document.ReplaceNodeAsync(tryStatement, newTryStatement, cancellationToken).ConfigureAwait(false);
                    }
                }

                return await document.RemoveNodeAsync(finallyClause, SyntaxRemoveOptions.KeepExteriorTrivia, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                IEnumerable<StatementSyntax> newNodes = tryStatement
                    .Block
                    .Statements
                    .Select(f => f.WithFormatterAnnotation());

                return await document.ReplaceNodeAsync(tryStatement, newNodes, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
