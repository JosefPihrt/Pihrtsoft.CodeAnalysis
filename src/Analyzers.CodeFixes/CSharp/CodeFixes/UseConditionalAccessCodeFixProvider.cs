// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
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
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Analysis;
using Roslynator.CSharp.Refactorings;
using Roslynator.CSharp.Syntax;
using Roslynator.Text;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseConditionalAccessCodeFixProvider))]
    [Shared]
    public sealed class UseConditionalAccessCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Use conditional access";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UseConditionalAccess); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(
                root,
                context.Span,
                out SyntaxNode node,
                predicate: f => f.IsKind(
                    SyntaxKind.LogicalAndExpression,
                    SyntaxKind.LogicalOrExpression,
                    SyntaxKind.IfStatement)))
            {
                return;
            }

            switch (node.Kind())
            {
                case SyntaxKind.LogicalAndExpression:
                case SyntaxKind.LogicalOrExpression:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            Title,
                            ct => UseConditionalAccessAsync(context.Document, (BinaryExpressionSyntax)node, ct),
                            GetEquivalenceKey(DiagnosticIdentifiers.UseConditionalAccess));

                        context.RegisterCodeFix(codeAction, context.Diagnostics);
                        break;
                    }
                case SyntaxKind.IfStatement:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            Title,
                            ct => UseConditionalAccessAsync(context.Document, (IfStatementSyntax)node, ct),
                            GetEquivalenceKey(DiagnosticIdentifiers.UseConditionalAccess));

                        context.RegisterCodeFix(codeAction, context.Diagnostics);
                        break;
                    }
            }
        }

        private static async Task<Document> UseConditionalAccessAsync(
            Document document,
            BinaryExpressionSyntax binaryExpression,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            SyntaxKind kind = binaryExpression.Kind();

            (ExpressionSyntax left, ExpressionSyntax right) = UseConditionalAccessAnalyzer.GetFixableExpressions(binaryExpression, kind, semanticModel, cancellationToken);

            NullCheckStyles allowedStyles = (kind == SyntaxKind.LogicalAndExpression)
                ? NullCheckStyles.NotEqualsToNull
                : NullCheckStyles.EqualsToNull;

            NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(left, semanticModel, allowedStyles: allowedStyles, cancellationToken: cancellationToken);

            ExpressionSyntax expression = nullCheck.Expression;

            bool isNullable = semanticModel.GetTypeSymbol(expression, cancellationToken).IsNullableType();

            ExpressionSyntax expression2 = UseConditionalAccessAnalyzer.FindExpressionThatCanBeConditionallyAccessed(
                expression,
                right,
                isNullable: isNullable,
                semanticModel,
                cancellationToken);

            var builder = new SyntaxNodeTextBuilder(binaryExpression, StringBuilderCache.GetInstance(binaryExpression.FullSpan.Length));

            builder.Append(TextSpan.FromBounds(binaryExpression.FullSpan.Start, left.SpanStart));

            int parenDiff = GetParenTokenDiff();

            if (parenDiff > 0)
                builder.Append('(', parenDiff);

            builder.AppendSpan(expression);
            builder.Append("?");
            builder.Append(TextSpan.FromBounds(expression2.Span.End, right.Span.End));

            switch (right.Kind())
            {
                case SyntaxKind.LogicalOrExpression:
                case SyntaxKind.LogicalAndExpression:
                case SyntaxKind.BitwiseOrExpression:
                case SyntaxKind.BitwiseAndExpression:
                case SyntaxKind.ExclusiveOrExpression:
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                case SyntaxKind.IsExpression:
                case SyntaxKind.AsExpression:
                case SyntaxKind.IsPatternExpression:
                    {
                        break;
                    }
                case SyntaxKind.LogicalNotExpression:
                    {
                        builder.Append((kind == SyntaxKind.LogicalAndExpression) ? " == false" : " != true");
                        break;
                    }
                default:
                    {
                        builder.Append((kind == SyntaxKind.LogicalAndExpression) ? " == true" : " != false");
                        break;
                    }
            }

            if (parenDiff < 0)
                builder.Append(')', -parenDiff);

            builder.Append(TextSpan.FromBounds(right.Span.End, binaryExpression.FullSpan.End));

            string text = StringBuilderCache.GetStringAndFree(builder.StringBuilder);

            ParenthesizedExpressionSyntax newNode = SyntaxFactory.ParseExpression(text)
                .WithFormatterAnnotation()
                .Parenthesize();

            return await document.ReplaceNodeAsync(binaryExpression, newNode, cancellationToken).ConfigureAwait(false);

            int GetParenTokenDiff()
            {
                int count = 0;

                foreach (SyntaxToken token in binaryExpression.DescendantTokens(TextSpan.FromBounds(left.SpanStart, expression2.Span.End)))
                {
                    SyntaxKind tokenKind = token.Kind();

                    if (tokenKind == SyntaxKind.OpenParenToken)
                    {
                        if (token.IsParentKind(SyntaxKind.ParenthesizedExpression))
                            count++;
                    }
                    else if (tokenKind == SyntaxKind.CloseParenToken)
                    {
                        if (token.IsParentKind(SyntaxKind.ParenthesizedExpression))
                            count--;
                    }
                }

                return count;
            }
        }

        private static async Task<Document> UseConditionalAccessAsync(
            Document document,
            IfStatementSyntax ifStatement,
            CancellationToken cancellationToken)
        {
            var statement = (ExpressionStatementSyntax)ifStatement.SingleNonBlockStatementOrDefault();

            StatementSyntax newStatement = statement;

            NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(ifStatement.Condition, NullCheckStyles.NotEqualsToNull);

            SimpleMemberInvocationStatementInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationStatementInfo(statement);

            ExpressionSyntax expression = invocationInfo.Expression;

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            if (semanticModel.GetTypeSymbol(nullCheck.Expression, cancellationToken).IsNullableType())
            {
                var memberAccess = (MemberAccessExpressionSyntax)invocationInfo.Expression;

                newStatement = statement.ReplaceNode(memberAccess, memberAccess.Expression.WithTrailingTrivia(memberAccess.GetTrailingTrivia()));

                expression = memberAccess.Expression;
            }

            int insertIndex = expression.Span.End - statement.FullSpan.Start;

            newStatement = SyntaxFactory.ParseStatement(newStatement.ToFullString().Insert(insertIndex, "?"));

            IEnumerable<SyntaxTrivia> leading = ifStatement.DescendantTrivia(TextSpan.FromBounds(ifStatement.SpanStart, statement.SpanStart));

            newStatement = (leading.All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                ? newStatement.WithLeadingTrivia(ifStatement.GetLeadingTrivia())
                : newStatement.WithLeadingTrivia(ifStatement.GetLeadingTrivia().Concat(leading));

            IEnumerable<SyntaxTrivia> trailing = ifStatement.DescendantTrivia(TextSpan.FromBounds(statement.Span.End, ifStatement.Span.End));

            newStatement = (leading.All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                ? newStatement.WithTrailingTrivia(ifStatement.GetTrailingTrivia())
                : newStatement.WithTrailingTrivia(trailing.Concat(ifStatement.GetTrailingTrivia()));

            return await document.ReplaceNodeAsync(ifStatement, newStatement, cancellationToken).ConfigureAwait(false);
        }
    }
}