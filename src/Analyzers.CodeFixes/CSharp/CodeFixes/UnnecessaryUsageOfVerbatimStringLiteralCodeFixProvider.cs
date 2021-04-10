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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnnecessaryUsageOfVerbatimStringLiteralCodeFixProvider))]
    [Shared]
    public sealed class UnnecessaryUsageOfVerbatimStringLiteralCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Remove '@'";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UnnecessaryUsageOfVerbatimStringLiteral); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SyntaxNode node, predicate: f => f.IsKind(SyntaxKind.StringLiteralExpression, SyntaxKind.InterpolatedStringExpression)))
                return;

            switch (node.Kind())
            {
                case SyntaxKind.StringLiteralExpression:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            Title,
                            cancellationToken => UseRegularStringLiteralInsteadOfVerbatimStringLiteralRefactoring.RefactorAsync(context.Document, (LiteralExpressionSyntax)node, cancellationToken),
                            GetEquivalenceKey(DiagnosticIdentifiers.UnnecessaryUsageOfVerbatimStringLiteral));

                        context.RegisterCodeFix(codeAction, context.Diagnostics);
                        break;
                    }
                case SyntaxKind.InterpolatedStringExpression:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            Title,
                            cancellationToken => UseRegularStringLiteralInsteadOfVerbatimStringLiteralRefactoring.RefactorAsync(context.Document, (InterpolatedStringExpressionSyntax)node, cancellationToken),
                            GetEquivalenceKey(DiagnosticIdentifiers.UnnecessaryUsageOfVerbatimStringLiteral));

                        context.RegisterCodeFix(codeAction, context.Diagnostics);
                        break;
                    }
            }
        }
    }
}