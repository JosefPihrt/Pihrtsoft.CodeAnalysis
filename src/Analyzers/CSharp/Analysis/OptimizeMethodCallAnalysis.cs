// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    internal static class OptimizeMethodCallAnalysis
    {
        public static void OptimizeStringCompare(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            SeparatedSyntaxList<ArgumentSyntax> arguments = invocationInfo.Arguments;

            if (arguments.Count != 3)
                return;

            ISymbol symbol = context.SemanticModel.GetSymbol(invocationExpression, context.CancellationToken);

            if (symbol?.Kind != SymbolKind.Method)
                return;

            if (symbol.ContainingType?.SpecialType != SpecialType.System_String)
                return;

            var methodSymbol = (IMethodSymbol)symbol;

            if (!SymbolUtility.IsPublicStaticNonGeneric(methodSymbol, "Compare"))
                return;

            ImmutableArray<IParameterSymbol> parameters = methodSymbol.Parameters;

            if (parameters.Length != 3)
                return;

            if (parameters[0].Type.SpecialType != SpecialType.System_String)
                return;

            if (parameters[1].Type.SpecialType != SpecialType.System_String)
                return;

            if (!parameters[2].Type.HasMetadataName(MetadataNames.System_StringComparison))
                return;

            SyntaxNode node = invocationExpression.WalkUpParentheses();

            if (node.IsParentKind(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression))
            {
                var equalsExpression = (BinaryExpressionSyntax)node.Parent;

                ExpressionSyntax other = (equalsExpression.Left == node)
                    ? equalsExpression.Right
                    : equalsExpression.Left;

                if (other.WalkDownParentheses().IsNumericLiteralExpression("0"))
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.OptimizeMethodCall, equalsExpression, "string.Compare");
                    return;
                }
            }

            Optional<object> optional = context.SemanticModel.GetConstantValue(invocationInfo.Arguments[2].Expression, context.CancellationToken);

            if (optional.HasValue
                && optional.Value is int value
                && value == (int)StringComparison.Ordinal)
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.OptimizeMethodCall, invocationExpression, "string.Compare");
            }
        }

        public static void OptimizeDebugAssert(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            if (invocationExpression.SpanContainsDirectives())
                return;

            SeparatedSyntaxList<ArgumentSyntax> arguments = invocationInfo.Arguments;

            if (arguments.Count < 1 || arguments.Count > 3)
                return;

            if (arguments[0].Expression?.Kind() != SyntaxKind.FalseLiteralExpression)
                return;

            IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(invocationExpression, context.CancellationToken);

            if (!SymbolUtility.IsPublicStaticNonGeneric(methodSymbol, "Assert"))
                return;

            if (methodSymbol.ContainingType?.HasMetadataName(MetadataNames.System_Diagnostics_Debug) != true)
                return;

            if (!methodSymbol.ReturnsVoid)
                return;

            ImmutableArray<IParameterSymbol> assertParameters = methodSymbol.Parameters;

            if (assertParameters[0].Type.SpecialType != SpecialType.System_Boolean)
                return;

            int length = assertParameters.Length;

            for (int i = 1; i < length; i++)
            {
                if (assertParameters[i].Type.SpecialType != SpecialType.System_String)
                    return;
            }

            if (!ContainsFailMethod())
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.OptimizeMethodCall, invocationExpression, "Debug.Assert");

            bool ContainsFailMethod()
            {
                foreach (ISymbol symbol in methodSymbol.ContainingType.GetMembers("Fail"))
                {
                    if (symbol is IMethodSymbol failMethodSymbol
                        && SymbolUtility.IsPublicStaticNonGeneric(failMethodSymbol)
                        && failMethodSymbol.ReturnsVoid)
                    {
                        ImmutableArray<IParameterSymbol> failParameters = failMethodSymbol.Parameters;

                        if (failParameters.Length == ((length == 1) ? 1 : length - 1)
                            && failParameters.All(f => f.Type.SpecialType == SpecialType.System_String))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public static void OptimizeStringJoin(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            ArgumentSyntax firstArgument = invocationInfo.Arguments.FirstOrDefault();

            if (firstArgument == null)
                return;

            if (invocationInfo.MemberAccessExpression.SpanOrTrailingTriviaContainsDirectives()
                || invocationInfo.ArgumentList.OpenParenToken.ContainsDirectives
                || firstArgument.ContainsDirectives)
            {
                return;
            }

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            IMethodSymbol methodSymbol = semanticModel.GetMethodSymbol(invocationExpression, cancellationToken);

            if (!SymbolUtility.IsPublicStaticNonGeneric(methodSymbol, "Join"))
                return;

            if (methodSymbol.ContainingType?.SpecialType != SpecialType.System_String)
                return;

            if (!methodSymbol.IsReturnType(SpecialType.System_String))
                return;

            ImmutableArray<IParameterSymbol> parameters = methodSymbol.Parameters;

            if (parameters.Length != 2)
                return;

            if (parameters[0].Type.SpecialType != SpecialType.System_String)
                return;

            if (!parameters[1].IsParameterArrayOf(SpecialType.System_String, SpecialType.System_Object)
                && !parameters[1].Type.OriginalDefinition.IsIEnumerableOfT())
            {
                return;
            }

            if (firstArgument.Expression == null)
                return;

            if (!CSharpUtility.IsEmptyStringExpression(firstArgument.Expression, semanticModel, cancellationToken))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.OptimizeMethodCall, invocationExpression, "string.Join");
        }

        public static void OptimizeDictionaryContainsKey(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            var isNegation = false;

            IfStatementSyntax ifStatement = GetIfStatement();

            ConditionalStatementInfo conditionalInfo = SyntaxInfo.ConditionalStatementInfo(ifStatement);

            if (!conditionalInfo.Success)
                return;

            SimpleAssignmentStatementInfo simpleAssignmentStatement = SyntaxInfo.SimpleAssignmentStatementInfo((isNegation) ? conditionalInfo.WhenFalse : conditionalInfo.WhenTrue);

            if (!simpleAssignmentStatement.Success)
                return;

            if (!(simpleAssignmentStatement.Left is ElementAccessExpressionSyntax elementAccessExpression))
                return;

            if (!CSharpFactory.AreEquivalent(invocationInfo.Expression, elementAccessExpression.Expression))
                return;

            ExpressionSyntax argumentExpression = elementAccessExpression.ArgumentList.Arguments.SingleOrDefault(shouldThrow: false)?.Expression;

            ExpressionSyntax keyExpression = invocationInfo.Arguments[0].Expression;

            if (!CSharpFactory.AreEquivalent(keyExpression, argumentExpression))
                return;

            SimpleMemberInvocationStatementInfo invocationInfo2 = SyntaxInfo.SimpleMemberInvocationStatementInfo((isNegation) ? conditionalInfo.WhenTrue : conditionalInfo.WhenFalse);

            if (!invocationInfo2.Success)
                return;

            if (invocationInfo2.Arguments.Count != 2)
                return;

            if (invocationInfo2.NameText != "Add")
                return;

            if (!CSharpFactory.AreEquivalent(invocationInfo.Expression, invocationInfo2.Expression))
                return;

            if (!CSharpFactory.AreEquivalent(keyExpression, invocationInfo2.Arguments[0].Expression))
                return;

            if (!CSharpFactory.AreEquivalent(simpleAssignmentStatement.Right, invocationInfo2.Arguments[1].Expression))
                return;

            IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(invocationExpression, context.CancellationToken);

            if (!IsDictionaryContainsKey(methodSymbol))
                return;

            IMethodSymbol methodSymbol2 = context.SemanticModel.GetMethodSymbol(invocationInfo2.InvocationExpression, context.CancellationToken);

            if (!IsDictionaryAdd(methodSymbol2))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.OptimizeMethodCall, ifStatement, "Dictionary<TKey, TValue>.ContainsKey");

            IfStatementSyntax GetIfStatement()
            {
                SyntaxNode parent = invocationExpression.Parent;

                switch (parent.Kind())
                {
                    case SyntaxKind.LogicalNotExpression:
                        {
                            var logicalNot = (PrefixUnaryExpressionSyntax)parent;

                            isNegation = true;

                            if (logicalNot.IsParentKind(SyntaxKind.IfStatement))
                            {
                                var ifStatement2 = (IfStatementSyntax)logicalNot.Parent;

                                if (ifStatement2.Condition == logicalNot)
                                    return ifStatement2;
                            }

                            break;
                        }
                    case SyntaxKind.IfStatement:
                        {
                            var ifStatement2 = (IfStatementSyntax)parent;

                            if (ifStatement2.Condition == invocationExpression)
                                return ifStatement2;

                            break;
                        }
                }

                return null;
            }

            static bool IsDictionaryContainsKey(IMethodSymbol symbol)
            {
                return symbol?.DeclaredAccessibility == Accessibility.Public
                    && !symbol.IsStatic
                    && symbol.ReturnType.SpecialType == SpecialType.System_Boolean
                    && symbol.Parameters.Length == 1
                    && symbol.Name == "ContainsKey"
                    && symbol.ContainingType.OriginalDefinition.HasMetadataName(MetadataNames.System_Collections_Generic_Dictionary_T2);
            }

            static bool IsDictionaryAdd(IMethodSymbol symbol)
            {
                return symbol?.DeclaredAccessibility == Accessibility.Public
                    && !symbol.IsStatic
                    && symbol.ReturnType.SpecialType == SpecialType.System_Void
                    && symbol.Parameters.Length == 2
                    && symbol.Name == "Add"
                    && symbol.ContainingType.OriginalDefinition.HasMetadataName(MetadataNames.System_Collections_Generic_Dictionary_T2);
            }
        }
    }
}
