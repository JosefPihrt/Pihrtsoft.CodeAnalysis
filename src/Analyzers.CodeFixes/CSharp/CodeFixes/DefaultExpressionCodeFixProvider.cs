// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DefaultExpressionCodeFixProvider))]
    [Shared]
    public sealed class DefaultExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.SimplifyDefaultExpression); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out DefaultExpressionSyntax defaultExpression))
                return;

            Document document = context.Document;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.SimplifyDefaultExpression:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Simplify 'default' expression",
                                ct => SimplifyDefaultExpressionAsync(document, defaultExpression, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static Task<Document> SimplifyDefaultExpressionAsync(
            Document document,
            DefaultExpressionSyntax defaultExpression,
            CancellationToken cancellationToken)
        {
            LiteralExpressionSyntax defaultLiteral = CSharpFactory.DefaultLiteralExpression().WithTrailingTrivia(defaultExpression.GetTrailingTrivia());

            return document.ReplaceNodeAsync(defaultExpression, defaultLiteral, cancellationToken);
        }
    }
}