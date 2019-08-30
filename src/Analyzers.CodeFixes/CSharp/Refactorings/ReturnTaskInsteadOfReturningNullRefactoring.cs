// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ReturnTaskInsteadOfReturningNullRefactoring
    {
        public static async Task<Document> RefactorAsync(
            Document document,
            ConditionalAccessExpressionSyntax conditionalAccess,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax expression = conditionalAccess.Expression;

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            int position = conditionalAccess.SpanStart;

            string localName = NameGenerator.Default.EnsureUniqueLocalName(DefaultNames.Variable, semanticModel, position, cancellationToken: cancellationToken);

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(expression, cancellationToken);

            TypeSyntax type = (typeSymbol?.SupportsExplicitDeclaration() == true)
                ? typeSymbol.ToMinimalTypeSyntax(semanticModel, position)
                : VarType();

            LocalDeclarationStatementSyntax localStatement = LocalDeclarationStatement(
                type,
                Identifier(localName).WithRenameAnnotation(),
                expression);

            InvocationExpressionSyntax newExpression = CreateTaskFromResultExpression(document, conditionalAccess, semanticModel, cancellationToken);

            IfStatementSyntax ifStatement = IfStatement(
                NotEqualsExpression(IdentifierName(localName), NullLiteralExpression()),
                Block(ReturnStatement(conditionalAccess.RemoveOperatorToken())),
                ElseClause(Block(ReturnStatement(newExpression))));

            SyntaxList<StatementSyntax> statements = List(new StatementSyntax[] { localStatement, ifStatement });

            SyntaxNode parent = conditionalAccess.Parent;

            if (parent is ReturnStatementSyntax returnStatement)
            {
                statements = statements.WithTriviaFrom(returnStatement);

                if (returnStatement.IsEmbedded())
                {
                    return await document.ReplaceNodeAsync(returnStatement, Block(statements), cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    return await document.ReplaceNodeAsync(returnStatement, statements, cancellationToken).ConfigureAwait(false);
                }
            }
            else if (parent is SimpleLambdaExpressionSyntax simpleLambda)
            {
                SimpleLambdaExpressionSyntax newNode = simpleLambda
                    .WithBody(Block(statements))
                    .WithFormatterAnnotation();

                return await document.ReplaceNodeAsync(simpleLambda, newNode, cancellationToken).ConfigureAwait(false);
            }
            else if (parent is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
            {
                ParenthesizedLambdaExpressionSyntax newNode = parenthesizedLambda
                    .WithBody(Block(statements))
                    .WithFormatterAnnotation();

                return await document.ReplaceNodeAsync(parenthesizedLambda, newNode, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var arrowExpressionClause = (ArrowExpressionClauseSyntax)parent;

                SyntaxNode node = arrowExpressionClause.Parent;

                SyntaxNode newNode = CreateNewNode(node).WithFormatterAnnotation();

                return await document.ReplaceNodeAsync(node, newNode, cancellationToken).ConfigureAwait(false);
            }

            SyntaxNode CreateNewNode(SyntaxNode node)
            {
                switch (node)
                {
                    case MethodDeclarationSyntax methodDeclaration:
                        {
                            return methodDeclaration
                                .WithExpressionBody(null)
                                .WithBody(Block(statements));
                        }
                    case LocalFunctionStatementSyntax localFunction:
                        {
                            return localFunction
                                .WithExpressionBody(null)
                                .WithBody(Block(statements));
                        }
                    case PropertyDeclarationSyntax propertyDeclaration:
                        {
                            return propertyDeclaration
                                .WithExpressionBody(null)
                                .WithSemicolonToken(default(SyntaxToken))
                                .WithAccessorList(AccessorList(GetAccessorDeclaration(Block(statements))));
                        }
                    case IndexerDeclarationSyntax indexerDeclaration:
                        {
                            return indexerDeclaration
                                .WithExpressionBody(null)
                                .WithSemicolonToken(default(SyntaxToken))
                                .WithAccessorList(AccessorList(GetAccessorDeclaration(Block(statements))));
                        }
                    case AccessorDeclarationSyntax accessorDeclaration:
                        {
                            return accessorDeclaration
                                .WithExpressionBody(null)
                                .WithSemicolonToken(default(SyntaxToken))
                                .WithBody(Block(statements));
                        }
                    default:
                        {
                            Debug.Fail(node.Kind().ToString());
                            return node;
                        }
                }
            }
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            ExpressionSyntax expression,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            ExpressionSyntax newExpression = CreateTaskFromResultExpression(document, expression, semanticModel, cancellationToken);

            return await document.ReplaceNodeAsync(expression, newExpression, cancellationToken).ConfigureAwait(false);
        }

        private static InvocationExpressionSyntax CreateTaskFromResultExpression(
            Document document,
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            var typeSymbol = (INamedTypeSymbol)semanticModel.GetTypeInfo(expression, cancellationToken).ConvertedType;

            int position = expression.SpanStart;

            ITypeSymbol typeArgument = typeSymbol.TypeArguments[0];

            TypeSyntax type = typeArgument.ToTypeSyntax().WithSimplifierAnnotation();

            ExpressionSyntax defaultValue = typeArgument.GetDefaultValueSyntax(document.GetDefaultSyntaxOptions(), type);

            SimpleNameSyntax name;

            if (defaultValue.IsKind(
                SyntaxKind.TrueLiteralExpression,
                SyntaxKind.FalseLiteralExpression,
                SyntaxKind.CharacterLiteralExpression,
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxKind.DefaultExpression))
            {
                name = IdentifierName("FromResult");
            }
            else
            {
                name = GenericName("FromResult", type);
            }

            InvocationExpressionSyntax newNode = SimpleMemberInvocationExpression(
                ParseTypeName("System.Threading.Tasks.Task").WithSimplifierAnnotation(),
                name,
                Argument(defaultValue));

            return newNode.WithTriviaFrom(expression);
        }
    }
}
