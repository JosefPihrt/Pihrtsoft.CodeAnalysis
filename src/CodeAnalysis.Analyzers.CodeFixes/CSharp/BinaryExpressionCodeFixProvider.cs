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
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeAnalysis.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BinaryExpressionCodeFixProvider))]
    [Shared]
    public sealed class BinaryExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.CallAnyInsteadOfAccessingCount,
                    DiagnosticIdentifiers.UnnecessaryNullCheck);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out BinaryExpressionSyntax binaryExpression))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            switch (diagnostic.Id)
            {
                case DiagnosticIdentifiers.CallAnyInsteadOfAccessingCount:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            "Call 'Any' instead of accessing 'Count'",
                            ct => CallAnyInsteadOfUsingCountAsync(document, binaryExpression, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.UnnecessaryNullCheck:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            "Remove unnecessary null check",
                            ct =>
                            {
                                ExpressionSyntax newExpression = binaryExpression.Right.WithLeadingTrivia(binaryExpression.GetLeadingTrivia());

                                return document.ReplaceNodeAsync(binaryExpression, newExpression, ct);
                            },
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }
        }

        private static Task<Document> CallAnyInsteadOfUsingCountAsync(
            Document document,
            BinaryExpressionSyntax binaryExpression,
            CancellationToken cancellationToken)
        {
            BinaryExpressionInfo binaryExpressionInfo = SyntaxInfo.BinaryExpressionInfo(binaryExpression);

            if (!(binaryExpressionInfo.Left is MemberAccessExpressionSyntax memberAccessExpression))
                memberAccessExpression = (MemberAccessExpressionSyntax)binaryExpressionInfo.Right;

            SimpleNameSyntax name = memberAccessExpression.Name;

            ExpressionSyntax newExpression = SimpleMemberInvocationExpression(
                memberAccessExpression.Expression,
                IdentifierName("Any").WithLeadingTrivia(name.GetLeadingTrivia()),
                ArgumentList().WithTrailingTrivia(name.GetTrailingTrivia()));

            if (binaryExpression.IsKind(SyntaxKind.EqualsExpression))
                newExpression = LogicalNotExpression(newExpression.WithoutLeadingTrivia().Parenthesize());

            newExpression = newExpression
                .WithTriviaFrom(binaryExpression)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(binaryExpression, newExpression, cancellationToken);
        }
    }
}
