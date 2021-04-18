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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UsingDirectiveCodeFixProvider))]
    [Shared]
    public sealed class UsingDirectiveCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.AvoidUsageOfUsingAliasDirective); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out UsingDirectiveSyntax usingDirective))
                return;

            CodeAction codeAction = CodeAction.Create(
                "Inline alias expression",
                cancellationToken => AvoidUsageOfUsingAliasDirectiveRefactoring.RefactorAsync(context.Document, usingDirective, cancellationToken),
                GetEquivalenceKey(DiagnosticIdentifiers.AvoidUsageOfUsingAliasDirective));

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}
