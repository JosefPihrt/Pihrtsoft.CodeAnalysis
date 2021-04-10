// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis.UsePatternMatching
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UsePatternMatchingInsteadOfIsAndCastAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UsePatternMatchingInsteadOfIsAndCast);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (((CSharpCompilation)startContext.Compilation).LanguageVersion < LanguageVersion.CSharp7)
                    return;

                startContext.RegisterSyntaxNodeAction(f => AnalyzeIsExpression(f), SyntaxKind.IsExpression);
            });
        }

        private static void AnalyzeIsExpression(SyntaxNodeAnalysisContext context)
        {
            var isExpression = (BinaryExpressionSyntax)context.Node;

            IsExpressionInfo isExpressionInfo = SyntaxInfo.IsExpressionInfo(isExpression);

            if (!isExpressionInfo.Success)
                return;

            ExpressionSyntax expression = isExpressionInfo.Expression;

            var identifierName = expression as IdentifierNameSyntax;

            if (identifierName == null)
            {
                if (expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                {
                    var memberAccess = (MemberAccessExpressionSyntax)expression;

                    if (memberAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                        identifierName = memberAccess.Name as IdentifierNameSyntax;
                }

                if (identifierName == null)
                    return;
            }

            ExpressionSyntax left = isExpression.WalkUpParentheses();

            SyntaxNode node = left.Parent;

            if (node.ContainsDiagnostics)
                return;

            switch (node.Kind())
            {
                case SyntaxKind.LogicalAndExpression:
                    {
                        var logicalAnd = (BinaryExpressionSyntax)node;

                        if (left != logicalAnd.Left)
                            return;

                        ExpressionSyntax right = logicalAnd.Right;

                        if (right == null)
                            return;

                        SemanticModel semanticModel = context.SemanticModel;
                        CancellationToken cancellationToken = context.CancellationToken;

                        if (semanticModel.GetTypeSymbol(isExpressionInfo.Type, cancellationToken).IsNullableType())
                            return;

                        if (logicalAnd.Parent.IsInExpressionTree(semanticModel, cancellationToken))
                            return;

                        if (!IsFixable(right, identifierName, semanticModel, cancellationToken))
                            return;

                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UsePatternMatchingInsteadOfIsAndCast, logicalAnd);
                        break;
                    }
                case SyntaxKind.IfStatement:
                    {
                        var ifStatement = (IfStatementSyntax)node;

                        if (left != ifStatement.Condition)
                            return;

                        StatementSyntax statement = ifStatement.Statement;

                        if (statement == null)
                            return;

                        SemanticModel semanticModel = context.SemanticModel;
                        CancellationToken cancellationToken = context.CancellationToken;

                        if (semanticModel.GetTypeSymbol(isExpressionInfo.Type, cancellationToken).IsNullableType())
                            return;

                        if (!IsFixable(statement, identifierName, semanticModel, cancellationToken))
                            return;

                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UsePatternMatchingInsteadOfIsAndCast, ifStatement.Condition);
                        break;
                    }
            }
        }

        private static bool IsFixable(
            SyntaxNode node,
            IdentifierNameSyntax identifierName,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            UsePatternMatchingWalker walker = UsePatternMatchingWalker.GetInstance();

            walker.SetValues(identifierName, semanticModel, cancellationToken);

            walker.Visit(node);

            bool isFixable = walker.IsFixable.GetValueOrDefault();

            UsePatternMatchingWalker.Free(walker);

            return isFixable;
        }
    }
}
