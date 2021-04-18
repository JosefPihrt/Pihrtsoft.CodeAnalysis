// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DuplicateWordInCommentCodeFixProvider))]
    [Shared]
    public sealed class DuplicateWordInCommentCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.DuplicateWordInComment); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindToken(root, context.Span.Start, out SyntaxToken token, findInsideTrivia: true))
                return;

            string s = token.Text;

            int start = context.Span.Start - token.SpanStart;

            while (start < s.Length - 1
                && char.IsWhiteSpace(s[start - 1]))
            {
                start--;
            }

            start += token.SpanStart;

            CodeAction codeAction = CodeAction.Create(
                "Remove duplicate word",
                ct => context.Document.WithTextChangeAsync(TextSpan.FromBounds(start, context.Span.End), "", ct),
                GetEquivalenceKey(DiagnosticIdentifiers.DuplicateWordInComment));

            context.RegisterCodeFix(codeAction, context.Diagnostics[0]);
        }
    }
}