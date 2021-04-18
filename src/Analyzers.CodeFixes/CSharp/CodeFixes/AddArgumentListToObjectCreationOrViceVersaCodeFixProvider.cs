// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddArgumentListToObjectCreationOrViceVersaCodeFixProvider))]
    [Shared]
    public sealed class AddArgumentListToObjectCreationOrViceVersaCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.AddArgumentListToObjectCreationOrViceVersa); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ObjectCreationExpressionSyntax objectCreationExpression))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            if (objectCreationExpression.ArgumentList != null)
            {
                CodeAction codeAction = CodeAction.Create(
                    "Remove argument list",
                    cancellationToken => RemoveArgumentListFromObjectCreationAsync(document, objectCreationExpression.ArgumentList, cancellationToken),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
            else
            {
                CodeAction codeAction = CodeAction.Create(
                    "Add argument list",
                    cancellationToken => AddArgumentListToObjectCreationRefactoring.RefactorAsync(document, objectCreationExpression, cancellationToken),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        private static Task<Document> RemoveArgumentListFromObjectCreationAsync(
            Document document,
            ArgumentListSyntax argumentList,
            CancellationToken cancellationToken)
        {
            return document.RemoveNodeAsync(argumentList, SyntaxRemoveOptions.KeepExteriorTrivia, cancellationToken);
        }
    }
}
