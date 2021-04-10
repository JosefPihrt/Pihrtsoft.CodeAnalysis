// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseCompoundAssignmentAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                {
                    Immutable.InterlockedInitialize(
                        ref _supportedDiagnostics,
                        DiagnosticRules.UseCompoundAssignment,
                        DiagnosticRules.UseCompoundAssignmentFadeOut);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.UseCompoundAssignment.IsEffective(c))
                        AnalyzeSimpleAssignment(c);
                },
                SyntaxKind.SimpleAssignmentExpression);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (((CSharpCompilation)startContext.Compilation).LanguageVersion >= LanguageVersion.CSharp8)
                {
                    startContext.RegisterSyntaxNodeAction(
                        c =>
                        {
                            if (DiagnosticRules.UseCompoundAssignment.IsEffective(c))
                                AnalyzeCoalesceExpression(c);
                        },
                        SyntaxKind.CoalesceExpression);
                }
            });
        }

        private static void AnalyzeSimpleAssignment(SyntaxNodeAnalysisContext context)
        {
            var assignmentExpression = (AssignmentExpressionSyntax)context.Node;

            SimpleAssignmentExpressionInfo assignmentInfo = SyntaxInfo.SimpleAssignmentExpressionInfo(assignmentExpression);

            if (!assignmentInfo.Success)
                return;

            if (assignmentExpression.IsParentKind(
                SyntaxKind.ObjectInitializerExpression,
                SyntaxKind.WithInitializerExpression))
            {
                return;
            }

            ExpressionSyntax right = assignmentInfo.Right;

            if (!CanBeReplacedWithCompoundAssignment(right.Kind()))
                return;

            BinaryExpressionInfo binaryInfo = SyntaxInfo.BinaryExpressionInfo((BinaryExpressionSyntax)right);

            if (!binaryInfo.Success)
                return;

            if (!CSharpFactory.AreEquivalent(assignmentInfo.Left, binaryInfo.Left))
                return;

            var binaryExpression = (BinaryExpressionSyntax)right;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseCompoundAssignment, assignmentExpression, GetCompoundAssignmentOperatorText(binaryExpression));
            DiagnosticHelpers.ReportNode(context, DiagnosticRules.UseCompoundAssignmentFadeOut, binaryExpression.Left);

            bool CanBeReplacedWithCompoundAssignment(SyntaxKind kind)
            {
                switch (kind)
                {
                    case SyntaxKind.AddExpression:
                    case SyntaxKind.SubtractExpression:
                    case SyntaxKind.MultiplyExpression:
                    case SyntaxKind.DivideExpression:
                    case SyntaxKind.ModuloExpression:
                    case SyntaxKind.BitwiseAndExpression:
                    case SyntaxKind.ExclusiveOrExpression:
                    case SyntaxKind.BitwiseOrExpression:
                    case SyntaxKind.LeftShiftExpression:
                    case SyntaxKind.RightShiftExpression:
                        return true;
                    case SyntaxKind.CoalesceExpression:
                        return ((CSharpCompilation)context.Compilation).LanguageVersion >= LanguageVersion.CSharp8;
                    default:
                        return false;
                }
            }
        }

        internal static string GetCompoundAssignmentOperatorText(BinaryExpressionSyntax binaryExpression)
        {
            SyntaxKind compoundAssignmentKind = CSharpFacts.GetCompoundAssignmentKind(binaryExpression.Kind());

            SyntaxKind compoundAssignmentOperatorKind = CSharpFacts.GetCompoundAssignmentOperatorKind(compoundAssignmentKind);

            return SyntaxFacts.GetText(compoundAssignmentOperatorKind);
        }

        private static void AnalyzeCoalesceExpression(SyntaxNodeAnalysisContext context)
        {
            var coalesceExpression = (BinaryExpressionSyntax)context.Node;

            BinaryExpressionInfo binaryExpressionInfo = SyntaxInfo.BinaryExpressionInfo(coalesceExpression, walkDownParentheses: false);

            if (!binaryExpressionInfo.Success)
                return;

            ExpressionSyntax right = binaryExpressionInfo.Right;

            if (!right.IsKind(SyntaxKind.ParenthesizedExpression))
                return;

            var parenthesizedExpression = (ParenthesizedExpressionSyntax)right;

            ExpressionSyntax expression = parenthesizedExpression.Expression;

            if (!expression.IsKind(SyntaxKind.SimpleAssignmentExpression))
                return;

            SimpleAssignmentExpressionInfo assignmentInfo = SyntaxInfo.SimpleAssignmentExpressionInfo((AssignmentExpressionSyntax)expression);

            if (!assignmentInfo.Success)
                return;

            if (!CSharpFactory.AreEquivalent(binaryExpressionInfo.Left, assignmentInfo.Left))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseCompoundAssignment, coalesceExpression);

            DiagnosticHelpers.ReportToken(context, DiagnosticRules.UseCompoundAssignmentFadeOut, parenthesizedExpression.OpenParenToken);
            DiagnosticHelpers.ReportNode(context, DiagnosticRules.UseCompoundAssignmentFadeOut, assignmentInfo.Left);
            DiagnosticHelpers.ReportToken(context, DiagnosticRules.UseCompoundAssignmentFadeOut, parenthesizedExpression.CloseParenToken);
        }
    }
}
