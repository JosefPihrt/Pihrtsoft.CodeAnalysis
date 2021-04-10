// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Analysis;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SimplifyNestedUsingStatementCodeFixProvider))]
    [Shared]
    public sealed class SimplifyNestedUsingStatementCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.SimplifyNestedUsingStatement); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out UsingStatementSyntax usingStatement))
                return;

            CodeAction codeAction = CodeAction.Create(
                "Remove braces",
                cancellationToken => SimplifyNestedUsingStatementRefactoring.RefactorAsync(context.Document, usingStatement, cancellationToken),
                GetEquivalenceKey(DiagnosticIdentifiers.SimplifyNestedUsingStatement));

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}
