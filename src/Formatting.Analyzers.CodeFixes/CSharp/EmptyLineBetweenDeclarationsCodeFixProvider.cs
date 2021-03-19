// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Roslynator.Formatting.CSharp;

namespace Roslynator.Formatting.CodeFixes.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EmptyLineBetweenDeclarationsCodeFixProvider))]
    [Shared]
    public class EmptyLineBetweenDeclarationsCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.AddEmptyLineBetweenDeclarations,
                    DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineDeclarations,
                    DiagnosticIdentifiers.AddEmptyLineBetweenDeclarationAndDocumentationComment,
                    DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineDeclarationsOfDifferentKind,
                    DiagnosticIdentifiers.RemoveEmptyLineBetweenSingleLineDeclarationsOfSameKind);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!TryFindTrivia(root, context.Span.Start, out SyntaxTrivia trivia, findInsideTrivia: false))
                return;

            switch (diagnostic.Id)
            {
                case DiagnosticIdentifiers.AddEmptyLineBetweenDeclarations:
                case DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineDeclarations:
                case DiagnosticIdentifiers.AddEmptyLineBetweenDeclarationAndDocumentationComment:
                case DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineDeclarationsOfDifferentKind:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            CodeFixTitles.AddEmptyLine,
                            ct => CodeFixHelpers.AppendEndOfLineAsync(document, trivia.Token, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.RemoveEmptyLineBetweenSingleLineDeclarationsOfSameKind:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            CodeFixTitles.RemoveEmptyLine,
                            ct => CodeFixHelpers.RemoveEmptyLinesBeforeAsync(document, trivia.Token, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }
        }
    }
}
