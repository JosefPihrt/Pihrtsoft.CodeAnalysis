// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RemoveRedundantDelegateCreationAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.RemoveRedundantDelegateCreation,
                        DiagnosticRules.RemoveRedundantDelegateCreationFadeOut);
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
                    if (DiagnosticRules.RemoveRedundantDelegateCreation.IsEffective(c))
                        AnalyzeAssignmentExpression(c);
                },
                SyntaxKind.AddAssignmentExpression);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.RemoveRedundantDelegateCreation.IsEffective(c))
                        AnalyzeAssignmentExpression(c);
                },
                SyntaxKind.SubtractAssignmentExpression);
        }

        private static void AnalyzeAssignmentExpression(SyntaxNodeAnalysisContext context)
        {
            var assignmentExpression = (AssignmentExpressionSyntax)context.Node;

            AssignmentExpressionInfo info = SyntaxInfo.AssignmentExpressionInfo(assignmentExpression);

            if (!info.Success)
                return;

            ExpressionSyntax right = info.Right;

            if (right.Kind() != SyntaxKind.ObjectCreationExpression)
                return;

            if (right.SpanContainsDirectives())
                return;

            ExpressionSyntax left = info.Left;

            if (!left.IsKind(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression))
                return;

            var objectCreation = (ObjectCreationExpressionSyntax)right;

            ExpressionSyntax expression = objectCreation
                .ArgumentList?
                .Arguments
                .SingleOrDefault(shouldThrow: false)?
                .Expression
                .WalkDownParentheses();

            if (expression == null)
                return;

            if (!expression.IsKind(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression))
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            if (!(semanticModel.GetSymbol(assignmentExpression, cancellationToken) is IMethodSymbol methodSymbol))
                return;

            if (!methodSymbol.MethodKind.Is(MethodKind.EventAdd, MethodKind.EventRemove))
                return;

            if (!(methodSymbol.Parameters.SingleOrDefault(shouldThrow: false)?.Type is INamedTypeSymbol typeSymbol))
                return;

            if (!SymbolUtility.IsEventHandlerMethod(typeSymbol.DelegateInvokeMethod))
                return;

            if (semanticModel.GetSymbol(expression, cancellationToken)?.Kind != SymbolKind.Method)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveRedundantDelegateCreation, right);

            DiagnosticHelpers.ReportToken(context, DiagnosticRules.RemoveRedundantDelegateCreationFadeOut, objectCreation.NewKeyword);
            DiagnosticHelpers.ReportNode(context, DiagnosticRules.RemoveRedundantDelegateCreationFadeOut, objectCreation.Type);
            CSharpDiagnosticHelpers.ReportParentheses(context, DiagnosticRules.RemoveRedundantDelegateCreationFadeOut, objectCreation.ArgumentList);
        }
    }
}
