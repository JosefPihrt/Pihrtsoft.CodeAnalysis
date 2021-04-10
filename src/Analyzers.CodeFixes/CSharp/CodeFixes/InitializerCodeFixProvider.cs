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

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InitializerCodeFixProvider))]
    [Shared]
    public sealed class InitializerCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.RemoveRedundantCommaInInitializer); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out InitializerExpressionSyntax initializer))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.RemoveRedundantCommaInInitializer:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove redundant comma",
                                ct =>
                                {
                                    ct.ThrowIfCancellationRequested();

                                    InitializerExpressionSyntax newInitializer = RemoveTrailingComma(initializer);

                                    return context.Document.ReplaceNodeAsync(initializer, newInitializer, ct);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static InitializerExpressionSyntax RemoveTrailingComma(InitializerExpressionSyntax initializer)
        {
            SeparatedSyntaxList<ExpressionSyntax> expressions = initializer.Expressions;

            SyntaxToken trailingComma = expressions.GetTrailingSeparator();

            SeparatedSyntaxList<ExpressionSyntax> newExpressions = expressions.ReplaceSeparator(
                trailingComma,
                SyntaxFactory.MissingToken(SyntaxKind.CommaToken));

            int lastIndex = expressions.Count - 1;

            SyntaxTriviaList newTrailingTrivia = expressions[lastIndex]
                .GetTrailingTrivia()
                .AddRange(trailingComma.LeadingTrivia)
                .AddRange(trailingComma.TrailingTrivia);

            ExpressionSyntax newExpression = newExpressions[lastIndex].WithTrailingTrivia(newTrailingTrivia);

            newExpressions = newExpressions.ReplaceAt(lastIndex, newExpression);

            return initializer.WithExpressions(newExpressions);
        }
    }
}
