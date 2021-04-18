// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Analysis;
using Roslynator.CSharp.Refactorings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConvertHasFlagCallToBitwiseOperationOrViceVersaCodeFixProvider))]
    [Shared]
    public sealed class ConvertHasFlagCallToBitwiseOperationOrViceVersaCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ExpressionSyntax expression))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            if (expression is InvocationExpressionSyntax invocationExpression)
            {
                CodeAction codeAction = CodeAction.Create(
                    ConvertHasFlagCallToBitwiseOperationRefactoring.Title,
                    ct => ConvertHasFlagCallToBitwiseOperationRefactoring.RefactorAsync(document, invocationExpression, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
            else
            {
                Debug.Assert(expression.IsKind(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression), expression.Kind().ToString());

                CodeAction codeAction = CodeAction.Create(
                    "Call 'HasFlag'",
                    ct => ConvertBitwiseOperationToHasFlagCallAsync(document, (BinaryExpressionSyntax)expression, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        private async Task<Document> ConvertBitwiseOperationToHasFlagCallAsync(
            Document document,
            BinaryExpressionSyntax equalsOrNotEquals,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax left = equalsOrNotEquals.Left.WalkDownParentheses();
            ExpressionSyntax right = equalsOrNotEquals.Right.WalkDownParentheses();

            BinaryExpressionSyntax bitwiseAnd;
            ExpressionSyntax valueExpression;
            if (left.IsKind(SyntaxKind.BitwiseAndExpression))
            {
                bitwiseAnd = (BinaryExpressionSyntax)left;
                valueExpression = right;
            }
            else
            {
                bitwiseAnd = (BinaryExpressionSyntax)right;
                valueExpression = left;
            }

            ExpressionSyntax expression = bitwiseAnd.Left;
            ExpressionSyntax argumentExpression = bitwiseAnd.Right;

            ExpressionSyntax newExpression = SimpleMemberInvocationExpression(
                expression,
                IdentifierName("HasFlag"),
                ArgumentList(Argument(argumentExpression.WalkDownParentheses())))
                .Parenthesize();

            if (!(equalsOrNotEquals.IsKind(SyntaxKind.EqualsExpression)
                ^ valueExpression.IsNumericLiteralExpression("0")))
            {
                newExpression = LogicalNotExpression(newExpression).Parenthesize();
            }

            newExpression = newExpression.WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(equalsOrNotEquals, newExpression, cancellationToken).ConfigureAwait(false);
        }
    }
}
