// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Analysis.UseMethodChaining;
using Roslynator.CSharp.Refactorings;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StatementCodeFixProvider))]
    [Shared]
    public sealed class StatementCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.InlineLazyInitialization,
                    DiagnosticIdentifiers.RemoveRedundantDisposeOrCloseCall,
                    DiagnosticIdentifiers.RemoveRedundantStatement,
                    DiagnosticIdentifiers.UseMethodChaining);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out StatementSyntax statement))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.InlineLazyInitialization:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Inline lazy initialization",
                                cancellationToken =>
                                {
                                    return InlineLazyInitializationAsync(
                                        context.Document,
                                        (IfStatementSyntax)statement,
                                        cancellationToken);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.RemoveRedundantDisposeOrCloseCall:
                        {
                            var expressionStatement = (ExpressionStatementSyntax)statement;
                            var invocation = (InvocationExpressionSyntax)expressionStatement.Expression;
                            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

                            CodeAction codeAction = CodeAction.Create(
                                $"Remove redundant '{memberAccess.Name?.Identifier.ValueText}' call",
                                cancellationToken => RemoveRedundantDisposeOrCloseCallRefactoring.RefactorAsync(context.Document, expressionStatement, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.RemoveRedundantStatement:
                        {
                            CodeAction codeAction = CodeActionFactory.RemoveStatement(
                                context.Document,
                                statement,
                                title: $"Remove redundant {CSharpFacts.GetTitle(statement)}",
                                equivalenceKey: GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UseMethodChaining:
                        {
                            var expressionStatement = (ExpressionStatementSyntax)statement;

                            UseMethodChainingAnalysis analysis;
                            if (expressionStatement.Expression.Kind() == SyntaxKind.InvocationExpression)
                            {
                                analysis = UseMethodChainingAnalysis.WithoutAssignmentAnalysis;
                            }
                            else
                            {
                                analysis = UseMethodChainingAnalysis.WithAssignmentAnalysis;
                            }

                            CodeAction codeAction = CodeAction.Create(
                                "Use method chaining",
                                cancellationToken => UseMethodChainingRefactoring.RefactorAsync(context.Document, analysis, expressionStatement, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        public static Task<Document> InlineLazyInitializationAsync(
            Document document,
            IfStatementSyntax ifStatement,
            CancellationToken cancellationToken = default)
        {
            StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(ifStatement);

            var assignmentStatement = (ExpressionStatementSyntax)ifStatement.SingleNonBlockStatementOrDefault();

            SimpleAssignmentStatementInfo assignmentInfo = SyntaxInfo.SimpleAssignmentStatementInfo(assignmentStatement, walkDownParentheses: false);

            ExpressionSyntax right = assignmentInfo.Right;

            int index = statementsInfo.IndexOf(ifStatement);

            var expressionStatement2 = (ExpressionStatementSyntax)statementsInfo[index + 1];

            SimpleMemberInvocationStatementInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationStatementInfo(expressionStatement2);

            ExpressionSyntax expression = invocationInfo.Expression;

            var newLeading = new List<SyntaxTrivia>(ifStatement.GetLeadingTrivia());

            ExpressionSyntax coalesceExpression;

            if (document.SupportsLanguageFeature(CSharpLanguageFeature.NullCoalescingAssignmentOperator))
            {
                AddTrivia(ifStatement.DescendantTrivia(TextSpan.FromBounds(ifStatement.SpanStart, right.SpanStart)).ToSyntaxTriviaList());

                coalesceExpression = CoalesceAssignmentExpression(expression.WithoutTrivia(), right.WithoutTrivia());
            }
            else
            {
                AddTrivia(ifStatement.DescendantTrivia(TextSpan.FromBounds(ifStatement.SpanStart, assignmentInfo.AssignmentExpression.SpanStart)).ToSyntaxTriviaList());

                coalesceExpression = CoalesceExpression(expression.WithoutTrivia(), ParenthesizedExpression(assignmentInfo.AssignmentExpression.WithoutTrivia()));
            }

            AddTrivia(ifStatement.DescendantTrivia(TextSpan.FromBounds(right.Span.End, ifStatement.Span.End)).ToSyntaxTriviaList());
            AddTrivia(ifStatement.GetTrailingTrivia());
            AddTrivia(expressionStatement2.GetLeadingTrivia());

            ParenthesizedExpressionSyntax newExpression = ParenthesizedExpression(coalesceExpression)
                .WithLeadingTrivia(newLeading)
                .WithTrailingTrivia(expression.GetTrailingTrivia());

            StatementSyntax newExpressionStatement = expressionStatement2.ReplaceNode(expression, newExpression);

            StatementListInfo newStatements = statementsInfo
                .Replace(expressionStatement2, newExpressionStatement)
                .RemoveAt(index);

            return document.ReplaceStatementsAsync(statementsInfo, newStatements, cancellationToken);

            void AddTrivia(SyntaxTriviaList trivia)
            {
                if (!trivia.IsEmptyOrWhitespace())
                    newLeading.AddRange(trivia);
            }
        }
    }
}
