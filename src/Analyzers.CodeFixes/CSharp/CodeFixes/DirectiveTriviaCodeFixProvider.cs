// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeFixes;
using Roslynator.Text;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DirectiveTriviaCodeFixProvider))]
    [Shared]
    public sealed class DirectiveTriviaCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.MergePreprocessorDirectives); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out DirectiveTriviaSyntax directive, findInsideTrivia: true))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                "Merge directives",
                cancellationToken => RefactorAsync(context.Document, (PragmaWarningDirectiveTriviaSyntax)directive, cancellationToken),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static Task<Document> RefactorAsync(
            Document document,
            PragmaWarningDirectiveTriviaSyntax directive,
            CancellationToken cancellationToken)
        {
            SyntaxTrivia trivia = directive.ParentTrivia;

            SyntaxTriviaList list = trivia.GetContainingList();

            int index = list.IndexOf(trivia);

            int start = directive.EndOfDirectiveToken.SpanStart;

            StringBuilder sb = StringBuilderCache.GetInstance();

            int i = index + 1;

            SyntaxKind disableOrRestoreKind = directive.DisableOrRestoreKeyword.Kind();

            int end = start;

            bool addComma = !directive.ErrorCodes.HasTrailingSeparator();

            while (i < list.Count)
            {
                SyntaxTrivia trivia2 = list[i];

                if (trivia2.IsWhitespaceOrEndOfLineTrivia())
                {
                    i++;
                    continue;
                }

                if (trivia2.GetStructure() is PragmaWarningDirectiveTriviaSyntax directive2
                    && disableOrRestoreKind == directive2.DisableOrRestoreKeyword.Kind())
                {
                    if (addComma)
                        sb.Append(",");

                    sb.Append(" ");

                    SeparatedSyntaxList<ExpressionSyntax> errorCodes = directive2.ErrorCodes;
                    sb.Append(errorCodes.ToString());

                    addComma = !errorCodes.HasTrailingSeparator();

                    end = directive2.ErrorCodes.Span.End;
                }

                i++;
            }

            return document.WithTextChangeAsync(
                TextSpan.FromBounds(start, end),
                StringBuilderCache.GetStringAndFree(sb),
                cancellationToken);
        }
    }
}