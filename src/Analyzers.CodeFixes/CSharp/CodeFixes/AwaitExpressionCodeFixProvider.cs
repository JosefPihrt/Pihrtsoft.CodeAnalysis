// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Analysis;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AwaitExpressionCodeFixProvider))]
    [Shared]
    public sealed class AwaitExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.AddCallToConfigureAwaitOrViceVersa); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out AwaitExpressionSyntax awaitExpression))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            if (AddCallToConfigureAwaitOrViceVersaAnalyzer.IsConfigureAwait(awaitExpression.Expression))
            {
                CodeAction codeAction = CodeAction.Create(
                    "Remove call to 'ConfigureAwait'",
                    ct => RemoveCallToConfigureAwaitRefactorAsync(document, awaitExpression, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
            else
            {
                CodeAction codeAction = CodeAction.Create(
                    "Add to call 'ConfigureAwait'",
                    ct => AddCallToConfigureAwaitRefactorAsync(document, awaitExpression, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        private static Task<Document> AddCallToConfigureAwaitRefactorAsync(
            Document document,
            AwaitExpressionSyntax awaitExpression,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax expression = awaitExpression.Expression;

            SyntaxTriviaList leading = default;

            switch (expression.Kind())
            {
                case SyntaxKind.SimpleMemberAccessExpression:
                    {
                        var memberAccess = (MemberAccessExpressionSyntax)expression;

                        leading = memberAccess.OperatorToken.LeadingTrivia;
                        break;
                    }
                case SyntaxKind.InvocationExpression:
                    {
                        var invocation = (InvocationExpressionSyntax)expression;

                        SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocation);

                        leading = invocationInfo.OperatorToken.LeadingTrivia;
                        break;
                    }
            }

            SyntaxTrivia last = leading.LastOrDefault();

            last = (last.IsWhitespaceTrivia()) ? last : default;

            ParenthesizedExpressionSyntax expression2 = expression.WithoutTrailingTrivia().Parenthesize();

            if (last.IsWhitespaceTrivia())
                expression2 = expression2.WithTrailingTrivia(SyntaxTriviaAnalysis.DetermineEndOfLine(awaitExpression));

            InvocationExpressionSyntax newInvocationExpression = InvocationExpression(
                SimpleMemberAccessExpression(
                    expression2,
                    Token(SyntaxKind.DotToken).WithLeadingTrivia(last),
                    IdentifierName("ConfigureAwait")),
                ArgumentList(
                    Token(SyntaxKind.OpenParenToken),
                    SingletonSeparatedList(Argument(FalseLiteralExpression())),
                    Token(default, SyntaxKind.CloseParenToken, expression.GetTrailingTrivia())));

            return document.ReplaceNodeAsync(expression, newInvocationExpression, cancellationToken);
        }

        private static Task<Document> RemoveCallToConfigureAwaitRefactorAsync(
            Document document,
            AwaitExpressionSyntax awaitExpression,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax expression = awaitExpression.Expression;

            SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(expression);

            ExpressionSyntax newExpression = invocationInfo.Expression
                .WithTrailingTrivia(invocationInfo.Expression.GetTrailingTrivia().EmptyIfWhitespace())
                .AppendToTrailingTrivia(expression.GetTrailingTrivia());

            return document.ReplaceNodeAsync(expression, newExpression, cancellationToken);
        }
    }
}
