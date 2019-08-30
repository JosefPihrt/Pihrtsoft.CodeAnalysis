// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.SyntaxRewriters;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class UseListInsteadOfYieldRefactoring
    {
        public static void ComputeRefactoring(
            RefactoringContext context,
            SyntaxNode declaration,
            SemanticModel semanticModel)
        {
            if (!(semanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) is IMethodSymbol methodSymbol))
                return;

            ITypeSymbol typeSymbol = GetElementType(methodSymbol.ReturnType, semanticModel);

            if (typeSymbol?.IsErrorType() != false)
                return;

            context.RegisterRefactoring(
                "Use List instead of yield",
                cancellationToken => RefactorAsync(context.Document, declaration, typeSymbol, semanticModel, cancellationToken),
                RefactoringIdentifiers.UseListInsteadOfYield);
        }

        private static ITypeSymbol GetElementType(ITypeSymbol returnType, SemanticModel semanticModel)
        {
            if (returnType.SpecialType == SpecialType.System_Collections_IEnumerable)
                return semanticModel.Compilation.ObjectType;

            if (returnType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
                return ((INamedTypeSymbol)returnType).TypeArguments[0];

            return null;
        }

        private static Task<Document> RefactorAsync(
            Document document,
            SyntaxNode declaration,
            ITypeSymbol typeSymbol,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            BlockSyntax body = GetBody();

            SyntaxList<StatementSyntax> statements = body.Statements;

            int position = statements[0].SpanStart;

            string name = NameGenerator.Default.EnsureUniqueLocalName("items", semanticModel, position, cancellationToken: cancellationToken);

            IdentifierNameSyntax identifierName = IdentifierName(name);

            TypeSyntax listType = semanticModel
                .GetTypeByMetadataName("System.Collections.Generic.List`1")
                .Construct(typeSymbol)
                .ToMinimalTypeSyntax(semanticModel, position);

            LocalDeclarationStatementSyntax localDeclarationStatement = LocalDeclarationStatement(
                    VarType(),
                    name,
                    EqualsValueClause(ObjectCreationExpression(listType, ArgumentList())));

            localDeclarationStatement = localDeclarationStatement.WithFormatterAnnotation();

            SyntaxList<StatementSyntax> newStatements = InsertLocalDeclarationStatement(statements, localDeclarationStatement);

            ReturnStatementSyntax returnStatement = ReturnStatement(identifierName).WithFormatterAnnotation();

            newStatements = InsertReturnStatement(newStatements, returnStatement);

            BlockSyntax newBlock = body.WithStatements(newStatements);

            var rewriter = new YieldRewriter(identifierName, typeSymbol, semanticModel);

            newBlock = (BlockSyntax)rewriter.Visit(newBlock);

            return document.ReplaceNodeAsync(body, newBlock, cancellationToken);

            BlockSyntax GetBody()
            {
                switch (declaration)
                {
                    case MethodDeclarationSyntax methodDeclaration:
                        return methodDeclaration.Body;
                    case LocalFunctionStatementSyntax localFunction:
                        return localFunction.Body;
                    case AccessorDeclarationSyntax accessorDeclaration:
                        return accessorDeclaration.Body;
                }

                throw new InvalidOperationException();
            }
        }

        private static SyntaxList<StatementSyntax> InsertLocalDeclarationStatement(
            SyntaxList<StatementSyntax> statements,
            LocalDeclarationStatementSyntax localDeclarationStatement)
        {
            int insertIndex = 0;

            while (insertIndex < statements.Count
                && IsParameterCheck(statements[insertIndex]))
            {
                insertIndex++;
            }

            return statements.Insert(insertIndex, localDeclarationStatement);
        }

        private static SyntaxList<StatementSyntax> InsertReturnStatement(SyntaxList<StatementSyntax> newStatements, ReturnStatementSyntax returnStatement)
        {
            int insertIndex = newStatements.Count;

            while (insertIndex > 0
                && newStatements[insertIndex - 1].IsKind(SyntaxKind.LocalFunctionStatement))
            {
                insertIndex--;
            }

            return newStatements.Insert(insertIndex, returnStatement);
        }

        private static bool IsParameterCheck(StatementSyntax statement)
        {
            if (statement is IfStatementSyntax ifStatement)
            {
                return ifStatement.SingleNonBlockStatementOrDefault()?.Kind() == SyntaxKind.ThrowStatement;
            }

            return false;
        }

        private class YieldRewriter : SkipFunctionRewriter
        {
            private static readonly IdentifierNameSyntax _addName = IdentifierName("Add");

            private readonly IdentifierNameSyntax _identifierName;
            private readonly ITypeSymbol _typeSymbol;
            private readonly SemanticModel _semanticModel;

            public YieldRewriter(IdentifierNameSyntax identifierName, ITypeSymbol typeSymbol, SemanticModel semanticModel)
            {
                _identifierName = identifierName;
                _typeSymbol = typeSymbol;
                _semanticModel = semanticModel;
            }

            public override SyntaxNode VisitYieldStatement(YieldStatementSyntax node)
            {
                SyntaxToken keyword = node.ReturnOrBreakKeyword;
                ExpressionSyntax expression = node.Expression;

                SyntaxKind kind = node.Kind();

                if (kind == SyntaxKind.YieldReturnStatement)
                {
                    ParenthesizedExpressionSyntax parenthesizedExpression = expression.Parenthesize();

                    CastExpressionSyntax castExpression = CastExpression(
                        _typeSymbol.ToMinimalTypeSyntax(_semanticModel, node.SpanStart),
                        parenthesizedExpression);

                    InvocationExpressionSyntax invocationExpression = SimpleMemberInvocationExpression(
                        _identifierName,
                        _addName,
                        Argument(castExpression.WithSimplifierAnnotation()));

                    return ExpressionStatement(invocationExpression.WithoutTrivia())
                        .WithTriviaFrom(node)
                        .AppendToLeadingTrivia(node.DescendantTrivia(TextSpan.FromBounds(keyword.Span.End, expression.SpanStart)));
                }
                else if (kind == SyntaxKind.YieldBreakStatement)
                {
                    return ReturnStatement(
                        Token(keyword.LeadingTrivia, SyntaxKind.ReturnKeyword, keyword.TrailingTrivia),
                        _identifierName,
                        node.SemicolonToken);
                }

                Debug.Fail(node.Kind().ToString());

                return base.VisitYieldStatement(node);
            }
        }
    }
}
