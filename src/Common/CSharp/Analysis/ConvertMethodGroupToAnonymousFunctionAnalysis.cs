// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    public static class ConvertMethodGroupToAnonymousFunctionAnalysis
    {
        public static bool IsFixable(IdentifierNameSyntax identifierName, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (CanBeMethodGroup(identifierName))
            {
                IMethodSymbol methodSymbol = semanticModel.GetMethodSymbol(identifierName, cancellationToken);

                if (methodSymbol != null)
                    return true;
            }

            return false;
        }

        public static bool IsFixable(MemberAccessExpressionSyntax memberAccessExpression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (CanBeMethodGroup(memberAccessExpression))
            {
                IMethodSymbol methodSymbol = semanticModel.GetMethodSymbol(memberAccessExpression, cancellationToken);

                if (methodSymbol != null)
                    return true;
            }

            return false;
        }

        public static bool CanBeMethodGroup(ExpressionSyntax expression)
        {
            expression = expression.WalkUpParentheses();

            SyntaxNode parent = expression.Parent;

            switch (parent.Kind())
            {
                case SyntaxKind.Argument:
                case SyntaxKind.ArrayInitializerExpression:
                case SyntaxKind.ArrowExpressionClause:
                case SyntaxKind.CollectionInitializerExpression:
                case SyntaxKind.EqualsValueClause:
                case SyntaxKind.ReturnStatement:
                case SyntaxKind.YieldReturnStatement:
                    return true;
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.CoalesceAssignmentExpression:
                case SyntaxKind.SimpleAssignmentExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                    return object.ReferenceEquals(((AssignmentExpressionSyntax)parent).Right, expression);
                case SyntaxKind.SwitchExpressionArm:
                    return object.ReferenceEquals(((SwitchExpressionArmSyntax)parent).Expression, expression);
                default:
                    return false;
            }
        }
    }
}
