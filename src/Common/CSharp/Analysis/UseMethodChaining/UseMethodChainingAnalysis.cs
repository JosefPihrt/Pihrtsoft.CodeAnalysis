// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis.UseMethodChaining
{
    internal abstract class UseMethodChainingAnalysis
    {
        public static MethodChainingWithoutAssignmentAnalysis WithoutAssignmentAnalysis { get; } = new MethodChainingWithoutAssignmentAnalysis();

        public static MethodChainingWithAssignmentAnalysis WithAssignmentAnalysis { get; } = new MethodChainingWithAssignmentAnalysis();

        public static bool IsFixable(
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            SyntaxNode parent = invocationExpression.Parent;

            switch (parent?.Kind())
            {
                case SyntaxKind.ExpressionStatement:
                    {
                        var expressionStatement = (ExpressionStatementSyntax)parent;

                        if (!(WalkDownMethodChain(invocationInfo).Expression is IdentifierNameSyntax identifierName))
                            break;

                        string name = identifierName.Identifier.ValueText;

                        return WithoutAssignmentAnalysis.Analyze(invocationInfo, expressionStatement, name, semanticModel, cancellationToken);
                    }
                case SyntaxKind.SimpleAssignmentExpression:
                    {
                        var assignmentExpression = (AssignmentExpressionSyntax)parent;

                        if (!(assignmentExpression.Left is IdentifierNameSyntax identifierName))
                            break;

                        if (assignmentExpression.Right != invocationExpression)
                            break;

                        if (!(assignmentExpression.Parent is ExpressionStatementSyntax expressionStatement))
                            break;

                        string name = identifierName.Identifier.ValueText;

                        if (name != (WalkDownMethodChain(invocationInfo).Expression as IdentifierNameSyntax)?.Identifier.ValueText)
                            break;

                        return WithAssignmentAnalysis.Analyze(invocationInfo, expressionStatement, name, semanticModel, cancellationToken);
                    }
            }

            return false;
        }

        public bool Analyze(
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            StatementSyntax statement,
            string name,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (statement.SpanOrTrailingTriviaContainsDirectives())
                return false;

            StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(statement);

            if (!statementsInfo.Success)
                return false;

            SyntaxList<StatementSyntax> statements = statementsInfo.Statements;

            if (statements.Count == 1)
                return false;

            IMethodSymbol methodSymbol = semanticModel.GetMethodSymbol(invocationInfo.InvocationExpression, cancellationToken);

            if (methodSymbol == null)
                return false;

            ITypeSymbol returnType = methodSymbol.ReturnType;

            int i = statements.IndexOf(statement);

            if (i != 0
                && IsFixableStatement(statements[i - 1], name, returnType, semanticModel, cancellationToken))
            {
                return false;
            }

            int j = i;
            while (j < statements.Count - 1)
            {
                if (!IsFixableStatement(statements[j + 1], name, returnType, semanticModel, cancellationToken))
                    break;

                j++;
            }

            return j > i;
        }

        public abstract bool IsFixableStatement(
            StatementSyntax statement,
            string name,
            ITypeSymbol typeSymbol,
            SemanticModel semanticModel,
            CancellationToken cancellationToken);

        public static SimpleMemberInvocationExpressionInfo WalkDownMethodChain(SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            while (true)
            {
                SimpleMemberInvocationExpressionInfo invocationInfo2 = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocationInfo.Expression);

                if (invocationInfo2.Success)
                {
                    invocationInfo = invocationInfo2;
                }
                else
                {
                    break;
                }
            }

            return invocationInfo;
        }
    }
}