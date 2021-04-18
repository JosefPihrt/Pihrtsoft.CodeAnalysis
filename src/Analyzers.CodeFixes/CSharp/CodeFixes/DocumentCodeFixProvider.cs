// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DocumentCodeFixProvider))]
    [Shared]
    public sealed class DocumentCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.RemoveFileWithNoCode); }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return null;
        }

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.RemoveFileWithNoCode:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove file with no code",
                                cancellationToken =>
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    return RemoveFromSolutionAsync(context.Document);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }

            return Task.CompletedTask;
        }

        public static Task<Solution> RemoveFromSolutionAsync(Document document)
        {
            return Task.FromResult(document.Solution().RemoveDocument(document.Id));
        }
    }
}
