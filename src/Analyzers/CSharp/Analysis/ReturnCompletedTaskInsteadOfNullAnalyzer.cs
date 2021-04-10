// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.SyntaxWalkers;

namespace Roslynator.CSharp.Analysis.ReturnTaskInsteadOfNull
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ReturnCompletedTaskInsteadOfNullAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.ReturnCompletedTaskInsteadOfNull);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeMethodDeclaration(f), SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzePropertyDeclaration(f), SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeIndexerDeclaration(f), SyntaxKind.IndexerDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeLocalFunction(f), SyntaxKind.LocalFunctionStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeLambdaExpression(f), SyntaxKind.SimpleLambdaExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeLambdaExpression(f), SyntaxKind.ParenthesizedLambdaExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeAnonymousMethod(f), SyntaxKind.AnonymousMethodExpression);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (methodDeclaration.Modifiers.Contains(SyntaxKind.AsyncKeyword))
                return;

            ArrowExpressionClauseSyntax expressionBody = methodDeclaration.ExpressionBody;

            if (expressionBody != null)
            {
                ExpressionSyntax expression = expressionBody.Expression?.WalkDownParentheses();

                if (expression?.IsKind(
                    SyntaxKind.NullLiteralExpression,
                    SyntaxKind.DefaultExpression,
                    SyntaxKind.DefaultLiteralExpression,
                    SyntaxKind.ConditionalAccessExpression) != true)
                {
                    return;
                }

                if (!ReturnsTaskOrTaskOfT())
                    return;

                ReportDiagnostic(context, expression);
            }
            else
            {
                BlockSyntax body = methodDeclaration.Body;

                if (body == null)
                    return;

                if (!ReturnsTaskOrTaskOfT())
                    return;

                AnalyzeBlock(context, body);
            }

            bool ReturnsTaskOrTaskOfT()
            {
                return IsTaskOrTaskOfT(context.SemanticModel
                    .GetDeclaredSymbol(methodDeclaration, context.CancellationToken)?
                    .ReturnType);
            }
        }

        private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
        {
            var localFunction = (LocalFunctionStatementSyntax)context.Node;

            if (localFunction.Modifiers.Contains(SyntaxKind.AsyncKeyword))
                return;

            ArrowExpressionClauseSyntax expressionBody = localFunction.ExpressionBody;

            if (expressionBody != null)
            {
                ExpressionSyntax expression = expressionBody.Expression?.WalkDownParentheses();

                if (expression?.IsKind(
                    SyntaxKind.NullLiteralExpression,
                    SyntaxKind.DefaultExpression,
                    SyntaxKind.DefaultLiteralExpression,
                    SyntaxKind.ConditionalAccessExpression) != true)
                {
                    return;
                }

                if (!ReturnsTaskOrTaskOfT())
                    return;

                ReportDiagnostic(context, expression);
            }
            else
            {
                BlockSyntax body = localFunction.Body;

                if (body == null)
                    return;

                if (!ReturnsTaskOrTaskOfT())
                    return;

                AnalyzeBlock(context, body);
            }

            bool ReturnsTaskOrTaskOfT()
            {
                return IsTaskOrTaskOfT(context.SemanticModel
                    .GetDeclaredSymbol(localFunction, context.CancellationToken)?
                    .ReturnType);
            }
        }

        private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

            ArrowExpressionClauseSyntax expressionBody = propertyDeclaration.ExpressionBody;

            if (expressionBody != null)
            {
                ExpressionSyntax expression = expressionBody.Expression?.WalkDownParentheses();

                if (expression?.IsKind(
                    SyntaxKind.NullLiteralExpression,
                    SyntaxKind.DefaultExpression,
                    SyntaxKind.DefaultLiteralExpression,
                    SyntaxKind.ConditionalAccessExpression) != true)
                {
                    return;
                }

                if (!ReturnsTaskOrTaskOfT())
                    return;

                ReportDiagnostic(context, expression);
            }
            else
            {
                AccessorDeclarationSyntax getter = propertyDeclaration.Getter();

                if (getter == null)
                    return;

                if (!ReturnsTaskOrTaskOfT())
                    return;

                AnalyzeGetAccessor(context, getter);
            }

            bool ReturnsTaskOrTaskOfT()
            {
                return IsTaskOrTaskOfT(context.SemanticModel
                    .GetDeclaredSymbol(propertyDeclaration, context.CancellationToken)?
                    .Type);
            }
        }

        private static void AnalyzeIndexerDeclaration(SyntaxNodeAnalysisContext context)
        {
            var indexerDeclaration = (IndexerDeclarationSyntax)context.Node;

            ArrowExpressionClauseSyntax expressionBody = indexerDeclaration.ExpressionBody;

            if (expressionBody != null)
            {
                ExpressionSyntax expression = expressionBody.Expression?.WalkDownParentheses();

                if (expression?.IsKind(
                    SyntaxKind.NullLiteralExpression,
                    SyntaxKind.DefaultExpression,
                    SyntaxKind.DefaultLiteralExpression,
                    SyntaxKind.ConditionalAccessExpression) != true)
                {
                    return;
                }

                if (!ReturnsTaskOrTaskOfT())
                    return;

                ReportDiagnostic(context, expression);
            }
            else
            {
                AccessorDeclarationSyntax getter = indexerDeclaration.Getter();

                if (getter == null)
                    return;

                if (!ReturnsTaskOrTaskOfT())
                    return;

                AnalyzeGetAccessor(context, getter);
            }

            bool ReturnsTaskOrTaskOfT()
            {
                return IsTaskOrTaskOfT(context.SemanticModel
                    .GetDeclaredSymbol(indexerDeclaration, context.CancellationToken)?
                    .Type);
            }
        }

        private static void AnalyzeLambdaExpression(SyntaxNodeAnalysisContext context)
        {
            var lambda = (LambdaExpressionSyntax)context.Node;

            if (lambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword))
                return;

            CSharpSyntaxNode body = lambda.Body;

            if (body is ExpressionSyntax expression)
            {
                expression = expression.WalkDownParentheses();

                if (expression?.IsKind(
                    SyntaxKind.NullLiteralExpression,
                    SyntaxKind.DefaultExpression,
                    SyntaxKind.DefaultLiteralExpression,
                    SyntaxKind.ConditionalAccessExpression) != true)
                {
                    return;
                }

                if (!ReturnsTaskOrTaskOfT())
                    return;

                ReportDiagnostic(context, expression);
            }
            else if (body is BlockSyntax block)
            {
                if (!ReturnsTaskOrTaskOfT())
                    return;

                AnalyzeBlock(context, block);
            }

            bool ReturnsTaskOrTaskOfT()
            {
                return IsTaskOrTaskOfT(context.SemanticModel
                    .GetMethodSymbol(lambda, context.CancellationToken)?
                    .ReturnType);
            }
        }

        private static void AnalyzeAnonymousMethod(SyntaxNodeAnalysisContext context)
        {
            var anonymousMethod = (AnonymousMethodExpressionSyntax)context.Node;

            if (anonymousMethod.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword))
                return;

            if (!ReturnsTaskOrTaskOfT())
                return;

            AnalyzeBlock(context, anonymousMethod.Block);

            bool ReturnsTaskOrTaskOfT()
            {
                return IsTaskOrTaskOfT(context.SemanticModel
                    .GetMethodSymbol(anonymousMethod, context.CancellationToken)?
                    .ReturnType);
            }
        }

        private static void AnalyzeGetAccessor(SyntaxNodeAnalysisContext context, AccessorDeclarationSyntax getter)
        {
            ArrowExpressionClauseSyntax expressionBody = getter.ExpressionBody;

            if (expressionBody != null)
            {
                ExpressionSyntax expression = expressionBody.Expression?.WalkDownParentheses();

                if (expression?.IsKind(
                    SyntaxKind.NullLiteralExpression,
                    SyntaxKind.DefaultExpression,
                    SyntaxKind.DefaultLiteralExpression,
                    SyntaxKind.ConditionalAccessExpression) == true)
                {
                    ReportDiagnostic(context, expression);
                }
            }
            else
            {
                AnalyzeBlock(context, getter.Body);
            }
        }

        private static void AnalyzeBlock(SyntaxNodeAnalysisContext context, BlockSyntax body)
        {
            if (body == null)
                return;

            SyntaxWalker walker = SyntaxWalker.GetInstance();

            walker.VisitBlock(body);

            if (walker.Expressions?.Count > 0)
            {
                foreach (ExpressionSyntax expression in walker.Expressions)
                    ReportDiagnostic(context, expression);
            }

            SyntaxWalker.Free(walker);
        }

        public static bool IsTaskOrTaskOfT(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return false;

            INamedTypeSymbol namedTypeSymbol = SymbolUtility.GetPossiblyAwaitableType(typeSymbol);

            if (namedTypeSymbol == null)
                return false;

            return namedTypeSymbol.HasMetadataName(MetadataNames.System_Threading_Tasks_Task)
                || namedTypeSymbol.OriginalDefinition.EqualsOrInheritsFrom(MetadataNames.System_Threading_Tasks_Task_T);
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
        {
            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.ReturnCompletedTaskInsteadOfNull,
                expression);
        }

        private class SyntaxWalker : StatementWalker
        {
            [ThreadStatic]
            private static SyntaxWalker _cachedInstance;

            public List<ExpressionSyntax> Expressions { get; private set; }

            public override void VisitReturnStatement(ReturnStatementSyntax node)
            {
                ExpressionSyntax expression = node.Expression;

                if (expression?.WalkDownParentheses().IsKind(
                    SyntaxKind.NullLiteralExpression,
                    SyntaxKind.DefaultExpression,
                    SyntaxKind.DefaultLiteralExpression,
                    SyntaxKind.ConditionalAccessExpression) == true)
                {
                    (Expressions ??= new List<ExpressionSyntax>()).Add(expression);
                }
            }

            public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
            {
            }

            public static SyntaxWalker GetInstance()
            {
                SyntaxWalker walker = _cachedInstance;

                if (walker != null)
                {
                    Debug.Assert(walker.Expressions == null || walker.Expressions.Count == 0);

                    _cachedInstance = null;
                    return walker;
                }

                return new SyntaxWalker();
            }

            public static void Free(SyntaxWalker walker)
            {
                walker.Expressions?.Clear();

                _cachedInstance = walker;
            }
        }
    }
}
