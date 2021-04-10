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

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VariableDeclaratorCodeFixProvider))]
    [Shared]
    public sealed class VariableDeclaratorCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.RemoveRedundantFieldInitialization); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out VariableDeclaratorSyntax declarator))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.RemoveRedundantFieldInitialization:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove redundant initialization",
                                ct => RemoveRedundantFieldInitializationAsync(context.Document, declarator, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static Task<Document> RemoveRedundantFieldInitializationAsync(
            Document document,
            VariableDeclaratorSyntax variableDeclarator,
            CancellationToken cancellationToken)
        {
            EqualsValueClauseSyntax initializer = variableDeclarator.Initializer;

            var removeOptions = SyntaxRemoveOptions.KeepExteriorTrivia;

            if (initializer.GetLeadingTrivia().IsEmptyOrWhitespace())
                removeOptions &= ~SyntaxRemoveOptions.KeepLeadingTrivia;

            if (initializer.GetTrailingTrivia().IsEmptyOrWhitespace())
                removeOptions &= ~SyntaxRemoveOptions.KeepTrailingTrivia;

            VariableDeclaratorSyntax newNode = variableDeclarator
                .RemoveNode(initializer, removeOptions)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(variableDeclarator, newNode, cancellationToken);
        }
    }
}
