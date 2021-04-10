// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnnecessaryNullCheckAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UnnecessaryNullCheck);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeLogicalAndExpression(f), SyntaxKind.LogicalAndExpression);
        }

        private static void AnalyzeLogicalAndExpression(SyntaxNodeAnalysisContext context)
        {
            var logicalAnd = (BinaryExpressionSyntax)context.Node;

            if (logicalAnd.SpanContainsDirectives())
                return;

            BinaryExpressionInfo logicalAndInfo = SyntaxInfo.BinaryExpressionInfo(logicalAnd);

            if (!logicalAndInfo.Success)
                return;

            NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(
                logicalAndInfo.Left,
                context.SemanticModel,
                NullCheckStyles.NotEqualsToNull | NullCheckStyles.HasValue,
                cancellationToken: context.CancellationToken);

            if (!nullCheck.Success)
                return;

            ExpressionSyntax right = logicalAndInfo.Right;

            switch (right.Kind())
            {
                case SyntaxKind.LogicalNotExpression:
                    {
                        var logicalNot = (PrefixUnaryExpressionSyntax)right;

                        Analyze(nullCheck.Expression, logicalNot.Operand?.WalkDownParentheses(), null);
                        break;
                    }
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                    {
                        BinaryExpressionInfo binaryExpressionInfo = SyntaxInfo.BinaryExpressionInfo((BinaryExpressionSyntax)right);

                        if (!binaryExpressionInfo.Success)
                            break;

                        ExpressionSyntax left = binaryExpressionInfo.Left;

                        Analyze(nullCheck.Expression, left, binaryExpressionInfo.Right);
                        break;
                    }
                case SyntaxKind.SimpleMemberAccessExpression:
                    {
                        AnalyzeSimpleMemberAccessExpression(nullCheck.Expression, (MemberAccessExpressionSyntax)right, null);
                        break;
                    }
            }

            void Analyze(ExpressionSyntax expression1, ExpressionSyntax expression2, ExpressionSyntax expression3)
            {
                if (expression2.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    AnalyzeSimpleMemberAccessExpression(expression1, (MemberAccessExpressionSyntax)expression2, expression3);
            }

            void AnalyzeSimpleMemberAccessExpression(ExpressionSyntax expression, MemberAccessExpressionSyntax memberAccessExpression, ExpressionSyntax expression3)
            {
                if (!(memberAccessExpression.Name is IdentifierNameSyntax identifierName)
                    || !string.Equals(identifierName.Identifier.ValueText, "Value", StringComparison.Ordinal))
                {
                    return;
                }

                if (!SyntaxUtility.IsPropertyOfNullableOfT(memberAccessExpression, "Value", context.SemanticModel, context.CancellationToken))
                    return;

                if (!AreEquivalent(expression, memberAccessExpression.Expression))
                    return;

                if (expression3 != null)
                {
                    switch (expression3.Kind())
                    {
                        case SyntaxKind.NumericLiteralExpression:
                        case SyntaxKind.StringLiteralExpression:
                        case SyntaxKind.CharacterLiteralExpression:
                        case SyntaxKind.TrueLiteralExpression:
                        case SyntaxKind.FalseLiteralExpression:
                            {
                                break;
                            }
                        case SyntaxKind.NullLiteralExpression:
                        case SyntaxKind.DefaultLiteralExpression:
                            {
                                return;
                            }
                        default:
                            {
                                if (context.SemanticModel.GetTypeSymbol(expression3, context.CancellationToken).IsNullableType())
                                    return;

                                break;
                            }
                    }
                }

                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UnnecessaryNullCheck, logicalAnd);
            }
        }
    }
}
