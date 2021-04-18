// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UsePatternMatchingToCheckForNullOrViceVersaCodeFixProvider))]
    [Shared]
    public sealed class UsePatternMatchingToCheckForNullOrViceVersaCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UsePatternMatchingToCheckForNullOrViceVersa); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(
                root,
                context.Span,
                out SyntaxNode node,
                predicate: f => f.IsKind(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression, SyntaxKind.IsPatternExpression)))
            {
                return;
            }

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            if (node is BinaryExpressionSyntax binaryExpression)
            {
                CodeAction codeAction = CodeAction.Create(
                    "Use pattern matching",
                    ct => UseIsNullPatternInsteadOfComparisonAsync(document, binaryExpression, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
            else if (node is IsPatternExpressionSyntax isPatternExpression)
            {
                CodeAction codeAction = CodeAction.Create(
                    (isPatternExpression.WalkUpParentheses().IsParentKind(SyntaxKind.LogicalNotExpression))
                        ? "Use '!=' operator"
                        : "Use '==' operator",
                    ct => UseComparisonInsteadOfIsNullPatternAsync(document, isPatternExpression, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static Task<Document> UseIsNullPatternInsteadOfComparisonAsync(
            Document document,
            BinaryExpressionSyntax binaryExpression,
            CancellationToken cancellationToken)
        {
            NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(binaryExpression, NullCheckStyles.ComparisonToNull, walkDownParentheses: false);

            ExpressionSyntax expression = nullCheck.Expression;
            ExpressionSyntax nullLiteral;

            if (object.ReferenceEquals(expression, binaryExpression.Left))
            {
                nullLiteral = binaryExpression.Right;
            }
            else
            {
                expression = expression.WithTrailingTrivia(binaryExpression.Left.GetTrailingTrivia());

                nullLiteral = binaryExpression.Left.WithLeadingTrivia(expression.GetLeadingTrivia());
            }

            bool useIsNotNull = !AnalyzerOptions.UseLogicalNegationAndPatternMatchingToCheckForNull.IsEnabled(document, binaryExpression);

            PatternSyntax pattern = ConstantPattern(nullLiteral);

            if (binaryExpression.IsKind(SyntaxKind.NotEqualsExpression)
                && useIsNotNull)
            {
                pattern = NotPattern(pattern);
            }

            ExpressionSyntax newExpression = IsPatternExpression(
                expression,
                Token(binaryExpression.OperatorToken.LeadingTrivia, SyntaxKind.IsKeyword, binaryExpression.OperatorToken.TrailingTrivia),
                pattern);

            if (binaryExpression.IsKind(SyntaxKind.NotEqualsExpression)
                && !useIsNotNull)
            {
                newExpression = LogicalNotExpression(ParenthesizedExpression(newExpression.WithoutTrivia()));
            }

            newExpression = newExpression
                .WithTriviaFrom(binaryExpression)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(binaryExpression, newExpression, cancellationToken);
        }

        private static Task<Document> UseComparisonInsteadOfIsNullPatternAsync(
            Document document,
            IsPatternExpressionSyntax isPatternExpression,
            CancellationToken cancellationToken)
        {
            bool isNegation = isPatternExpression.WalkUpParentheses().IsParentKind(SyntaxKind.LogicalNotExpression);

            BinaryExpressionSyntax binaryExpression = BinaryExpression(
                (isNegation) ? SyntaxKind.NotEqualsExpression : SyntaxKind.EqualsExpression,
                isPatternExpression.Expression,
                Token(
                    isPatternExpression.IsKeyword.LeadingTrivia,
                    (isNegation) ? SyntaxKind.ExclamationEqualsToken : SyntaxKind.EqualsEqualsToken,
                    isPatternExpression.IsKeyword.TrailingTrivia),
                ((ConstantPatternSyntax)isPatternExpression.Pattern).Expression);

            if (isNegation)
            {
                SyntaxNode parent = isPatternExpression.WalkUpParentheses().Parent;

                binaryExpression = binaryExpression.WithTriviaFrom(parent);

                return document.ReplaceNodeAsync(parent, binaryExpression, cancellationToken);
            }
            else
            {
                return document.ReplaceNodeAsync(isPatternExpression, binaryExpression, cancellationToken);
            }
        }
    }
}