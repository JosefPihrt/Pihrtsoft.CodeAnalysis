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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddBracesToIfElseCodeFixProvider))]
    [Shared]
    public sealed class AddBracesToIfElseCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.AddBracesToIfElseWhenExpressionSpansOverMultipleLines); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out IfStatementSyntax ifStatement))
                return;

            ifStatement = ifStatement.GetTopmostIf();

            CodeAction codeAction = CodeAction.Create(
                "Add braces to if-else",
                cancellationToken => AddBracesToIfElseRefactoring.RefactorAsync(context.Document, ifStatement, cancellationToken),
                GetEquivalenceKey(DiagnosticIdentifiers.AddBracesToIfElseWhenExpressionSpansOverMultipleLines));

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}