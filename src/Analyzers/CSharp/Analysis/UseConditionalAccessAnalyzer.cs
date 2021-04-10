// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseConditionalAccessAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UseConditionalAccess);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (((CSharpCompilation)startContext.Compilation).LanguageVersion < LanguageVersion.CSharp6)
                    return;

                startContext.RegisterSyntaxNodeAction(f => AnalyzeIfStatement(f), SyntaxKind.IfStatement);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeBinaryExpression(f), SyntaxKind.LogicalAndExpression);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeBinaryExpression(f), SyntaxKind.LogicalOrExpression);
            });
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            if (!ifStatement.IsSimpleIf())
                return;

            if (ifStatement.ContainsDiagnostics)
                return;

            if (ifStatement.SpanContainsDirectives())
                return;

            NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(ifStatement.Condition, allowedStyles: NullCheckStyles.NotEqualsToNull);

            ExpressionSyntax expression = nullCheck.Expression;

            if (expression == null)
                return;

            SimpleMemberInvocationStatementInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationStatementInfo(ifStatement.SingleNonBlockStatementOrDefault());

            ExpressionSyntax expression2 = invocationInfo.Expression;

            if (expression2 == null)
                return;

            ITypeSymbol typeSymbol = context.SemanticModel.GetTypeSymbol(expression);

            if (typeSymbol == null)
                return;

            if (typeSymbol.IsNullableType())
            {
                if (!expression2.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    return;

                var memberAccess = (MemberAccessExpressionSyntax)expression2;

                if (!(memberAccess.Name is IdentifierNameSyntax identifierName))
                    return;

                if (!string.Equals(identifierName.Identifier.ValueText, "Value", StringComparison.Ordinal))
                    return;

                expression2 = memberAccess.Expression;
            }

            if (!CSharpFactory.AreEquivalent(expression, expression2))
                return;

            if (ifStatement.IsInExpressionTree(context.SemanticModel, context.CancellationToken))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseConditionalAccess, ifStatement);
        }

        private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;

            if (binaryExpression.ContainsDiagnostics)
                return;

            SyntaxKind kind = binaryExpression.Kind();

            if (binaryExpression.WalkUpParentheses().IsParentKind(kind))
                return;

            if (binaryExpression.IsInExpressionTree(context.SemanticModel, context.CancellationToken))
                return;

            (ExpressionSyntax left, ExpressionSyntax right) = GetFixableExpressions(binaryExpression, kind, context.SemanticModel, context.CancellationToken);

            if (left == null)
                return;

            ISymbol operatorSymbol = context.SemanticModel.GetSymbol(binaryExpression, context.CancellationToken);

            if (operatorSymbol?.Name == WellKnownMemberNames.BitwiseOrOperatorName)
            {
                INamedTypeSymbol containingType = operatorSymbol.ContainingType;

                if (containingType.SpecialType != SpecialType.System_Boolean
                    && !ExistsImplicitConversionToBoolean(containingType))
                {
                    return;
                }
            }

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.UseConditionalAccess,
                Location.Create(binaryExpression.SyntaxTree, TextSpan.FromBounds(left.SpanStart, right.Span.End)));

            static bool ExistsImplicitConversionToBoolean(INamedTypeSymbol typeSymbol)
            {
                foreach (ISymbol member in typeSymbol.GetMembers(WellKnownMemberNames.ImplicitConversionName))
                {
                    if (member.Kind == SymbolKind.Method)
                    {
                        var methodSymbol = (IMethodSymbol)member;

                        if (methodSymbol.ReturnType.SpecialType == SpecialType.System_Boolean)
                            return true;
                    }
                }

                return false;
            }
        }

        private static bool IsFixable(
            ExpressionSyntax left,
            ExpressionSyntax right,
            SyntaxKind binaryExpressionKind,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            NullCheckStyles allowedStyles = (binaryExpressionKind == SyntaxKind.LogicalAndExpression)
                ? NullCheckStyles.NotEqualsToNull
                : NullCheckStyles.EqualsToNull;

            NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(left, semanticModel, allowedStyles: allowedStyles, cancellationToken: cancellationToken);

            ExpressionSyntax expression = nullCheck.Expression;

            if (expression == null)
                return false;

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(expression, cancellationToken);

            if (typeSymbol == null)
                return false;

            if (!typeSymbol.IsReferenceTypeOrNullableType())
                return false;

            if (right == null)
                return false;

            if (!ValidateRightExpression(right, binaryExpressionKind, semanticModel, cancellationToken))
                return false;

            if (CSharpUtility.ContainsOutArgumentWithLocalOrParameter(right, semanticModel, cancellationToken))
                return false;

            ExpressionSyntax e = FindExpressionThatCanBeConditionallyAccessed(expression, right, isNullable: !typeSymbol.IsReferenceType, semanticModel, cancellationToken);

            return e != null;
        }

        internal static ExpressionSyntax FindExpressionThatCanBeConditionallyAccessed(
            ExpressionSyntax expressionToFind,
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default)
        {
            return FindExpressionThatCanBeConditionallyAccessed(
                expressionToFind: expressionToFind,
                expression: expression,
                isNullable: false,
                semanticModel: semanticModel,
                cancellationToken: cancellationToken);
        }

        internal static ExpressionSyntax FindExpressionThatCanBeConditionallyAccessed(
            ExpressionSyntax expressionToFind,
            ExpressionSyntax expression,
            bool isNullable,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default)
        {
            if (expression.IsKind(SyntaxKind.LogicalNotExpression))
                expression = ((PrefixUnaryExpressionSyntax)expression).Operand;

            SyntaxKind kind = expressionToFind.Kind();

            SyntaxToken firstToken = expression.GetFirstToken();

            int start = firstToken.SpanStart;

            SyntaxNode node = firstToken.Parent;

            while (node?.SpanStart == start)
            {
                if (kind == node.Kind()
                    && node.IsParentKind(SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.ElementAccessExpression)
                    && semanticModel.GetTypeSymbol(node.Parent, cancellationToken)?.Kind != SymbolKind.PointerType
                    && CSharpFactory.AreEquivalent(expressionToFind, node))
                {
                    if (!isNullable)
                        return (ExpressionSyntax)node;

                    if (node.IsParentKind(SyntaxKind.SimpleMemberAccessExpression)
                        && (((MemberAccessExpressionSyntax)node.Parent).Name is IdentifierNameSyntax identifierName)
                        && string.Equals(identifierName.Identifier.ValueText, "Value", StringComparison.Ordinal)
                        && node.Parent.IsParentKind(SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.ElementAccessExpression))
                    {
                        return (ExpressionSyntax)node.Parent;
                    }

                    return null;
                }

                node = node.Parent;
            }

            return null;
        }

        private static bool ValidateRightExpression(
            ExpressionSyntax expression,
            SyntaxKind binaryExpressionKind,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (binaryExpressionKind == SyntaxKind.LogicalAndExpression)
            {
                switch (expression.Kind())
                {
                    case SyntaxKind.LessThanExpression:
                    case SyntaxKind.GreaterThanExpression:
                    case SyntaxKind.LessThanOrEqualExpression:
                    case SyntaxKind.GreaterThanOrEqualExpression:
                    case SyntaxKind.EqualsExpression:
                        {
                            expression = ((BinaryExpressionSyntax)expression)
                                .Right?
                                .WalkDownParentheses();

                            if (expression == null)
                                return false;

                            Optional<object> optional = semanticModel.GetConstantValue(expression, cancellationToken);

                            return optional.HasValue
                                && optional.Value != null;
                        }
                    case SyntaxKind.NotEqualsExpression:
                        {
                            return ((BinaryExpressionSyntax)expression)
                                .Right?
                                .WalkDownParentheses()
                                .Kind() == SyntaxKind.NullLiteralExpression;
                        }
                    case SyntaxKind.IsPatternExpression:
                        {
                            var isPatternExpression = (IsPatternExpressionSyntax)expression;

                            return !(isPatternExpression.Pattern is ConstantPatternSyntax constantPattern)
                                || !constantPattern.Expression.WalkDownParentheses().IsKind(SyntaxKind.NullLiteralExpression);
                        }
                    case SyntaxKind.SimpleMemberAccessExpression:
                    case SyntaxKind.InvocationExpression:
                    case SyntaxKind.ElementAccessExpression:
                    case SyntaxKind.LogicalNotExpression:
                    case SyntaxKind.IsExpression:
                    case SyntaxKind.AsExpression:
                        {
                            return true;
                        }
                    default:
                        {
                            return false;
                        }
                }
            }
            else if (binaryExpressionKind == SyntaxKind.LogicalOrExpression)
            {
                switch (expression.Kind())
                {
                    case SyntaxKind.SimpleMemberAccessExpression:
                    case SyntaxKind.InvocationExpression:
                    case SyntaxKind.ElementAccessExpression:
                    case SyntaxKind.LogicalNotExpression:
                        return true;
                    default:
                        return false;
                }
            }

            Debug.Fail(binaryExpressionKind.ToString());

            return false;
        }

        internal static (ExpressionSyntax left, ExpressionSyntax right) GetFixableExpressions(
            BinaryExpressionSyntax binaryExpression,
            SyntaxKind kind,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax e = binaryExpression;

            ExpressionSyntax left;
            ExpressionSyntax right = null;

            while (true)
            {
                ExpressionSyntax last = GetLastChild(e);

                if (last != null)
                {
                    e = last;
                }
                else
                {
                    while (e != binaryExpression
                        && IsFirstChild(e))
                    {
                        e = (ExpressionSyntax)e.Parent;
                    }

                    if (e == binaryExpression)
                        break;

                    e = GetPreviousSibling(e);
                }

                if (!e.IsKind(kind, SyntaxKind.ParenthesizedExpression))
                {
                    if (right == null)
                    {
                        right = e;
                    }
                    else
                    {
                        left = e;

                        if (!binaryExpression.ContainsDirectives(TextSpan.FromBounds(left.SpanStart, right.Span.End))
                            && IsFixable(left, right, kind, semanticModel, cancellationToken))
                        {
                            return (left, right);
                        }

                        right = left;
                    }
                }
            }

            return default;

            ExpressionSyntax GetLastChild(SyntaxNode node)
            {
                SyntaxKind kind2 = node.Kind();

                if (kind2 == kind)
                    return ((BinaryExpressionSyntax)node).Right;

                if (kind2 == SyntaxKind.ParenthesizedExpression)
                    return ((ParenthesizedExpressionSyntax)node).Expression;

                return null;
            }

            ExpressionSyntax GetPreviousSibling(SyntaxNode node)
            {
                SyntaxNode parent = node.Parent;

                if (parent.IsKind(kind))
                {
                    var logicalAnd = (BinaryExpressionSyntax)parent;

                    if (logicalAnd.Right == node)
                        return logicalAnd.Left;
                }

                return null;
            }

            bool IsFirstChild(SyntaxNode node)
            {
                SyntaxNode parent = node.Parent;

                if (parent.IsKind(kind))
                    return ((BinaryExpressionSyntax)parent).Left == node;

                return true;
            }
        }
    }
}
