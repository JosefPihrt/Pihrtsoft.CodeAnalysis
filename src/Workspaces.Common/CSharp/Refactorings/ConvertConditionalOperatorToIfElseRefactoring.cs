// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeActions;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ConvertConditionalOperatorToIfElseRefactoring
    {
        public const string Title = "Convert ?: to if-else";

        public const string RecursiveTitle = Title + " (recursively)";

        public static (CodeAction codeAction, CodeAction recursiveCodeAction) ComputeRefactoring(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            in CodeActionData data,
            in CodeActionData recursiveData,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default)
        {
            CodeAction codeAction = null;
            CodeAction recursiveCodeAction = null;

            ExpressionSyntax expression = conditionalExpression.WalkUpParentheses();

            SyntaxNode parent = expression.Parent;

            if (parent.IsKind(SyntaxKind.ReturnStatement, SyntaxKind.YieldReturnStatement))
            {
                var statement = (StatementSyntax)parent;

                if (!data.IsDefault)
                    codeAction = CreateCodeAction(document, conditionalExpression, statement, data);

                if (!recursiveData.IsDefault && IsRecursive())
                    recursiveCodeAction = CreateCodeAction(document, conditionalExpression, statement, recursiveData, recursive: true);
            }
            else if (parent is AssignmentExpressionSyntax assignment)
            {
                if (assignment.Parent is ExpressionStatementSyntax expressionStatement)
                {
                    if (!data.IsDefault)
                        codeAction = CreateCodeAction(document, conditionalExpression, expressionStatement, data);

                    if (!recursiveData.IsDefault && IsRecursive())
                        recursiveCodeAction = CreateCodeAction(document, conditionalExpression, expressionStatement, recursiveData, recursive: true);
                }
            }
            else
            {
                SingleLocalDeclarationStatementInfo localDeclarationInfo = SyntaxInfo.SingleLocalDeclarationStatementInfo(expression);

                if (localDeclarationInfo.Success)
                {
                    TypeSyntax type = localDeclarationInfo.Type;

                    if (!type.IsVar
                        || semanticModel.GetTypeSymbol(type, cancellationToken)?.SupportsExplicitDeclaration() == true)
                    {
                        LocalDeclarationStatementSyntax statement = localDeclarationInfo.Statement;

                        if (!data.IsDefault)
                            codeAction = CreateCodeAction(document, conditionalExpression, statement, semanticModel, data);

                        if (!recursiveData.IsDefault && IsRecursive())
                            recursiveCodeAction = CreateCodeAction(document, conditionalExpression, statement, semanticModel, recursiveData, recursive: true);
                    }
                }
            }

            return (codeAction, recursiveCodeAction);

            bool IsRecursive()
            {
                return conditionalExpression.WhenTrue.WalkDownParentheses().IsKind(SyntaxKind.ConditionalExpression)
                    || conditionalExpression.WhenFalse.WalkDownParentheses().IsKind(SyntaxKind.ConditionalExpression);
            }
        }

        private static CodeAction CreateCodeAction(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            StatementSyntax statement,
            in CodeActionData data,
            bool recursive = false)
        {
            return data.ToCodeAction(ct => RefactorAsync(document, conditionalExpression, statement, recursive: recursive, cancellationToken: ct));
        }

        private static CodeAction CreateCodeAction(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            LocalDeclarationStatementSyntax localDeclaration,
            SemanticModel semanticModel,
            in CodeActionData data,
            bool recursive = false)
        {
            return data.ToCodeAction(ct => RefactorAsync(document, conditionalExpression, localDeclaration, semanticModel, recursive: recursive, cancellationToken: ct));
        }

        private static Task<Document> RefactorAsync(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            StatementSyntax statement,
            bool recursive,
            CancellationToken cancellationToken)
        {
            StatementSyntax newStatement = statement.TrimTrivia();

            IfStatementSyntax ifElseStatement = ConvertConditionalExpressionToIfElse(
                conditionalExpression,
                expression => CreateNewStatement(newStatement, expression),
                recursive: recursive);

            ifElseStatement = ifElseStatement
                .WithTriviaFrom(statement)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(statement, ifElseStatement, cancellationToken);
        }

        private static StatementSyntax CreateNewStatement(StatementSyntax statement, ExpressionSyntax expression)
        {
            switch (statement.Kind())
            {
                case SyntaxKind.ReturnStatement:
                    {
                        return ((ReturnStatementSyntax)statement).WithExpression(expression);
                    }
                case SyntaxKind.YieldReturnStatement:
                    {
                        return ((YieldStatementSyntax)statement).WithExpression(expression);
                    }
                case SyntaxKind.ExpressionStatement:
                    {
                        var expressionStatement = (ExpressionStatementSyntax)statement;

                        var assignment = (AssignmentExpressionSyntax)expressionStatement.Expression;

                        return expressionStatement.WithExpression(assignment.WithRight(expression));
                    }
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
        }

        private static Task<Document> RefactorAsync(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            LocalDeclarationStatementSyntax localDeclaration,
            SemanticModel semanticModel,
            bool recursive,
            CancellationToken cancellationToken)
        {
            VariableDeclaratorSyntax variableDeclarator = localDeclaration.Declaration.Variables[0];

            LocalDeclarationStatementSyntax newLocalDeclaration = localDeclaration.RemoveNode(variableDeclarator.Initializer, SyntaxRemoveOptions.KeepExteriorTrivia);

            TypeSyntax type = newLocalDeclaration.Declaration.Type;

            if (type.IsVar)
            {
                newLocalDeclaration = newLocalDeclaration.ReplaceNode(
                    type,
                    semanticModel.GetTypeSymbol(conditionalExpression, cancellationToken)
                        .ToMinimalTypeSyntax(semanticModel, type.SpanStart)
                        .WithTriviaFrom(type));
            }

            IdentifierNameSyntax left = IdentifierName(variableDeclarator.Identifier.ValueText);

            IfStatementSyntax ifElseStatement = ConvertConditionalExpressionToIfElse(conditionalExpression, expression => SimpleAssignmentStatement(left, expression), recursive: recursive);

            StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(localDeclaration);

            SyntaxList<StatementSyntax> statements = statementsInfo.Statements;

            SyntaxList<StatementSyntax> newStatements = statements
                .Replace(localDeclaration, newLocalDeclaration.WithFormatterAnnotation())
                .Insert(statements.IndexOf(localDeclaration) + 1, ifElseStatement.WithFormatterAnnotation());

            return document.ReplaceStatementsAsync(statementsInfo, newStatements, cancellationToken);
        }

        private static IfStatementSyntax ConvertConditionalExpressionToIfElse(
            ConditionalExpressionSyntax conditionalExpression,
            Func<ExpressionSyntax, StatementSyntax> createStatement,
            bool recursive = false)
        {
            StatementSyntax statement = null;

            if (recursive
                && conditionalExpression.WhenTrue.WalkDownParentheses() is ConditionalExpressionSyntax whenTrue)
            {
                statement = ConvertConditionalExpressionToIfElse(whenTrue, createStatement, recursive: true);
            }
            else
            {
                statement = createStatement(conditionalExpression.WhenTrue.WithoutTrivia());
            }

            ElseClauseSyntax elseClause;

            if (recursive
                && conditionalExpression.WhenFalse.WalkDownParentheses() is ConditionalExpressionSyntax whenFalse)
            {
                IfStatementSyntax ifElseStatement = ConvertConditionalExpressionToIfElse(whenFalse, createStatement, recursive: true);

                elseClause = ElseClause(ifElseStatement);
            }
            else
            {
                elseClause = ElseClause(Block(createStatement(conditionalExpression.WhenFalse.WithoutTrivia())));
            }

            return IfStatement(
                conditionalExpression.Condition.WalkDownParentheses().WithoutTrivia(),
                Block(statement),
                elseClause);
        }
    }
}
