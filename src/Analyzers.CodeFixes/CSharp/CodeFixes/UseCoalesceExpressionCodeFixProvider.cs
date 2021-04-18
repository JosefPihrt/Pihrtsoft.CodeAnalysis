// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
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
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCoalesceExpressionCodeFixProvider))]
    [Shared]
    public sealed class UseCoalesceExpressionCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Use coalesce expression";

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UseCoalesceExpression); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SyntaxNode node, predicate: f => f.IsKind(SyntaxKind.InvocationExpression) || f is StatementSyntax))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];

            if (node is StatementSyntax statement)
            {
                CodeAction codeAction = CodeAction.Create(
                    Title,
                    ct => RefactorAsync(context.Document, statement, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
            else
            {
                CodeAction codeAction = CodeAction.Create(
                    Title,
                    ct => RefactorAsync(context.Document, (InvocationExpressionSyntax)node, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            StatementSyntax statement,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(statement);

            SyntaxList<StatementSyntax> statements = statementsInfo.Statements;

            int index = statements.IndexOf(statement);

            switch (statement)
            {
                case IfStatementSyntax ifStatement:
                    {
                        var expressionStatement = (ExpressionStatementSyntax)ifStatement.SingleNonBlockStatementOrDefault();

                        var assignment = (AssignmentExpressionSyntax)expressionStatement.Expression;

                        ExpressionSyntax left = assignment.Left;
                        ExpressionSyntax right = assignment.Right;

                        BinaryExpressionSyntax coalesceExpression = CreateCoalesceExpression(
                            left.WithoutLeadingTrivia().WithTrailingTrivia(Space),
                            right.WithLeadingTrivia(Space),
                            semanticModel.GetTypeSymbol(left, cancellationToken));

                        AssignmentExpressionSyntax newAssignment = assignment.WithRight(coalesceExpression.WithTriviaFrom(right));

                        ExpressionStatementSyntax newNode = expressionStatement.WithExpression(newAssignment);

                        IEnumerable<SyntaxTrivia> trivia = ifStatement.DescendantTrivia(TextSpan.FromBounds(ifStatement.SpanStart, expressionStatement.SpanStart));

                        if (trivia.All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                        {
                            newNode = newNode.WithLeadingTrivia(ifStatement.GetLeadingTrivia());
                        }
                        else
                        {
                            newNode = newNode
                                .WithLeadingTrivia(ifStatement.GetLeadingTrivia().Concat(trivia))
                                .WithFormatterAnnotation();
                        }

                        return await document.ReplaceNodeAsync(ifStatement, newNode, cancellationToken).ConfigureAwait(false);
                    }
                case ExpressionStatementSyntax expressionStatement:
                    {
                        var assignment = (AssignmentExpressionSyntax)expressionStatement.Expression;

                        return await RefactorAsync(document, expressionStatement, (IfStatementSyntax)statements[index + 1], index, statementsInfo, assignment.Right, semanticModel, cancellationToken).ConfigureAwait(false);
                    }
                case LocalDeclarationStatementSyntax localDeclaration:
                    {
                        ExpressionSyntax value = localDeclaration
                            .Declaration
                            .Variables[0]
                            .Initializer
                            .Value;

                        return await RefactorAsync(document, localDeclaration, (IfStatementSyntax)statements[index + 1], index, statementsInfo, value, semanticModel, cancellationToken).ConfigureAwait(false);
                    }
                default:
                    {
                        Debug.Fail(statement.Kind().ToString());

                        return document;
                    }
            }
        }

        private static Task<Document> RefactorAsync(
            Document document,
            StatementSyntax statement,
            IfStatementSyntax ifStatement,
            int statementIndex,
            in StatementListInfo statementsInfo,
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            var expressionStatement = (ExpressionStatementSyntax)ifStatement.SingleNonBlockStatementOrDefault();

            var assignment = (AssignmentExpressionSyntax)expressionStatement.Expression;

            BinaryExpressionSyntax newNode = CreateCoalesceExpression(
                expression.WithoutTrailingTrivia(),
                assignment.Right.WithTrailingTrivia(expression.GetTrailingTrivia()),
                semanticModel.GetTypeSymbol(assignment.Left, cancellationToken));

            StatementSyntax newStatement = statement.ReplaceNode(expression, newNode);

            IEnumerable<SyntaxTrivia> trivia = statementsInfo.Parent.DescendantTrivia(TextSpan.FromBounds(statement.Span.End, ifStatement.Span.End));

            if (!trivia.All(f => f.IsWhitespaceOrEndOfLineTrivia()))
            {
                newStatement = newStatement.WithTrailingTrivia(trivia);
                newStatement = newStatement.AppendToTrailingTrivia(ifStatement.GetTrailingTrivia());
            }
            else
            {
                newStatement = newStatement.WithTrailingTrivia(ifStatement.GetTrailingTrivia());
            }

            SyntaxList<StatementSyntax> newStatements = statementsInfo.Statements
                .Remove(ifStatement)
                .ReplaceAt(statementIndex, newStatement);

            return document.ReplaceStatementsAsync(statementsInfo, newStatements, cancellationToken);
        }

        private static BinaryExpressionSyntax CreateCoalesceExpression(
            ExpressionSyntax left,
            ExpressionSyntax right,
            ITypeSymbol targetType)
        {
            if (targetType?.SupportsExplicitDeclaration() == true)
            {
                right = CastExpression(
                    targetType.ToTypeSyntax().WithSimplifierAnnotation(),
                    right.Parenthesize())
                    .WithSimplifierAnnotation();
            }

            return CoalesceExpression(
                left.Parenthesize(),
                right.Parenthesize());
        }

        private static Task<Document> RefactorAsync(
            Document document,
            InvocationExpressionSyntax invocationExpression,
            CancellationToken cancellationToken)
        {
            var memberAccessExpression = (MemberAccessExpressionSyntax)invocationExpression.Expression;

            ExpressionSyntax expression = memberAccessExpression.Expression;

            ExpressionSyntax topExpression = CSharpUtility.GetTopmostExpressionInCallChain(invocationExpression);

            if (topExpression != invocationExpression)
            {
                expression = ParseExpression(topExpression.ToString(TextSpan.FromBounds(topExpression.FullSpan.Start, expression.FullSpan.End)));
            }

            ExpressionSyntax newNode = CoalesceExpression(
                expression.Parenthesize(),
                invocationExpression.ArgumentList.Arguments.Single().Expression.Parenthesize());

            newNode = newNode.Parenthesize().WithFormatterAnnotation();

            return document.ReplaceNodeAsync(
                (topExpression != invocationExpression) ? topExpression : invocationExpression,
                newNode,
                cancellationToken);
        }
    }
}
