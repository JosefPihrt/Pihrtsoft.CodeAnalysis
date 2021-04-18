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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFacts;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCompoundAssignmentCodeFixProvider))]
    [Shared]
    public sealed class UseCompoundAssignmentCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Use compound assignment";

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UseCompoundAssignment); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SyntaxNode node, predicate: f => f is AssignmentExpressionSyntax || f.IsKind(SyntaxKind.CoalesceExpression)))
                return;

            Document document = context.Document;

            Diagnostic diagnostic = context.Diagnostics[0];

            if (node is AssignmentExpressionSyntax assignment)
            {
                var binaryExpression = (BinaryExpressionSyntax)assignment.Right.WalkDownParentheses();

                string operatorText = UseCompoundAssignmentAnalyzer.GetCompoundAssignmentOperatorText(binaryExpression);

                CodeAction codeAction = CodeAction.Create(
                    Title,
                    ct => UseCompoundAssignmentAsync(document, assignment, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
            else
            {
                var coalesceExpression = (BinaryExpressionSyntax)node;

                CodeAction codeAction = CodeAction.Create(
                    Title,
                    ct => ConvertLazyInitializationToCompoundAssignmentAsync(document, coalesceExpression, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        private static Task<Document> UseCompoundAssignmentAsync(
            Document document,
            AssignmentExpressionSyntax assignmentExpression,
            CancellationToken cancellationToken = default)
        {
            SyntaxToken operatorToken = assignmentExpression.OperatorToken;

            var binaryExpression = (BinaryExpressionSyntax)assignmentExpression.Right.WalkDownParentheses();

            SyntaxKind kind = GetCompoundAssignmentKind(binaryExpression.Kind());

            SyntaxTriviaList trailingTrivia = binaryExpression
                .Left
                .DescendantTrivia()
                .Concat(binaryExpression.OperatorToken.LeadingAndTrailingTrivia())
                .ToSyntaxTriviaList()
                .EmptyIfWhitespace();

            AssignmentExpressionSyntax newNode = AssignmentExpression(
                kind,
                assignmentExpression.Left,
                Token(operatorToken.LeadingTrivia, GetCompoundAssignmentOperatorKind(kind), operatorToken.TrailingTrivia.AddRange(trailingTrivia)),
                binaryExpression.Right);

            newNode = newNode.WithFormatterAnnotation();

            return document.ReplaceNodeAsync(assignmentExpression, newNode, cancellationToken);
        }

        private static async Task<Document> ConvertLazyInitializationToCompoundAssignmentAsync(
            Document document,
            BinaryExpressionSyntax coalesceExpression,
            CancellationToken cancellationToken)
        {
            var parenthesizedExpression = (ParenthesizedExpressionSyntax)coalesceExpression.Right;

            var simpleAssignment = (AssignmentExpressionSyntax)parenthesizedExpression.Expression;

            ExpressionSyntax right = simpleAssignment.Right;

            AssignmentExpressionSyntax assignmentExpression = CSharpFactory.CoalesceAssignmentExpression(
                coalesceExpression.Left,
                Token(coalesceExpression.OperatorToken.LeadingTrivia, SyntaxKind.QuestionQuestionEqualsToken, coalesceExpression.OperatorToken.TrailingTrivia),
                right
                    .WithoutTrivia()
                    .WithTriviaFrom(right)
                    .AppendToTrailingTrivia(parenthesizedExpression.GetTrailingTrivia()));

            return await document.ReplaceNodeAsync(coalesceExpression, assignmentExpression, cancellationToken).ConfigureAwait(false);
        }
    }
}
