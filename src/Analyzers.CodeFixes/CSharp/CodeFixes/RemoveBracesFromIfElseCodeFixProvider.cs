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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveBracesFromIfElseCodeFixProvider))]
    [Shared]
    public sealed class RemoveBracesFromIfElseCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.RemoveBracesFromIfElse); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out IfStatementSyntax ifStatement))
                return;

            ifStatement = ifStatement.GetTopmostIf();

            CodeAction codeAction = CodeAction.Create(
                "Remove braces from if-else",
                cancellationToken => RemoveBracesFromIfElseElseRefactoring.RefactorAsync(context.Document, ifStatement, cancellationToken),
                GetEquivalenceKey(DiagnosticIdentifiers.RemoveBracesFromIfElse));

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}