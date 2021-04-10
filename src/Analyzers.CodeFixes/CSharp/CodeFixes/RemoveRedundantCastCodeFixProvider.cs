// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveRedundantCastCodeFixProvider))]
    [Shared]
    public sealed class RemoveRedundantCastCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.RemoveRedundantCast); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SyntaxNode node, predicate: f => f.IsKind(SyntaxKind.CastExpression, SyntaxKind.InvocationExpression)))
                return;

            switch (node.Kind())
            {
                case SyntaxKind.CastExpression:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            "Remove redundant cast",
                            cancellationToken => RemoveRedundantCastRefactoring.RefactorAsync(context.Document, (CastExpressionSyntax)node, cancellationToken),
                            GetEquivalenceKey(DiagnosticIdentifiers.RemoveRedundantCast));

                        context.RegisterCodeFix(codeAction, context.Diagnostics);
                        break;
                    }
                case SyntaxKind.InvocationExpression:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            "Remove redundant cast",
                            cancellationToken => RemoveRedundantCastRefactoring.RefactorAsync(context.Document, (InvocationExpressionSyntax)node, cancellationToken),
                            GetEquivalenceKey(DiagnosticIdentifiers.RemoveRedundantCast));

                        context.RegisterCodeFix(codeAction, context.Diagnostics);
                        break;
                    }
            }
        }
    }
}