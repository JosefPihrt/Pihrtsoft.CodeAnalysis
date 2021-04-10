// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = nameof(WhitespaceTriviaCodeFixProvider))]
    [Shared]
    public sealed class WhitespaceTriviaCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.RemoveTrailingWhitespace,
                    DiagnosticIdentifiers.RemoveRedundantEmptyLine);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            TextSpan span = context.Span;

            if (!root.FindTrivia(span.Start).IsWhitespaceOrEndOfLineTrivia()
                && !root.FindToken(span.Start, findInsideTrivia: true).IsKind(SyntaxKind.XmlTextLiteralToken))
            {
                Debug.Fail("");
                return;
            }

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.RemoveTrailingWhitespace:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove trailing white-space",
                                ct => context.Document.WithTextChangeAsync(span, "", ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.RemoveRedundantEmptyLine:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove empty line",
                                ct => context.Document.WithTextChangeAsync(span, "", ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }
    }
}
