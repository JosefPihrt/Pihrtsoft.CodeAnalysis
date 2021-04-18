// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.Formatting.CSharp;

namespace Roslynator.Formatting.CodeFixes.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DirectiveTriviaCodeFixProvider))]
    [Shared]
    public sealed class DirectiveTriviaCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.AddEmptyLineAfterRegionDirective,
                    DiagnosticIdentifiers.AddEmptyLineBeforeEndRegionDirective);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out DirectiveTriviaSyntax directiveTrivia, findInsideTrivia: true))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            switch (diagnostic.Id)
            {
                case DiagnosticIdentifiers.AddEmptyLineAfterRegionDirective:
                case DiagnosticIdentifiers.AddEmptyLineBeforeEndRegionDirective:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            CodeFixTitles.AddEmptyLine,
                            ct => AddEmptyLineAfterRegionDirectiveAndBeforeEndRegionAsync(document, directiveTrivia, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }
        }

        private static Task<Document> AddEmptyLineAfterRegionDirectiveAndBeforeEndRegionAsync(
            Document document,
            DirectiveTriviaSyntax directiveTrivia,
            CancellationToken cancellationToken)
        {
            switch (directiveTrivia.Kind())
            {
                case SyntaxKind.RegionDirectiveTrivia:
                    return CodeFixHelpers.AddEmptyLineAfterDirectiveAsync(document, directiveTrivia, cancellationToken);
                case SyntaxKind.EndRegionDirectiveTrivia:
                    return CodeFixHelpers.AddEmptyLineBeforeDirectiveAsync(document, directiveTrivia, cancellationToken);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
