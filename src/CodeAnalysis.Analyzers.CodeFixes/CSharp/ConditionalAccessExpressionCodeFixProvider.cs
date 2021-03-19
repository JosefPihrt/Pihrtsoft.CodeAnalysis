// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;

namespace Roslynator.CodeAnalysis.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConditionalAccessExpressionCodeFixProvider))]
    [Shared]
    public class ConditionalAccessExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UnnecessaryConditionalAccess); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ConditionalAccessExpressionSyntax conditionalAccess))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                "Remove unnecessary '?'",
                ct => RemoveUnnecessaryConditionalAccessAsync(document, conditionalAccess, ct),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static Task<Document> RemoveUnnecessaryConditionalAccessAsync(
            Document document,
            ConditionalAccessExpressionSyntax conditionalAccess,
            CancellationToken cancellationToken)
        {
            SyntaxNode equalsExpression = conditionalAccess.WalkUpParentheses().Parent;

            ExpressionSyntax newExpression = conditionalAccess
                .RemoveOperatorToken()
                .WithLeadingTrivia(equalsExpression.GetLeadingTrivia())
                .WithTrailingTrivia(equalsExpression.GetTrailingTrivia())
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(equalsExpression, newExpression, cancellationToken);
        }
    }
}
