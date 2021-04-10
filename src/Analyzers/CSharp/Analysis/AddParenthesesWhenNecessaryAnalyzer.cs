// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AddParenthesesWhenNecessaryAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AddParenthesesWhenNecessary);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                f => AnalyzeBinaryExpression(f),
                SyntaxKind.MultiplyExpression,
                SyntaxKind.DivideExpression,
                SyntaxKind.ModuloExpression,
                SyntaxKind.AddExpression,
                SyntaxKind.SubtractExpression,
                SyntaxKind.LeftShiftExpression,
                SyntaxKind.RightShiftExpression,
                SyntaxKind.LessThanExpression,
                SyntaxKind.GreaterThanExpression,
                SyntaxKind.LessThanOrEqualExpression,
                SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression,
                SyntaxKind.BitwiseAndExpression,
                SyntaxKind.ExclusiveOrExpression,
                SyntaxKind.BitwiseOrExpression,
                SyntaxKind.LogicalAndExpression,
                SyntaxKind.LogicalOrExpression);
        }

        private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;

            if (binaryExpression.ContainsDiagnostics)
                return;

            SyntaxKind binaryExpressionKind = binaryExpression.Kind();

            ExpressionSyntax left = binaryExpression.Left;

            if (left?.IsMissing == false)
                Analyze(context, left, binaryExpressionKind);

            ExpressionSyntax right = binaryExpression.Right;

            if (right?.IsMissing == false)
                Analyze(context, right, binaryExpressionKind);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, SyntaxKind binaryExpressionKind)
        {
            if (expression.ContainsUnbalancedIfElseDirectives())
                return;

            if (!IsFixable(expression, binaryExpressionKind))
                return;

            if (IsNestedDiagnostic(expression))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AddParenthesesWhenNecessary, expression);
        }

        private static bool IsNestedDiagnostic(SyntaxNode node)
        {
            for (SyntaxNode current = node.Parent; current != null; current = current.Parent)
            {
                if (IsFixable(current))
                    return true;
            }

            return false;
        }

        internal static bool IsFixable(SyntaxNode node)
        {
            SyntaxNode parent = node.Parent;

            return parent != null
                && IsFixable(node, parent.Kind());
        }

        private static bool IsFixable(SyntaxNode node, SyntaxKind binaryExpressionKind)
        {
            int groupNumber = GetGroupNumber(binaryExpressionKind);

            return groupNumber != 0
                && groupNumber == GetGroupNumber(node)
                && CSharpFacts.GetOperatorPrecedence(node.Kind()) < CSharpFacts.GetOperatorPrecedence(binaryExpressionKind);
        }

        private static int GetGroupNumber(SyntaxNode node)
        {
            return GetGroupNumber(node.Kind());
        }

        private static int GetGroupNumber(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.ModuloExpression:
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubtractExpression:
                    return 1;
                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.RightShiftExpression:
                    return 2;
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                    return 3;
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                    return 4;
                case SyntaxKind.BitwiseAndExpression:
                case SyntaxKind.ExclusiveOrExpression:
                case SyntaxKind.BitwiseOrExpression:
                case SyntaxKind.LogicalAndExpression:
                case SyntaxKind.LogicalOrExpression:
                    return 5;
                default:
                    return 0;
            }
        }
    }
}
