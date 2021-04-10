// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Formatting.CSharp;

namespace Roslynator.Formatting.CodeFixes.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReplaceTabWithSpacesCodeFixProvider))]
    [Shared]
    public sealed class ReplaceTabWithSpacesCodeFixProvider : BaseCodeFixProvider
    {
        public ReplaceTabWithSpacesCodeFixProvider()
        {
            TwoSpacesEquivalenceKey = GetEquivalenceKey(DiagnosticIdentifiers.UseSpacesInsteadOfTab, "TwoSpaces");
            FourSpacesEquivalenceKey = GetEquivalenceKey(DiagnosticIdentifiers.UseSpacesInsteadOfTab, "FourSpaces");
        }

        internal string TwoSpacesEquivalenceKey { get; }

        internal string FourSpacesEquivalenceKey { get; }

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UseSpacesInsteadOfTab); }
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                "Replace tab with 2 spaces",
                ct => UseSpacesInsteadOfTabAsync(document, context.Span, 2, ct),
                TwoSpacesEquivalenceKey);

            context.RegisterCodeFix(codeAction, diagnostic);

            codeAction = CodeAction.Create(
                "Replace tab with 4 spaces",
                ct => UseSpacesInsteadOfTabAsync(document, context.Span, 4, ct),
                FourSpacesEquivalenceKey);

            context.RegisterCodeFix(codeAction, diagnostic);

            return Task.CompletedTask;
        }

        private static Task<Document> UseSpacesInsteadOfTabAsync(
            Document document,
            TextSpan span,
            int numberOfSpaces,
            CancellationToken cancellationToken = default)
        {
            return document.WithTextChangeAsync(
                span,
                new string(' ', span.Length * numberOfSpaces),
                cancellationToken);
        }
    }
}
