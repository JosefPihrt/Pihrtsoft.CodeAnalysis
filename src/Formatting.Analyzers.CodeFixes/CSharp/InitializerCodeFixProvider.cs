// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using Roslynator.Formatting.CSharp;

namespace Roslynator.Formatting.CodeFixes.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InitializerCodeFixProvider))]
    [Shared]
    public sealed class InitializerCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out InitializerExpressionSyntax initializer))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            switch (diagnostic.Id)
            {
                case DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            CodeFixTitles.RemoveNewLines,
                            ct => SyntaxFormatter.ToSingleLineAsync(document, initializer, removeTrailingComma: true, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }
        }
    }
}
