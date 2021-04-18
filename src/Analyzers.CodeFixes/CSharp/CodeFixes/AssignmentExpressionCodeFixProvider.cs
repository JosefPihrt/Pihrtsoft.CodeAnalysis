// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
using Roslynator.CSharp.Analysis;
using Roslynator.CSharp.Refactorings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AssignmentExpressionCodeFixProvider))]
    [Shared]
    public sealed class AssignmentExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.UseUnaryOperatorInsteadOfAssignment,
                    DiagnosticIdentifiers.RemoveRedundantDelegateCreation);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out AssignmentExpressionSyntax assignment))
                return;

            Document document = context.Document;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.UseUnaryOperatorInsteadOfAssignment:
                        {
                            string operatorText = UseUnaryOperatorInsteadOfAssignmentAnalyzer.GetOperatorText(assignment);

                            CodeAction codeAction = CodeAction.Create(
                                $"Use {operatorText} operator",
                                ct => UseUnaryOperatorInsteadOfAssignmentAsync(document, assignment, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.RemoveRedundantDelegateCreation:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove redundant delegate creation",
                                cancellationToken =>
                                {
                                    return RemoveRedundantDelegateCreationRefactoring.RefactorAsync(
                                        document,
                                        (ObjectCreationExpressionSyntax)assignment.Right,
                                        cancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static Task<Document> UseUnaryOperatorInsteadOfAssignmentAsync(
            Document document,
            AssignmentExpressionSyntax assignment,
            CancellationToken cancellationToken = default)
        {
            ExpressionSyntax unaryExpression;
            if (assignment.IsParentKind(SyntaxKind.ExpressionStatement))
            {
                SyntaxKind kind = (UseUnaryOperatorInsteadOfAssignmentAnalyzer.UseIncrementOperator(assignment))
                    ? SyntaxKind.PostIncrementExpression
                    : SyntaxKind.PostDecrementExpression;

                unaryExpression = PostfixUnaryExpression(kind, assignment.Left);
            }
            else
            {
                SyntaxKind kind = (UseUnaryOperatorInsteadOfAssignmentAnalyzer.UseIncrementOperator(assignment))
                    ? SyntaxKind.PreIncrementExpression
                    : SyntaxKind.PreDecrementExpression;

                unaryExpression = PrefixUnaryExpression(kind, assignment.Left);
            }

            unaryExpression = unaryExpression
                .WithTrailingTrivia(GetTrailingTrivia())
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(assignment, unaryExpression, cancellationToken);

            List<SyntaxTrivia> GetTrailingTrivia()
            {
                var trivia = new List<SyntaxTrivia>();

                ExpressionSyntax right = assignment.Right;

                switch (assignment.Kind())
                {
                    case SyntaxKind.AddAssignmentExpression:
                    case SyntaxKind.SubtractAssignmentExpression:
                        {
                            trivia.AddRange(assignment.OperatorToken.GetAllTrivia());

                            if (right?.IsMissing == false)
                                trivia.AddRange(right.GetLeadingAndTrailingTrivia());

                            return trivia;
                        }
                }

                switch (right?.Kind())
                {
                    case SyntaxKind.AddExpression:
                    case SyntaxKind.SubtractExpression:
                        {
                            trivia.AddRange(assignment.OperatorToken.GetAllTrivia());

                            if (right?.IsMissing == false)
                            {
                                var binaryExpression = (BinaryExpressionSyntax)right;

                                trivia.AddRange(binaryExpression.DescendantTrivia());
                            }

                            return trivia;
                        }
                }

                throw new InvalidOperationException();
            }
        }
    }
}
