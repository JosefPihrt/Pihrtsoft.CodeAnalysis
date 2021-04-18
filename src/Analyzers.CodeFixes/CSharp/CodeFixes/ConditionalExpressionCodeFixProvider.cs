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
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeActions;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Refactorings;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConditionalExpressionCodeFixProvider))]
    [Shared]
    public sealed class ConditionalExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.UseCoalesceExpressionInsteadOfConditionalExpression,
                    DiagnosticIdentifiers.SimplifyConditionalExpression,
                    DiagnosticIdentifiers.UseConditionalAccessInsteadOfConditionalExpression,
                    DiagnosticIdentifiers.AvoidNestedConditionalOperators);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ConditionalExpressionSyntax conditionalExpression))
                return;

            Document document = context.Document;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.UseCoalesceExpressionInsteadOfConditionalExpression:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Use coalesce expression",
                                cancellationToken =>
                                {
                                    return SimplifyNullCheckRefactoring.RefactorAsync(
                                        document,
                                        conditionalExpression,
                                        cancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.SimplifyConditionalExpression:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Simplify conditional expression",
                                ct => SimplifyConditionalExpressionAsync(document, conditionalExpression, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UseConditionalAccessInsteadOfConditionalExpression:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Use conditional access",
                                cancellationToken =>
                                {
                                    return SimplifyNullCheckRefactoring.RefactorAsync(
                                        document,
                                        conditionalExpression,
                                        cancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.AvoidNestedConditionalOperators:
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            (CodeAction codeAction, CodeAction recursiveCodeAction) = ConvertConditionalOperatorToIfElseRefactoring.ComputeRefactoring(
                                document,
                                conditionalExpression,
                                data: default,
                                recursiveData: new CodeActionData(ConvertConditionalOperatorToIfElseRefactoring.Title, GetEquivalenceKey(diagnostic)),
                                semanticModel,
                                context.CancellationToken);

                            if (recursiveCodeAction != null)
                                context.RegisterCodeFix(recursiveCodeAction, diagnostic);

                            break;
                        }
                }
            }
        }

        private static async Task<Document> SimplifyConditionalExpressionAsync(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            CancellationToken cancellationToken)
        {
            ConditionalExpressionInfo info = SyntaxInfo.ConditionalExpressionInfo(conditionalExpression);

            ExpressionSyntax whenTrue = info.WhenTrue;
            ExpressionSyntax whenFalse = info.WhenFalse;

            SyntaxKind trueKind = whenTrue.Kind();
            SyntaxKind falseKind = whenFalse.Kind();

            ExpressionSyntax newNode = null;

            if (trueKind == SyntaxKind.TrueLiteralExpression)
            {
                if (falseKind == SyntaxKind.FalseLiteralExpression)
                {
                    // a ? true : false >>> a

                    newNode = CreateNewNode(conditionalExpression, info.Condition);
                }
                else
                {
                    // a ? true : b >>> a || b

                    SyntaxTriviaList trailingTrivia = info
                        .QuestionToken
                        .LeadingTrivia
                        .AddRange(info.QuestionToken.TrailingTrivia)
                        .AddRange(whenTrue.GetLeadingTrivia())
                        .EmptyIfWhitespace();

                    newNode = LogicalOrExpression(
                        conditionalExpression.Condition.Parenthesize().AppendToTrailingTrivia(trailingTrivia),
                        Token(info.ColonToken.LeadingTrivia, SyntaxKind.BarBarToken, info.ColonToken.TrailingTrivia),
                        whenFalse.Parenthesize());
                }
            }
            else if (trueKind == SyntaxKind.FalseLiteralExpression)
            {
                if (falseKind == SyntaxKind.TrueLiteralExpression)
                {
                    // a ? false : true >>> !a

                    SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                    newNode = CreateNewNode(conditionalExpression, SyntaxInverter.LogicallyInvert(info.Condition, semanticModel, cancellationToken));
                }
                else
                {
                    // a ? false : b >>> !a && b

                    SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                    ExpressionSyntax newCondition = SyntaxInverter.LogicallyInvert(conditionalExpression.Condition, semanticModel, cancellationToken);

                    SyntaxTriviaList trailingTrivia = info
                        .QuestionToken
                        .LeadingAndTrailingTrivia()
                        .AddRange(whenTrue.GetLeadingAndTrailingTrivia())
                        .EmptyIfWhitespace();

                    newNode = LogicalAndExpression(
                        newCondition.Parenthesize().AppendToTrailingTrivia(trailingTrivia),
                        Token(info.ColonToken.LeadingTrivia, SyntaxKind.AmpersandAmpersandToken, info.ColonToken.TrailingTrivia),
                        whenFalse.Parenthesize());
                }
            }
            else if (falseKind == SyntaxKind.FalseLiteralExpression)
            {
                // a ? b : false >>> a && b

                SyntaxTriviaList trailingTrivia = whenTrue
                    .GetTrailingTrivia()
                    .AddRange(info.ColonToken.LeadingTrivia)
                    .AddRange(info.ColonToken.TrailingTrivia)
                    .AddRange(whenFalse.GetLeadingTrivia())
                    .EmptyIfWhitespace()
                    .AddRange(whenFalse.GetTrailingTrivia());

                newNode = LogicalAndExpression(
                    conditionalExpression.Condition.Parenthesize(),
                    Token(info.QuestionToken.LeadingTrivia, SyntaxKind.AmpersandAmpersandToken, info.QuestionToken.TrailingTrivia),
                    whenTrue.WithTrailingTrivia(trailingTrivia).Parenthesize());
            }
            else if (falseKind == SyntaxKind.TrueLiteralExpression)
            {
                // a ? b : true >>> !a || b

                SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                ExpressionSyntax newCondition = SyntaxInverter.LogicallyInvert(conditionalExpression.Condition, semanticModel, cancellationToken);

                SyntaxTriviaList trailingTrivia = whenTrue
                    .GetTrailingTrivia()
                    .AddRange(info.ColonToken.LeadingAndTrailingTrivia())
                    .AddRange(whenFalse.GetLeadingAndTrailingTrivia())
                    .EmptyIfWhitespace();

                newNode = LogicalOrExpression(
                    newCondition.Parenthesize(),
                    Token(info.QuestionToken.LeadingTrivia, SyntaxKind.BarBarToken, info.QuestionToken.TrailingTrivia),
                    whenTrue.Parenthesize().WithTrailingTrivia(trailingTrivia));
            }

            newNode = newNode.Parenthesize();

            return await document.ReplaceNodeAsync(conditionalExpression, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static ExpressionSyntax CreateNewNode(
            ConditionalExpressionSyntax conditionalExpression,
            ExpressionSyntax newNode)
        {
            SyntaxTriviaList trailingTrivia = conditionalExpression
                .DescendantTrivia(TextSpan.FromBounds(conditionalExpression.Condition.Span.End, conditionalExpression.Span.End))
                .ToSyntaxTriviaList()
                .EmptyIfWhitespace()
                .AddRange(conditionalExpression.GetTrailingTrivia());

            return newNode
                .WithLeadingTrivia(conditionalExpression.GetLeadingTrivia())
                .WithTrailingTrivia(trailingTrivia)
                .WithSimplifierAnnotation();
        }
    }
}
