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
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveRedundantAssignmentCodeFixProvider))]
    [Shared]
    public sealed class RemoveRedundantAssignmentCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.RemoveRedundantAssignment); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SyntaxNode node, predicate: f => f.IsKind(SyntaxKind.VariableDeclarator) || f is AssignmentExpressionSyntax))
                return;

            Document document = context.Document;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.RemoveRedundantAssignment:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove redundant assignment",
                                ct =>
                                {
                                    if (node.IsKind(SyntaxKind.VariableDeclarator))
                                    {
                                        var variableDeclarator = (VariableDeclaratorSyntax)node;

                                        return RemoveRedundantAssignmentAfterLocalDeclarationAsync(document, variableDeclarator, ct);
                                    }
                                    else
                                    {
                                        var assignment = (AssignmentExpressionSyntax)node;

                                        return RemoveRedundantAssignmentBeforeReturnStatementAsync(document, assignment, ct);
                                    }
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        public static Task<Document> RemoveRedundantAssignmentAfterLocalDeclarationAsync(
            Document document,
            VariableDeclaratorSyntax declarator,
            CancellationToken cancellationToken = default)
        {
            var declaration = (VariableDeclarationSyntax)declarator.Parent;

            var localDeclaration = (LocalDeclarationStatementSyntax)declaration.Parent;

            StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(localDeclaration);

            SyntaxList<StatementSyntax> statements = statementsInfo.Statements;

            int index = statements.IndexOf(localDeclaration);

            StatementSyntax nextStatement = statements[index + 1];

            var expressionStatement = (ExpressionStatementSyntax)nextStatement;

            var assignment = (AssignmentExpressionSyntax)expressionStatement.Expression;

            ExpressionSyntax right = assignment.Right;

            EqualsValueClauseSyntax initializer = declarator.Initializer;

            ExpressionSyntax value = initializer?.Value;

            VariableDeclaratorSyntax newDeclarator = (value != null)
                ? declarator.ReplaceNode(value, right)
                : declarator.WithInitializer(EqualsValueClause(right));

            LocalDeclarationStatementSyntax newLocalDeclaration = localDeclaration.ReplaceNode(declarator, newDeclarator);

            SyntaxTriviaList trailingTrivia = localDeclaration.GetTrailingTrivia();

            if (!trailingTrivia.IsEmptyOrWhitespace())
            {
                newLocalDeclaration = newLocalDeclaration.WithTrailingTrivia(trailingTrivia.Concat(nextStatement.GetTrailingTrivia()));
            }
            else
            {
                newLocalDeclaration = newLocalDeclaration.WithTrailingTrivia(nextStatement.GetTrailingTrivia());
            }

            SyntaxTriviaList leadingTrivia = nextStatement.GetLeadingTrivia();

            if (!leadingTrivia.IsEmptyOrWhitespace())
                newLocalDeclaration = newLocalDeclaration.WithLeadingTrivia(newLocalDeclaration.GetLeadingTrivia().Concat(leadingTrivia));

            SyntaxList<StatementSyntax> newStatements = statements
                .Replace(localDeclaration, newLocalDeclaration)
                .RemoveAt(index + 1);

            return document.ReplaceStatementsAsync(statementsInfo, newStatements, cancellationToken);
        }

        private static Task<Document> RemoveRedundantAssignmentBeforeReturnStatementAsync(
            Document document,
            AssignmentExpressionSyntax assignmentExpression,
            CancellationToken cancellationToken)
        {
            var statement = (StatementSyntax)assignmentExpression.Parent;

            StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(statement);

            SyntaxList<StatementSyntax> statements = statementsInfo.Statements;

            int index = statements.IndexOf(statement);

            statements = statements.RemoveAt(index);

            var returnStatement = (ReturnStatementSyntax)statement.NextStatement();

            SyntaxTriviaList trivia = statementsInfo
                .Parent
                .DescendantTrivia(TextSpan.FromBounds(statement.SpanStart, returnStatement.SpanStart))
                .ToSyntaxTriviaList()
                .EmptyIfWhitespace();

            trivia = statement
                .GetLeadingTrivia()
                .AddRange(trivia);

            returnStatement = returnStatement
                .WithExpression(assignmentExpression.Right.WithTriviaFrom(returnStatement.Expression))
                .WithLeadingTrivia(trivia)
                .WithFormatterAnnotation();

            statements = statements.ReplaceAt(index, returnStatement);

            return document.ReplaceStatementsAsync(statementsInfo, statements, cancellationToken);
        }
    }
}
