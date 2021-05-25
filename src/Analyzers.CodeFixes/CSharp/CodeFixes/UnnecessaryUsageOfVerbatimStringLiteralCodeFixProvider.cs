// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnnecessaryUsageOfVerbatimStringLiteralCodeFixProvider))]
    [Shared]
    public sealed class UnnecessaryUsageOfVerbatimStringLiteralCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Remove '@'";

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UnnecessaryUsageOfVerbatimStringLiteral); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SyntaxNode node, predicate: f => f.IsKind(SyntaxKind.StringLiteralExpression, SyntaxKind.InterpolatedStringExpression)))
                return;

            CodeAction codeAction = CodeAction.Create(
                Title,
                ct => RefactorAsync(context.Document, node, ct),
                GetEquivalenceKey(DiagnosticIdentifiers.UnnecessaryUsageOfVerbatimStringLiteral));

            context.RegisterCodeFix(codeAction, context.Diagnostics[0]);
        }

        private static Task<Document> RefactorAsync(
            Document document,
            SyntaxNode node,
            CancellationToken cancellationToken)
        {
            int start = node.SpanStart;

            if (node is InterpolatedStringExpressionSyntax interpolatedString
                && interpolatedString.StringStartToken.ValueText.StartsWith("$"))
            {
                start++;
            }

            ExpressionSyntax newNode = SyntaxFactory.ParseExpression(node.ToFullString().Remove(start - node.FullSpan.Start, 1));

            return document.ReplaceNodeAsync(node, newNode, cancellationToken);
        }
    }
}