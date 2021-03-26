// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
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
                case SyntaxKind.AddExpression:
                case SyntaxKind.AddressOfExpression:
                case SyntaxKind.AliasQualifiedName:
                case SyntaxKind.AndAssignmentExpression:
                case SyntaxKind.AnonymousObjectMemberDeclarator:
                case SyntaxKind.ArrayRankSpecifier:
                case SyntaxKind.ArrayType:
                case SyntaxKind.AsExpression:
                case SyntaxKind.Attribute:
                case SyntaxKind.AttributeArgument:
                case SyntaxKind.AwaitExpression:
                case SyntaxKind.BitwiseAndExpression:
                case SyntaxKind.BitwiseNotExpression:
                case SyntaxKind.BitwiseOrExpression:
                case SyntaxKind.CaseSwitchLabel:
                case SyntaxKind.CastExpression:
                case SyntaxKind.CatchDeclaration:
                case SyntaxKind.CoalesceExpression:
                case SyntaxKind.ConditionalAccessExpression:
                case SyntaxKind.ConditionalExpression:
                case SyntaxKind.ConstantPattern:
                case SyntaxKind.ConversionOperatorDeclaration:
                case SyntaxKind.DeclarationExpression:
                case SyntaxKind.DeclarationPattern:
                case SyntaxKind.DefaultExpression:
                case SyntaxKind.DelegateDeclaration:
                case SyntaxKind.DivideAssignmentExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.DoStatement:
                case SyntaxKind.ElementAccessExpression:
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.EventDeclaration:
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                case SyntaxKind.ExclusiveOrExpression:
                case SyntaxKind.ExplicitInterfaceSpecifier:
                case SyntaxKind.ExpressionStatement:
                case SyntaxKind.ForEachStatement:
                case SyntaxKind.ForEachVariableStatement:
                case SyntaxKind.ForStatement:
                case SyntaxKind.FromClause:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                case SyntaxKind.IfDirectiveTrivia:
                case SyntaxKind.IfStatement:
                case SyntaxKind.IncompleteMember:
                case SyntaxKind.IndexerDeclaration:
                case SyntaxKind.Interpolation:
                case SyntaxKind.InvocationExpression:
                case SyntaxKind.IsExpression:
                case SyntaxKind.IsPatternExpression:
                case SyntaxKind.LeftShiftAssignmentExpression:
                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.LocalFunctionStatement:
                case SyntaxKind.LockStatement:
                case SyntaxKind.LogicalAndExpression:
                case SyntaxKind.LogicalNotExpression:
                case SyntaxKind.LogicalOrExpression:
                case SyntaxKind.MemberBindingExpression:
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.ModuloAssignmentExpression:
                case SyntaxKind.ModuloExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.NameColon:
                case SyntaxKind.NameEquals:
                case SyntaxKind.NameMemberCref:
                case SyntaxKind.NamespaceDeclaration:
                case SyntaxKind.NotEqualsExpression:
                case SyntaxKind.NullableType:
                case SyntaxKind.ObjectCreationExpression:
                case SyntaxKind.OperatorDeclaration:
                case SyntaxKind.OrAssignmentExpression:
                case SyntaxKind.Parameter:
                case SyntaxKind.ParenthesizedLambdaExpression:
                case SyntaxKind.PointerIndirectionExpression:
                case SyntaxKind.PostDecrementExpression:
                case SyntaxKind.PostIncrementExpression:
                case SyntaxKind.PragmaWarningDirectiveTrivia:
                case SyntaxKind.PreDecrementExpression:
                case SyntaxKind.PreIncrementExpression:
                case SyntaxKind.PropertyDeclaration:
                case SyntaxKind.QualifiedCref:
                case SyntaxKind.QualifiedName:
                case SyntaxKind.RangeExpression:
                case SyntaxKind.RecursivePattern:
                case SyntaxKind.RefType:
                case SyntaxKind.RightShiftAssignmentExpression:
                case SyntaxKind.RightShiftExpression:
                case SyntaxKind.SelectClause:
                case SyntaxKind.SimpleBaseType:
                case SyntaxKind.SimpleLambdaExpression:
                case SyntaxKind.SimpleMemberAccessExpression:
                case SyntaxKind.SubtractExpression:
                case SyntaxKind.SuppressNullableWarningExpression:
                case SyntaxKind.SwitchExpression:
                case SyntaxKind.SwitchStatement:
                case SyntaxKind.ThrowStatement:
                case SyntaxKind.TupleElement:
                case SyntaxKind.TypeArgumentList:
                case SyntaxKind.TypeConstraint:
                case SyntaxKind.TypeOfExpression:
                case SyntaxKind.TypeParameterConstraintClause:
                case SyntaxKind.UnaryMinusExpression:
                case SyntaxKind.UsingDirective:
                case SyntaxKind.UsingStatement:
                case SyntaxKind.VariableDeclaration:
                case SyntaxKind.WhileStatement:
                case SyntaxKind.XmlNameAttribute:
                    {
                        return false;
                    }
                default:
                    {
                        Debug.Fail($"{expression.Kind()} {expression}\n\n{parent.Kind()} {parent}");
                        return false;
                    }
            }
        }
    }
}
