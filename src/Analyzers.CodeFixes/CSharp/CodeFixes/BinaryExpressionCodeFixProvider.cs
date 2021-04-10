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
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Refactorings;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
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
                    DiagnosticIdentifiers.SimplifyBooleanComparison,
                    DiagnosticIdentifiers.ConstantValuesShouldBePlacedOnRightSideOfComparisons,
                    DiagnosticIdentifiers.UseStringIsNullOrEmptyMethod,
                    DiagnosticIdentifiers.SimplifyCoalesceExpression,
                    DiagnosticIdentifiers.RemoveRedundantAsOperator,
                    DiagnosticIdentifiers.UseStringLengthInsteadOfComparisonWithEmptyString,
                    DiagnosticIdentifiers.UnconstrainedTypeParameterCheckedForNull,
                    DiagnosticIdentifiers.ValueTypeObjectIsNeverEqualToNull,
                    DiagnosticIdentifiers.JoinStringExpressions,
                    DiagnosticIdentifiers.UseExclusiveOrOperator,
                    DiagnosticIdentifiers.UnnecessaryNullCheck,
                    DiagnosticIdentifiers.UseShortCircuitingOperator,
                    DiagnosticIdentifiers.UnnecessaryOperator);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out BinaryExpressionSyntax binaryExpression))
                return;

            Document document = context.Document;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.SimplifyBooleanComparison:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Simplify boolean comparison",
                                cancellationToken => SimplifyBooleanComparisonRefactoring.RefactorAsync(document, binaryExpression, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);

                            break;
                        }
                    case DiagnosticIdentifiers.ConstantValuesShouldBePlacedOnRightSideOfComparisons:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Swap operands",
                                cancellationToken => document.ReplaceNodeAsync(binaryExpression, SyntaxRefactorings.SwapBinaryOperands(binaryExpression), cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UseStringIsNullOrEmptyMethod:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Use 'string.IsNullOrEmpty' method",
                                cancellationToken => UseStringIsNullOrEmptyMethodAsync(document, binaryExpression, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.SimplifyCoalesceExpression:
                        {
                            ExpressionSyntax expression = binaryExpression.Left;

                            if (expression == null
                                || !context.Span.Contains(expression.Span))
                            {
                                expression = binaryExpression.Right;
                            }

                            CodeAction codeAction = CodeAction.Create(
                                "Simplify coalesce expression",
                                cancellationToken => SimplifyCoalesceExpressionRefactoring.RefactorAsync(document, binaryExpression, expression, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.RemoveRedundantAsOperator:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove redundant 'as' operator",
                                cancellationToken => RemoveRedundantAsOperatorRefactoring.RefactorAsync(document, binaryExpression, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UseStringLengthInsteadOfComparisonWithEmptyString:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Use string.Length",
                                cancellationToken => UseStringLengthInsteadOfComparisonWithEmptyStringAsync(document, binaryExpression, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UnconstrainedTypeParameterCheckedForNull:
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(binaryExpression.Left, context.CancellationToken);

                            CodeAction codeAction = CodeAction.Create(
                                $"Use EqualityComparer<{typeSymbol.Name}>.Default",
                                cancellationToken => UnconstrainedTypeParameterCheckedForNullRefactoring.RefactorAsync(document, binaryExpression, typeSymbol, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.ValueTypeObjectIsNeverEqualToNull:
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(binaryExpression.Left, context.CancellationToken);

                            string title;

                            if (CSharpFacts.IsSimpleType(typeSymbol.SpecialType)
                                || typeSymbol.ContainsMember<IMethodSymbol>(WellKnownMemberNames.EqualityOperatorName))
                            {
                                ExpressionSyntax expression = typeSymbol.GetDefaultValueSyntax(document.GetDefaultSyntaxOptions());

                                title = $"Replace 'null' with '{expression}'";
                            }
                            else
                            {
                                title = $"Use EqualityComparer<{SymbolDisplay.ToMinimalDisplayString(typeSymbol, semanticModel, binaryExpression.Right.SpanStart, SymbolDisplayFormats.DisplayName)}>.Default";
                            }

                            CodeAction codeAction = CodeAction.Create(
                                title,
                                cancellationToken => ValueTypeObjectIsNeverEqualToNullRefactoring.RefactorAsync(document, binaryExpression, typeSymbol, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.JoinStringExpressions:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Join string expressions",
                                cancellationToken => JoinStringExpressionsRefactoring.RefactorAsync(document, binaryExpression, context.Span, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UseExclusiveOrOperator:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Use ^ operator",
                                cancellationToken => UseExclusiveOrOperatorRefactoring.RefactorAsync(document, binaryExpression, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UnnecessaryNullCheck:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove unnecessary null check",
                                ct => RemoveUnnecessaryNullCheckAsync(document, binaryExpression, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UseShortCircuitingOperator:
                        {
                            SyntaxToken operatorToken = binaryExpression.OperatorToken;

                            SyntaxKind kind = binaryExpression.Kind();

                            SyntaxToken newToken = default;

                            if (kind == SyntaxKind.BitwiseAndExpression)
                            {
                                newToken = Token(operatorToken.LeadingTrivia, SyntaxKind.AmpersandAmpersandToken, operatorToken.TrailingTrivia);
                            }
                            else if (kind == SyntaxKind.BitwiseOrExpression)
                            {
                                newToken = Token(operatorToken.LeadingTrivia, SyntaxKind.BarBarToken, operatorToken.TrailingTrivia);
                            }

                            CodeAction codeAction = CodeAction.Create(
                                $"Use '{newToken.ToString()}' operator",
                                ct =>
                                {
                                    BinaryExpressionSyntax newBinaryExpression = null;

                                    if (kind == SyntaxKind.BitwiseAndExpression)
                                    {
                                        newBinaryExpression = LogicalAndExpression(binaryExpression.Left, newToken, binaryExpression.Right);
                                    }
                                    else if (kind == SyntaxKind.BitwiseOrExpression)
                                    {
                                        newBinaryExpression = LogicalOrExpression(binaryExpression.Left, newToken, binaryExpression.Right);
                                    }

                                    return document.ReplaceNodeAsync(binaryExpression, newBinaryExpression, ct);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UnnecessaryOperator:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Use '==' operator",
                                ct =>
                                {
                                    SyntaxToken operatorToken = binaryExpression.OperatorToken;

                                    BinaryExpressionSyntax newBinaryExpression = EqualsExpression(
                                        binaryExpression.Left,
                                        Token(operatorToken.LeadingTrivia, SyntaxKind.EqualsEqualsToken, operatorToken.TrailingTrivia),
                                        binaryExpression.Right);

                                    return document.ReplaceNodeAsync(binaryExpression, newBinaryExpression, ct);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static Task<Document> UseStringIsNullOrEmptyMethodAsync(
            Document document,
            BinaryExpressionSyntax binaryExpression,
            CancellationToken cancellationToken)
        {
            NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(binaryExpression.Left);

            ExpressionSyntax newNode = SimpleMemberInvocationExpression(
                CSharpTypeFactory.StringType(),
                IdentifierName("IsNullOrEmpty"),
                Argument(nullCheck.Expression));

            if (nullCheck.IsCheckingNotNull)
                newNode = LogicalNotExpression(newNode);

            newNode = newNode
                .WithTriviaFrom(binaryExpression)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(binaryExpression, newNode, cancellationToken);
        }

        private static async Task<Document> UseStringLengthInsteadOfComparisonWithEmptyStringAsync(
            Document document,
            BinaryExpressionSyntax binaryExpression,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            ExpressionSyntax left = binaryExpression.Left;

            ExpressionSyntax right = binaryExpression.Right;

            BinaryExpressionSyntax newNode;

            if (CSharpUtility.IsEmptyStringExpression(left, semanticModel, cancellationToken))
            {
                newNode = binaryExpression
                    .WithLeft(NumericLiteralExpression(0))
                    .WithRight(CreateConditionalAccess(right));
            }
            else if (CSharpUtility.IsEmptyStringExpression(right, semanticModel, cancellationToken))
            {
                newNode = binaryExpression
                    .WithLeft(CreateConditionalAccess(left))
                    .WithRight(NumericLiteralExpression(0));
            }
            else
            {
                Debug.Fail(binaryExpression.ToString());
                return document;
            }

            newNode = newNode.WithTriviaFrom(binaryExpression).WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(binaryExpression, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static ConditionalAccessExpressionSyntax CreateConditionalAccess(ExpressionSyntax expression)
        {
            return ConditionalAccessExpression(
                expression.Parenthesize(),
                MemberBindingExpression(IdentifierName("Length")));
        }

        private static async Task<Document> RemoveUnnecessaryNullCheckAsync(
            Document document,
            BinaryExpressionSyntax logicalAnd,
            CancellationToken cancellationToken)
        {
            BinaryExpressionInfo binaryExpressionInfo = SyntaxInfo.BinaryExpressionInfo(logicalAnd);

            ExpressionSyntax right = binaryExpressionInfo.Right;

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(binaryExpressionInfo.Left, semanticModel, NullCheckStyles.HasValue | NullCheckStyles.NotEqualsToNull);

            var binaryExpression = right as BinaryExpressionSyntax;

            ExpressionSyntax newRight;
            switch (right.Kind())
            {
                case SyntaxKind.SimpleMemberAccessExpression:
                    {
                        newRight = TrueLiteralExpression().WithTriviaFrom(right);
                        break;
                    }
                case SyntaxKind.LogicalNotExpression:
                    {
                        newRight = FalseLiteralExpression().WithTriviaFrom(right);
                        break;
                    }
                default:
                    {
                        newRight = binaryExpression.Right;
                        break;
                    }
            }

            BinaryExpressionSyntax newBinaryExpression = BinaryExpression(
                (binaryExpression != null)
                    ? right.Kind()
                    : SyntaxKind.EqualsExpression,
                nullCheck.Expression.WithLeadingTrivia(logicalAnd.GetLeadingTrivia()),
                (binaryExpression != null)
                    ? ((BinaryExpressionSyntax)right).OperatorToken
                    : Token(SyntaxKind.EqualsEqualsToken).WithTriviaFrom(logicalAnd.OperatorToken),
                newRight)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(logicalAnd, newBinaryExpression, cancellationToken).ConfigureAwait(false);
        }
    }
}