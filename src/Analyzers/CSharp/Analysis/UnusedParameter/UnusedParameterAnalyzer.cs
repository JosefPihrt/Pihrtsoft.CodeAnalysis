// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;
using Roslynator.CSharp.SyntaxWalkers;

namespace Roslynator.CSharp.Analysis.UnusedParameter
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnusedParameterAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.UnusedParameter,
                        DiagnosticRules.UnusedThisParameter,
                        DiagnosticRules.UnusedTypeParameter);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeConstructorDeclaration(f), SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeOperatorDeclaration(f), SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeConversionOperatorDeclaration(f), SyntaxKind.ConversionOperatorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeIndexerDeclaration(f), SyntaxKind.IndexerDeclaration);

            context.RegisterSyntaxNodeAction(f => AnalyzeMethodDeclaration(f), SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeLocalFunctionStatement(f), SyntaxKind.LocalFunctionStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeSimpleLambdaExpression(f), SyntaxKind.SimpleLambdaExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeParenthesizedLambdaExpression(f), SyntaxKind.ParenthesizedLambdaExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeAnonymousMethodExpression(f), SyntaxKind.AnonymousMethodExpression);
        }

        private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;

            if (constructorDeclaration.ContainsDiagnostics)
                return;

            ParameterInfo parameterInfo = SyntaxInfo.ParameterInfo(constructorDeclaration);

            if (!parameterInfo.Success)
                return;

            if (ContainsOnlyThrowNewExpression(parameterInfo.Body))
                return;

            // Skip a constructor that is required by ISerializable interface
            if (parameterInfo.Parameters.Count == 2)
            {
                IMethodSymbol symbol = context.SemanticModel.GetDeclaredSymbol(constructorDeclaration, context.CancellationToken);

                if (symbol != null)
                {
                    ImmutableArray<IParameterSymbol> parameters = symbol.Parameters;

                    if (parameters.Length == 2
                        && parameters[0].Type.HasMetadataName(MetadataNames.System_Runtime_Serialization_SerializationInfo)
                        && parameters[1].Type.HasMetadataName(MetadataNames.System_Runtime_Serialization_StreamingContext))
                    {
                        return;
                    }
                }
            }

            Analyze(context, parameterInfo);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (methodDeclaration.ContainsDiagnostics)
                return;

            if (!methodDeclaration.IsParentKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration))
                return;

            if (methodDeclaration.Modifiers.ContainsAny(
                SyntaxKind.AbstractKeyword,
                SyntaxKind.VirtualKeyword,
                SyntaxKind.OverrideKeyword,
                SyntaxKind.PartialKeyword))
            {
                return;
            }

            ParameterInfo parameterInfo = SyntaxInfo.ParameterInfo(methodDeclaration);

            if (!parameterInfo.Success)
                return;

            if (ContainsOnlyThrowNewExpression(parameterInfo.Body))
                return;

            IMethodSymbol methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

            if (methodSymbol == null)
                return;

            if (SymbolUtility.IsEventHandlerMethod(methodSymbol))
                return;

            if (!methodSymbol.ExplicitInterfaceImplementations.IsDefaultOrEmpty)
                return;

            if (methodSymbol.ImplementsInterfaceMember(allInterfaces: true))
                return;

            UnusedParameterWalker walker = UnusedParameterWalker.GetInstance();
            walker.SetValues(context.SemanticModel, context.CancellationToken);

            FindUnusedNodes(parameterInfo, walker);

            if (walker.Nodes.Count > 0
                && !MethodReferencedAsMethodGroupWalker.IsReferencedAsMethodGroup(methodDeclaration, methodSymbol, context.SemanticModel, context.CancellationToken))
            {
                foreach (KeyValuePair<string, NodeSymbolInfo> kvp in walker.Nodes)
                    ReportDiagnostic(context, kvp.Value.Node);
            }

            UnusedParameterWalker.Free(walker);
        }

        private static void AnalyzeOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var operatorDeclaration = (OperatorDeclarationSyntax)context.Node;

            if (operatorDeclaration.ContainsDiagnostics)
                return;

            ParameterInfo parameterInfo = SyntaxInfo.ParameterInfo(operatorDeclaration);

            if (!parameterInfo.Success)
                return;

            if (ContainsOnlyThrowNewExpression(parameterInfo.Body))
                return;

            Analyze(context, parameterInfo);
        }

        private static void AnalyzeConversionOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var conversionOperatorDeclaration = (ConversionOperatorDeclarationSyntax)context.Node;

            if (conversionOperatorDeclaration.ContainsDiagnostics)
                return;

            ParameterInfo parameterInfo = SyntaxInfo.ParameterInfo(conversionOperatorDeclaration);

            if (!parameterInfo.Success)
                return;

            if (ContainsOnlyThrowNewExpression(parameterInfo.Body))
                return;

            Analyze(context, parameterInfo);
        }

        private static void AnalyzeIndexerDeclaration(SyntaxNodeAnalysisContext context)
        {
            var indexerDeclaration = (IndexerDeclarationSyntax)context.Node;

            if (indexerDeclaration.ContainsDiagnostics)
                return;

            if (!indexerDeclaration.IsParentKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration))
                return;

            if (indexerDeclaration.Modifiers.ContainsAny(SyntaxKind.AbstractKeyword, SyntaxKind.VirtualKeyword, SyntaxKind.OverrideKeyword))
                return;

            ParameterInfo parameterInfo = SyntaxInfo.ParameterInfo(indexerDeclaration);

            if (!parameterInfo.Success)
                return;

            if (ContainsOnlyThrowNewExpression(parameterInfo.Body))
                return;

            IPropertySymbol propertySymbol = context.SemanticModel.GetDeclaredSymbol(indexerDeclaration, context.CancellationToken);

            if (propertySymbol?.ExplicitInterfaceImplementations.IsDefaultOrEmpty != true)
                return;

            if (propertySymbol.ImplementsInterfaceMember(allInterfaces: true))
                return;

            Analyze(context, parameterInfo, isIndexer: true);
        }

        private static void AnalyzeLocalFunctionStatement(SyntaxNodeAnalysisContext context)
        {
            var localFunctionStatement = (LocalFunctionStatementSyntax)context.Node;

            if (localFunctionStatement.ContainsDiagnostics)
                return;

            ParameterInfo parameterInfo = SyntaxInfo.ParameterInfo(localFunctionStatement);

            if (!parameterInfo.Success)
                return;

            IMethodSymbol methodSymbol = context.SemanticModel.GetDeclaredSymbol(localFunctionStatement, context.CancellationToken);

            if (methodSymbol == null)
                return;

            if (SymbolUtility.IsEventHandlerMethod(methodSymbol))
                return;

            Analyze(context, parameterInfo);
        }

        private static void AnalyzeSimpleLambdaExpression(SyntaxNodeAnalysisContext context)
        {
            var lambda = (SimpleLambdaExpressionSyntax)context.Node;

            if (lambda.ContainsDiagnostics)
                return;

            ParameterInfo parameterInfo = SyntaxInfo.ParameterInfo(lambda);

            if (!parameterInfo.Success)
                return;

            var methodSymbol = (IMethodSymbol)context.SemanticModel.GetSymbol(lambda, context.CancellationToken);

            if (methodSymbol == null)
                return;

            if (SymbolUtility.IsEventHandlerMethod(methodSymbol))
                return;

            Analyze(context, parameterInfo);
        }

        private static void AnalyzeParenthesizedLambdaExpression(SyntaxNodeAnalysisContext context)
        {
            var lambda = (ParenthesizedLambdaExpressionSyntax)context.Node;

            if (lambda.ContainsDiagnostics)
                return;

            ParameterInfo parameterInfo = SyntaxInfo.ParameterInfo(lambda);

            if (!parameterInfo.Success)
                return;

            var methodSymbol = (IMethodSymbol)context.SemanticModel.GetSymbol(lambda, context.CancellationToken);

            if (methodSymbol == null)
                return;

            if (SymbolUtility.IsEventHandlerMethod(methodSymbol))
                return;

            Analyze(context, parameterInfo);
        }

        private static void AnalyzeAnonymousMethodExpression(SyntaxNodeAnalysisContext context)
        {
            var anonymousMethod = (AnonymousMethodExpressionSyntax)context.Node;

            if (anonymousMethod.ContainsDiagnostics)
                return;

            ParameterInfo parameterInfo = SyntaxInfo.ParameterInfo(anonymousMethod);

            if (!parameterInfo.Success)
                return;

            var methodSymbol = (IMethodSymbol)context.SemanticModel.GetSymbol(anonymousMethod, context.CancellationToken);

            if (methodSymbol == null)
                return;

            if (SymbolUtility.IsEventHandlerMethod(methodSymbol))
                return;

            Analyze(context, parameterInfo);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, in ParameterInfo parameterInfo, bool isIndexer = false)
        {
            UnusedParameterWalker walker = UnusedParameterWalker.GetInstance();
            walker.SetValues(context.SemanticModel, context.CancellationToken, isIndexer);

            FindUnusedNodes(parameterInfo, walker);

            foreach (KeyValuePair<string, NodeSymbolInfo> kvp in walker.Nodes)
                ReportDiagnostic(context, kvp.Value.Node);

            UnusedParameterWalker.Free(walker);
        }

        private static void FindUnusedNodes(in ParameterInfo parameterInfo, UnusedParameterWalker walker)
        {
            if (parameterInfo.Parameter != null
                && !IsDiscardName(parameterInfo.Parameter.Identifier.ValueText))
            {
                walker.AddParameter(parameterInfo.Parameter);
            }
            else
            {
                foreach (ParameterSyntax parameter in parameterInfo.Parameters)
                {
                    if (!IsDiscardName(parameter.Identifier.ValueText))
                        walker.AddParameter(parameter);
                }
            }

            foreach (TypeParameterSyntax typeParameter in parameterInfo.TypeParameters)
            {
                walker.AddTypeParameter(typeParameter);
                walker.IsAnyTypeParameter = true;
            }

            if (walker.Nodes.Count > 0)
                walker.Visit(parameterInfo.Node);
        }

        private static bool IsDiscardName(string value)
        {
            if (value.Length > 0
                && value[0] == '_')
            {
                if (value.Length == 1)
                    return true;

                if (value[1] == '_')
                {
                    for (int i = 2; i < value.Length; i++)
                    {
                        if (value[i] != '_')
                            return false;
                    }

                    return true;
                }

                return uint.TryParse(value.Substring(1), out _);
            }

            return false;
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, SyntaxNode node)
        {
            if (node is ParameterSyntax parameter)
            {
                if (parameter.Modifiers.Contains(SyntaxKind.ThisKeyword))
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UnusedThisParameter, parameter, parameter.Identifier.ValueText);
                }
                else
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UnusedParameter, parameter, parameter.Identifier.ValueText);
                }
            }
            else if (node is TypeParameterSyntax typeParameter)
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UnusedTypeParameter, typeParameter, typeParameter.Identifier.ValueText);
            }
            else
            {
                Debug.Fail(node.ToString());
            }
        }

        private static bool ContainsOnlyThrowNewExpression(CSharpSyntaxNode node)
        {
            switch (node?.Kind())
            {
                case SyntaxKind.Block:
                    return ContainsOnlyThrowNewExpression((BlockSyntax)node);
                case SyntaxKind.ArrowExpressionClause:
                    return ContainsOnlyThrowNewExpression((ArrowExpressionClauseSyntax)node);
                case SyntaxKind.AccessorList:
                    {
                        return ((AccessorListSyntax)node)
                            .Accessors
                            .All(f => ContainsOnlyThrowNewExpression(f.BodyOrExpressionBody()));
                    }
            }

            return false;
        }

        private static bool ContainsOnlyThrowNewExpression(BlockSyntax body)
        {
            StatementSyntax statement = body?.Statements.SingleOrDefault(shouldThrow: false);

            if (statement?.Kind() == SyntaxKind.ThrowStatement)
            {
                var throwStatement = (ThrowStatementSyntax)statement;

                return throwStatement.Expression?.Kind() == SyntaxKind.ObjectCreationExpression;
            }

            return false;
        }

        private static bool ContainsOnlyThrowNewExpression(ArrowExpressionClauseSyntax expressionBody)
        {
            ExpressionSyntax expression = expressionBody?.Expression;

            if (expression?.Kind() == SyntaxKind.ThrowExpression)
            {
                var throwExpression = (ThrowExpressionSyntax)expression;

                return throwExpression.Expression?.Kind() == SyntaxKind.ObjectCreationExpression;
            }

            return false;
        }
    }
}
