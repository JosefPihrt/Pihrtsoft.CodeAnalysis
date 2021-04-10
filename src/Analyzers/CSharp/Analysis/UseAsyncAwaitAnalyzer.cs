// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.SyntaxWalkers;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseAsyncAwaitAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UseAsyncAwait);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeMethodDeclaration(f), SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeLocalFunctionStatement(f), SyntaxKind.LocalFunctionStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeSimpleLambdaExpression(f), SyntaxKind.SimpleLambdaExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeParenthesizedLambdaExpression(f), SyntaxKind.ParenthesizedLambdaExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeAnonymousMethodExpression(f), SyntaxKind.AnonymousMethodExpression);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (methodDeclaration.Modifiers.Contains(SyntaxKind.AsyncKeyword))
                return;

            BlockSyntax body = methodDeclaration.Body;

            if (body == null)
                return;

            if (!body.Statements.Any())
                return;

            if (!methodDeclaration.Identifier.ValueText.EndsWith("Async", StringComparison.Ordinal))
            {
                IMethodSymbol methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

                if (!IsTaskLike(methodSymbol.ReturnType))
                    return;
            }

            if (IsFixable(body))
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseAsyncAwait, methodDeclaration.Identifier);
        }

        private static void AnalyzeLocalFunctionStatement(SyntaxNodeAnalysisContext context)
        {
            var localFunction = (LocalFunctionStatementSyntax)context.Node;

            if (localFunction.Modifiers.Contains(SyntaxKind.AsyncKeyword))
                return;

            BlockSyntax body = localFunction.Body;

            if (body == null)
                return;

            if (!body.Statements.Any())
                return;

            if (!localFunction.Identifier.ValueText.EndsWith("Async", StringComparison.Ordinal))
            {
                IMethodSymbol methodSymbol = context.SemanticModel.GetDeclaredSymbol(localFunction, context.CancellationToken);

                if (!IsTaskLike(methodSymbol.ReturnType))
                    return;
            }

            if (IsFixable(body))
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseAsyncAwait, localFunction.Identifier);
        }

        private static void AnalyzeSimpleLambdaExpression(SyntaxNodeAnalysisContext context)
        {
            var simpleLambda = (SimpleLambdaExpressionSyntax)context.Node;

            if (simpleLambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword))
                return;

            if (!(simpleLambda.Body is BlockSyntax body))
                return;

            if (!(context.SemanticModel.GetSymbol(simpleLambda, context.CancellationToken) is IMethodSymbol methodSymbol))
                return;

            if (!IsTaskLike(methodSymbol.ReturnType))
                return;

            if (IsFixable(body))
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseAsyncAwait, simpleLambda);
        }

        private static void AnalyzeParenthesizedLambdaExpression(SyntaxNodeAnalysisContext context)
        {
            var parenthesizedLambda = (ParenthesizedLambdaExpressionSyntax)context.Node;

            if (parenthesizedLambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword))
                return;

            if (!(parenthesizedLambda.Body is BlockSyntax body))
                return;

            if (!(context.SemanticModel.GetSymbol(parenthesizedLambda, context.CancellationToken) is IMethodSymbol methodSymbol))
                return;

            if (!IsTaskLike(methodSymbol.ReturnType))
                return;

            if (IsFixable(body))
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseAsyncAwait, parenthesizedLambda);
        }

        private static void AnalyzeAnonymousMethodExpression(SyntaxNodeAnalysisContext context)
        {
            var anonymousMethod = (AnonymousMethodExpressionSyntax)context.Node;

            if (anonymousMethod.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword))
                return;

            BlockSyntax body = anonymousMethod.Block;

            if (body == null)
                return;

            if (!(context.SemanticModel.GetSymbol(anonymousMethod, context.CancellationToken) is IMethodSymbol methodSymbol))
                return;

            if (!IsTaskLike(methodSymbol.ReturnType))
                return;

            if (IsFixable(body))
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseAsyncAwait, anonymousMethod);
        }

        private static bool IsFixable(BlockSyntax body)
        {
            UseAsyncAwaitWalker walker = UseAsyncAwaitWalker.GetInstance();

            walker.VisitBlock(body);

            ReturnStatementSyntax returnStatement = walker.ReturnStatement;

            UseAsyncAwaitWalker.Free(walker);

            return returnStatement != null;
        }

        private static bool IsTaskLike(ITypeSymbol typeSymbol)
        {
            if (typeSymbol?.IsErrorType() == false
                && typeSymbol.SpecialType == SpecialType.None)
            {
                ITypeSymbol t = typeSymbol.OriginalDefinition;

                if (t.Name == "ValueTask`1"
                    && t.ContainingNamespace.HasMetadataName(MetadataNames.System_Threading_Tasks))
                {
                    return true;
                }

                do
                {
                    if ((t.Name == "Task" || t.Name == "Task`1")
                        && t.ContainingNamespace.HasMetadataName(MetadataNames.System_Threading_Tasks))
                    {
                        return true;
                    }

                    t = t.BaseType;

                } while (t?.SpecialType == SpecialType.None);
            }

            return false;
        }

        private class UseAsyncAwaitWalker : StatementWalker
        {
            [ThreadStatic]
            private static UseAsyncAwaitWalker _cachedInstance;

            private int _usingOrTryStatementDepth;
            private bool _shouldVisit = true;
            private readonly List<int> _usingDeclarations = new List<int>();

            public override bool ShouldVisit => _shouldVisit;

            public ReturnStatementSyntax ReturnStatement { get; private set; }

            private void Reset()
            {
                _shouldVisit = true;
                _usingDeclarations.Clear();
                ReturnStatement = null;
            }

            public override void VisitUsingStatement(UsingStatementSyntax node)
            {
                _usingOrTryStatementDepth++;
                base.VisitUsingStatement(node);
                _usingOrTryStatementDepth--;
            }

            public override void VisitTryStatement(TryStatementSyntax node)
            {
                BlockSyntax block = node.Block;

                if (block != null)
                {
                    _usingOrTryStatementDepth++;
                    VisitBlock(block);
                    _usingOrTryStatementDepth--;
                }

                foreach (CatchClauseSyntax catchClause in node.Catches)
                    VisitCatchClause(catchClause);

                FinallyClauseSyntax finallyClause = node.Finally;

                if (finallyClause != null)
                    VisitFinallyClause(finallyClause);
            }

            public override void VisitBlock(BlockSyntax node)
            {
                _usingDeclarations.Add(0);
                base.VisitBlock(node);
                _usingDeclarations.RemoveAt(_usingDeclarations.Count - 1);
            }

            public override void VisitSwitchSection(SwitchSectionSyntax node)
            {
                _usingDeclarations.Add(0);
                base.VisitSwitchSection(node);
                _usingDeclarations.RemoveAt(_usingDeclarations.Count - 1);
            }

            public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
            {
                if (node.UsingKeyword.IsKind(SyntaxKind.UsingKeyword))
                    _usingDeclarations[_usingDeclarations.Count - 1]++;

                base.VisitLocalDeclarationStatement(node);
            }

            public override void VisitReturnStatement(ReturnStatementSyntax node)
            {
                bool isInsideUsingOrTry = _usingOrTryStatementDepth > 0;

                if (!isInsideUsingOrTry)
                {
                    foreach (int count in _usingDeclarations)
                    {
                        if (count > 0)
                        {
                            isInsideUsingOrTry = true;
                            break;
                        }
                    }
                }

                if (isInsideUsingOrTry)
                {
                    ExpressionSyntax expression = node.Expression;

                    if (expression?.IsKind(SyntaxKind.AwaitExpression) == false)
                    {
                        ReturnStatement = node;
                    }

                    _shouldVisit = false;
                }

                base.VisitReturnStatement(node);
            }

            public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
            {
            }

            public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
            {
            }

            public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
            {
            }

            public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
            {
            }

            public static UseAsyncAwaitWalker GetInstance()
            {
                UseAsyncAwaitWalker walker = _cachedInstance;

                if (walker != null)
                {
                    Debug.Assert(walker.ReturnStatement == null);

                    _cachedInstance = null;
                    return walker;
                }

                return new UseAsyncAwaitWalker();
            }

            public static void Free(UseAsyncAwaitWalker walker)
            {
                walker.Reset();
                _cachedInstance = walker;
            }
        }
    }
}
