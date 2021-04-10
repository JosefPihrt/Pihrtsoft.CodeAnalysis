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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExpressionCodeFixProvider))]
    [Shared]
    public sealed class ExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.ExpressionIsAlwaysEqualToTrueOrFalse,
                    DiagnosticIdentifiers.ParenthesizeConditionOfConditionalExpression);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ExpressionSyntax expression))
                return;

            Document document = context.Document;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.ParenthesizeConditionOfConditionalExpression:
                        {
                            if (expression is ParenthesizedExpressionSyntax parenthesizedExpression)
                            {
                                CodeAction codeAction = CodeActionFactory.RemoveParentheses(document, parenthesizedExpression, equivalenceKey: GetEquivalenceKey(diagnostic));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }
                            else
                            {
                                CodeAction codeAction = CodeAction.Create(
                                    "Parenthesize condition",
                                    ct => ParenthesizeConditionOfConditionalExpressionAsync(document, expression, ct),
                                    GetEquivalenceKey(diagnostic));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }

                            break;
                        }
                    case DiagnosticIdentifiers.ExpressionIsAlwaysEqualToTrueOrFalse:
                        {
                            if (expression.IsKind(
                                SyntaxKind.LessThanExpression,
                                SyntaxKind.LessThanOrEqualExpression,
                                SyntaxKind.GreaterThanExpression,
                                SyntaxKind.GreaterThanOrEqualExpression))
                            {
                                var binaryExpression = (BinaryExpressionSyntax)expression;

                                LiteralExpressionSyntax newNode = BooleanLiteralExpression(binaryExpression.IsKind(SyntaxKind.GreaterThanOrEqualExpression, SyntaxKind.LessThanOrEqualExpression));

                                CodeAction codeAction = CodeAction.Create(
                                    $"Replace expression with '{newNode}'",
                                    ct => document.ReplaceNodeAsync(binaryExpression, newNode.WithTriviaFrom(binaryExpression), ct),
                                    GetEquivalenceKey(diagnostic));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }
                            else if (diagnostic.Properties.TryGetValue("DoubleNaN", out string leftOrRight))
                            {
                                var binaryExpression = (BinaryExpressionSyntax)expression;

                                CodeAction codeAction = CodeAction.Create(
                                    "Call 'IsNaN'",
                                    ct =>
                                    {
                                        ExpressionSyntax newExpression = SimpleMemberInvocationExpression(
                                            CSharpTypeFactory.DoubleType(),
                                            IdentifierName("IsNaN"),
                                            Argument((leftOrRight == "Left") ? binaryExpression.Left.WithoutLeadingTrivia() : binaryExpression.Right.WithoutTrailingTrivia()));

                                        if (binaryExpression.IsKind(SyntaxKind.NotEqualsExpression))
                                            newExpression = LogicalNotExpression(newExpression);

                                        newExpression = newExpression.Parenthesize().WithTriviaFrom(binaryExpression);

                                        return document.ReplaceNodeAsync(binaryExpression, newExpression, ct);
                                    },
                                    GetEquivalenceKey(diagnostic));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }
                            else
                            {
                                CodeAction codeAction = CodeAction.Create(
                                    "Remove null check",
                                    ct => RemoveUnnecessaryNullCheckAsync(document, expression, ct),
                                    GetEquivalenceKey(diagnostic));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }

                            break;
                        }
                }
            }
        }

        private static Task<Document> ParenthesizeConditionOfConditionalExpressionAsync(
            Document document,
            ExpressionSyntax condition,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax newCondition = ParenthesizedExpression(condition.WithoutTrivia())
                .WithTriviaFrom(condition)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(condition, newCondition, cancellationToken);
        }

        private static Task<Document> RemoveUnnecessaryNullCheckAsync(
            Document document,
            ExpressionSyntax expression,
            CancellationToken cancellationToken)
        {
            expression = expression.WalkUpParentheses();

            var binaryExpression = (BinaryExpressionSyntax)expression.Parent;

            ExpressionSyntax newExpression = binaryExpression.Right.WithLeadingTrivia(expression.GetLeadingTrivia());

            return document.ReplaceNodeAsync(binaryExpression, newExpression, cancellationToken);
        }
    }
}