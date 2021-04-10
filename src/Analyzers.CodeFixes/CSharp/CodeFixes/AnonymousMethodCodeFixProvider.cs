// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AnonymousMethodCodeFixProvider))]
    [Shared]
    public sealed class AnonymousMethodCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UseLambdaExpressionInsteadOfAnonymousMethod); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out AnonymousMethodExpressionSyntax anonymousMethod))
                return;

            CodeAction codeAction = CodeAction.Create(
                "Use lambda expression instead of anonymous method",
                cancellationToken => UseLambdaExpressionInsteadOfAnonymousMethodRefactoring.RefactorAsync(context.Document, anonymousMethod, cancellationToken),
                GetEquivalenceKey(DiagnosticIdentifiers.UseLambdaExpressionInsteadOfAnonymousMethod));

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}
