// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class IntroduceLocalVariableRefactoring
    {
        internal static string GetTitle(ExpressionSyntax expression)
        {
            return $"Introduce local for '{expression}'";
        }

        internal static Task<Document> RefactorAsync(
            Document document,
            ExpressionStatementSyntax expressionStatement,
            ITypeSymbol typeSymbol,
            bool addAwait,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (addAwait)
                typeSymbol = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];

            string name = NameGenerator.CreateName(typeSymbol, firstCharToLower: true) ?? DefaultNames.Variable;

            name = NameGenerator.Default.EnsureUniqueLocalName(
                name,
                semanticModel,
                expressionStatement.SpanStart,
                cancellationToken: cancellationToken);

            ExpressionSyntax value = expressionStatement.Expression;

            if (addAwait)
                value = AwaitExpression(value);

            LocalDeclarationStatementSyntax newNode = LocalDeclarationStatement(
                VarType(),
                Identifier(name).WithRenameAnnotation(),
                value);

            newNode = newNode
                .WithTriviaFrom(expressionStatement)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(expressionStatement, newNode, cancellationToken);
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            UsingStatementSyntax usingStatement,
            ExpressionSyntax expression,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            string name = NameGenerator.Default.EnsureUniqueLocalName(DefaultNames.Variable, semanticModel, expression.SpanStart, cancellationToken: cancellationToken);

            VariableDeclarationSyntax declaration = VariableDeclaration(
                VarType(),
                Identifier(name).WithRenameAnnotation(),
                EqualsValueClause(expression.WithoutTrivia()));

            declaration = declaration
                .WithTriviaFrom(expression)
                .WithFormatterAnnotation();

            UsingStatementSyntax newNode = usingStatement
                .WithExpression(null)
                .WithDeclaration(declaration);

            return await document.ReplaceNodeAsync(usingStatement, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}