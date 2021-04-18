// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;
using Roslynator.Formatting.CSharp;

namespace Roslynator.Formatting.CodeFixes.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FixFormattingOfCallChainCodeFixProvider))]
    [Shared]
    public sealed class FixFormattingOfCallChainCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.FixFormattingOfCallChain); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(
                root,
                context.Span,
                out ExpressionSyntax expression,
                predicate: f => f.IsKind(
                    SyntaxKind.InvocationExpression,
                    SyntaxKind.ElementAccessExpression,
                    SyntaxKind.ConditionalAccessExpression)))
            {
                return;
            }

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                "Fix formatting",
                ct =>
                {
                    TextLineCollection lines = expression.SyntaxTree.GetText().Lines;

                    TextLine line = lines.GetLineFromPosition(expression.SpanStart);

                    TextSpan span = TextSpan.FromBounds(line.EndIncludingLineBreak, expression.Span.End);

                    return CodeFixHelpers.FixCallChainAsync(document, expression, span, ct);
                },
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
