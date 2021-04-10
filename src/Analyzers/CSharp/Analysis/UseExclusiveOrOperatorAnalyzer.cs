// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseExclusiveOrOperatorAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UseExclusiveOrOperator);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeLogicalOrExpression(f), SyntaxKind.LogicalOrExpression);
        }

        private static void AnalyzeLogicalOrExpression(SyntaxNodeAnalysisContext context)
        {
            SyntaxNode node = context.Node;

            if (node.ContainsDiagnostics)
                return;

            if (node.SpanContainsDirectives())
                return;

            BinaryExpressionInfo info = SyntaxInfo.BinaryExpressionInfo((BinaryExpressionSyntax)context.Node);

            if (!info.Success)
                return;

            if (!info.Left.IsKind(SyntaxKind.LogicalAndExpression))
                return;

            if (!info.Right.IsKind(SyntaxKind.LogicalAndExpression))
                return;

            ExpressionPair expressions = GetExpressionPair((BinaryExpressionSyntax)info.Left);

            if (!expressions.IsValid)
                return;

            ExpressionPair expressions2 = GetExpressionPair((BinaryExpressionSyntax)info.Right);

            if (!expressions2.IsValid)
                return;

            if (expressions.Expression.Kind() != expressions2.InvertedExpression.Kind())
                return;

            if (expressions.InvertedExpression.Kind() != expressions2.Expression.Kind())
                return;

            if (!AreEquivalent(expressions.Expression, expressions2.InvertedExpression))
                return;

            if (!AreEquivalent(expressions.InvertedExpression, expressions2.Expression))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseExclusiveOrOperator, context.Node);
        }

        private static ExpressionPair GetExpressionPair(BinaryExpressionSyntax logicalAnd)
        {
            BinaryExpressionInfo info = SyntaxInfo.BinaryExpressionInfo(logicalAnd);

            if (info.Success)
            {
                ExpressionSyntax left = info.Left;
                ExpressionSyntax right = info.Right;

                if (left.Kind() == SyntaxKind.LogicalNotExpression)
                {
                    if (right.Kind() != SyntaxKind.LogicalNotExpression)
                    {
                        var logicalNot = (PrefixUnaryExpressionSyntax)left;

                        return new ExpressionPair(right, logicalNot.Operand.WalkDownParentheses());
                    }
                }
                else if (right.Kind() == SyntaxKind.LogicalNotExpression)
                {
                    var logicalNot = (PrefixUnaryExpressionSyntax)right;

                    return new ExpressionPair(left, logicalNot.Operand.WalkDownParentheses());
                }
            }

            return default;
        }

        private readonly struct ExpressionPair
        {
            public ExpressionPair(ExpressionSyntax expression, ExpressionSyntax invertedExpression)
            {
                Expression = expression;
                InvertedExpression = invertedExpression;
            }

            public bool IsValid
            {
                get { return Expression != null && InvertedExpression != null; }
            }

            public ExpressionSyntax Expression { get; }
            public ExpressionSyntax InvertedExpression { get; }
        }
    }
}
