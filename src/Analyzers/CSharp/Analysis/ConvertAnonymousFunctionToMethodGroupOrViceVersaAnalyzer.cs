// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ConvertAnonymousFunctionToMethodGroupOrViceVersaAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa,
                        DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersaFadeOut,
                        CommonDiagnosticRules.AnalyzerIsObsolete);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                startContext.RegisterSyntaxNodeAction(f => AnalyzeSimpleLambdaExpression(f), SyntaxKind.SimpleLambdaExpression);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeParenthesizedLambdaExpression(f), SyntaxKind.ParenthesizedLambdaExpression);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeAnonymousMethodExpression(f), SyntaxKind.AnonymousMethodExpression);

                startContext.RegisterSyntaxNodeAction(f => AnalyzeArgument(f), SyntaxKind.Argument);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeEqualsValueClause(f), SyntaxKind.EqualsValueClause);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeAssignment(f), SyntaxKind.SimpleAssignmentExpression, SyntaxKind.AddAssignmentExpression, SyntaxKind.SubtractAssignmentExpression, SyntaxKind.CoalesceAssignmentExpression);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeReturnStatement(f), SyntaxKind.ReturnStatement);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeYieldReturnStatement(f), SyntaxKind.YieldReturnStatement);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeArrowExpressionClause(f), SyntaxKind.ArrowExpressionClause);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeSwitchExpressionArm(f), SyntaxKind.SwitchExpressionArm);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeArrayInitializer(f), SyntaxKind.ArrayInitializerExpression, SyntaxKind.CollectionInitializerExpression);
#if DEBUG
                startContext.RegisterSyntaxNodeAction(f => AnalyzeIdentifierName(f), SyntaxKind.IdentifierName);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeSimpleMemberAccessExpression(f), SyntaxKind.SimpleMemberAccessExpression);
#endif
            });
        }

        private static void AnalyzeSimpleLambdaExpression(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa.IsEffective(context))
                return;

            if (AnalyzerOptions.ConvertMethodGroupToAnonymousFunction.IsEnabled(context))
                return;

            if (context.Node.SpanContainsDirectives())
                return;

            var lambda = (SimpleLambdaExpressionSyntax)context.Node;

            InvocationExpressionSyntax invocationExpression = GetInvocationExpression(lambda.Body);

            if (invocationExpression == null)
                return;

            ExpressionSyntax expression = invocationExpression.Expression;

            if (!IsSimpleInvocation(expression))
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            if (!(semanticModel.GetSymbol(invocationExpression, cancellationToken) is IMethodSymbol methodSymbol))
                return;

            if (methodSymbol.MethodKind == MethodKind.DelegateInvoke)
                return;

            bool isReduced = methodSymbol.MethodKind == MethodKind.ReducedExtension;

            if (!methodSymbol.IsStatic
                && expression.Kind() != SyntaxKind.IdentifierName)
            {
                if (!ExpressionIsParameter(expression, lambda.Parameter))
                {
                    return;
                }
                else if (isReduced
                    && !SymbolEqualityComparer.Default.Equals(context.ContainingSymbol.ContainingType, methodSymbol.ContainingType))
                {
                    return;
                }
            }

            ImmutableArray<IParameterSymbol> parameterSymbols = (isReduced) ? methodSymbol.ReducedFrom.Parameters : methodSymbol.Parameters;

            if (parameterSymbols.Length != 1)
                return;

            ArgumentListSyntax argumentList = invocationExpression.ArgumentList;

            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;

            if (arguments.Count != ((isReduced) ? 0 : 1))
                return;

            ParameterSyntax parameter = lambda.Parameter;

            MemberAccessExpressionSyntax memberAccessExpression = (isReduced) ? (MemberAccessExpressionSyntax)expression : null;

            if (!CheckParameter(
                parameter,
                (isReduced) ? memberAccessExpression.Expression : arguments[0].Expression,
                parameterSymbols[0]))
            {
                return;
            }

            methodSymbol = (isReduced) ? methodSymbol.GetConstructedReducedFrom() : methodSymbol;

            if (!CheckInvokeMethod(lambda, methodSymbol, semanticModel, context.CancellationToken))
                return;

            if (!CheckSpeculativeSymbol(
                lambda,
                (isReduced) ? memberAccessExpression.Name.WithoutTrivia() : expression,
                methodSymbol,
                semanticModel,
                cancellationToken))
            {
                return;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa, lambda, AnalyzerOptions.ConvertMethodGroupToAnonymousFunction);

            FadeOut(context, parameter, null, lambda.Body as BlockSyntax, argumentList, lambda.ArrowToken, memberAccessExpression);
        }

        private static void AnalyzeParenthesizedLambdaExpression(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa.IsEffective(context))
                return;

            if (AnalyzerOptions.ConvertMethodGroupToAnonymousFunction.IsEnabled(context))
                return;

            if (context.Node.SpanContainsDirectives())
                return;

            var lambda = (ParenthesizedLambdaExpressionSyntax)context.Node;

            InvocationExpressionSyntax invocationExpression = GetInvocationExpression(lambda.Body);

            if (invocationExpression == null)
                return;

            ExpressionSyntax expression = invocationExpression.Expression;

            if (!IsSimpleInvocation(expression))
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            if (!(semanticModel.GetSymbol(invocationExpression, cancellationToken) is IMethodSymbol methodSymbol))
                return;

            if (methodSymbol.MethodKind == MethodKind.DelegateInvoke)
                return;

            if (!methodSymbol.IsStatic
                && expression.Kind() != SyntaxKind.IdentifierName)
            {
                if (!ExpressionIsParameter(expression, lambda.ParameterList))
                {
                    return;
                }
                else if (methodSymbol.MethodKind == MethodKind.ReducedExtension
                    && !SymbolEqualityComparer.Default.Equals(context.ContainingSymbol.ContainingType, methodSymbol.ContainingType))
                {
                    return;
                }
            }

            ImmutableArray<IParameterSymbol> parameterSymbols = methodSymbol.Parameters;

            ArgumentListSyntax argumentList = invocationExpression.ArgumentList;

            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;

            if (arguments.Count != parameterSymbols.Length)
                return;

            ParameterListSyntax parameterList = lambda.ParameterList;

            SeparatedSyntaxList<ParameterSyntax> parameters = parameterList.Parameters;

            bool isReduced = methodSymbol.MethodKind == MethodKind.ReducedExtension;

            if (parameters.Count != ((isReduced) ? arguments.Count + 1 : arguments.Count))
                return;

            MemberAccessExpressionSyntax memberAccessExpression = (isReduced) ? (MemberAccessExpressionSyntax)expression : null;

            if (isReduced)
            {
                if (!CheckParameter(
                    parameters[0],
                    memberAccessExpression.Expression,
                    methodSymbol.ReducedFrom.Parameters[0]))
                {
                    return;
                }

                parameters = parameters.RemoveAt(0);
            }

            if (!CheckParameters(parameters, arguments, parameterSymbols))
                return;

            methodSymbol = (isReduced) ? methodSymbol.GetConstructedReducedFrom() : methodSymbol;

            if (!CheckInvokeMethod(lambda, methodSymbol, semanticModel, context.CancellationToken))
                return;

            if (!CheckSpeculativeSymbol(
                lambda,
                (isReduced) ? memberAccessExpression.Name.WithoutTrivia() : expression,
                methodSymbol,
                semanticModel,
                cancellationToken))
            {
                return;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa, lambda, AnalyzerOptions.ConvertMethodGroupToAnonymousFunction);

            FadeOut(context, null, parameterList, lambda.Body as BlockSyntax, argumentList, lambda.ArrowToken, memberAccessExpression);
        }

        private static void AnalyzeAnonymousMethodExpression(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa.IsEffective(context))
                return;

            if (AnalyzerOptions.ConvertMethodGroupToAnonymousFunction.IsEnabled(context))
                return;

            if (context.Node.SpanContainsDirectives())
                return;

            var anonymousMethod = (AnonymousMethodExpressionSyntax)context.Node;

            ParameterListSyntax parameterList = anonymousMethod.ParameterList;

            if (parameterList == null)
                return;

            InvocationExpressionSyntax invocationExpression = GetInvocationExpression(anonymousMethod.Body);

            if (invocationExpression == null)
                return;

            ExpressionSyntax expression = invocationExpression.Expression;

            if (!IsSimpleInvocation(expression))
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            if (!(semanticModel.GetSymbol(invocationExpression, cancellationToken) is IMethodSymbol methodSymbol))
                return;

            if (methodSymbol.MethodKind == MethodKind.DelegateInvoke)
                return;

            if (!methodSymbol.IsStatic
                && expression.Kind() != SyntaxKind.IdentifierName)
            {
                if (!ExpressionIsParameter(expression, parameterList))
                {
                    return;
                }
                else if (methodSymbol.MethodKind == MethodKind.ReducedExtension
                    && !SymbolEqualityComparer.Default.Equals(context.ContainingSymbol.ContainingType, methodSymbol.ContainingType))
                {
                    return;
                }
            }

            ImmutableArray<IParameterSymbol> parameterSymbols = methodSymbol.Parameters;

            ArgumentListSyntax argumentList = invocationExpression.ArgumentList;

            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;

            if (arguments.Count != parameterSymbols.Length)
                return;

            SeparatedSyntaxList<ParameterSyntax> parameters = parameterList.Parameters;

            bool isReduced = methodSymbol.MethodKind == MethodKind.ReducedExtension;

            if (parameters.Count != ((isReduced) ? arguments.Count + 1 : arguments.Count))
                return;

            MemberAccessExpressionSyntax memberAccessExpression = (isReduced) ? (MemberAccessExpressionSyntax)expression : null;

            if (isReduced)
            {
                if (!CheckParameter(
                    parameters[0],
                    ((MemberAccessExpressionSyntax)expression).Expression,
                    methodSymbol.ReducedFrom.Parameters[0]))
                {
                    return;
                }

                parameters = parameters.RemoveAt(0);
            }

            if (!CheckParameters(parameters, arguments, parameterSymbols))
                return;

            methodSymbol = (isReduced) ? methodSymbol.GetConstructedReducedFrom() : methodSymbol;

            if (!CheckInvokeMethod(anonymousMethod, methodSymbol, semanticModel, context.CancellationToken))
                return;

            if (!CheckSpeculativeSymbol(
                anonymousMethod,
                (isReduced) ? memberAccessExpression.Name.WithoutTrivia() : expression,
                methodSymbol,
                semanticModel,
                cancellationToken))
            {
                return;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa, anonymousMethod, AnalyzerOptions.ConvertMethodGroupToAnonymousFunction);

            FadeOut(context, null, parameterList, anonymousMethod.Block, argumentList, anonymousMethod.DelegateKeyword, memberAccessExpression);
        }

        private static bool ExpressionIsParameter(
            ExpressionSyntax expression,
            ParameterSyntax parameter)
        {
            return expression.Kind() == SyntaxKind.SimpleMemberAccessExpression
                && ((MemberAccessExpressionSyntax)expression).Expression is IdentifierNameSyntax identifierName
                && identifierName.Identifier.ValueText == parameter.Identifier.ValueText;
        }

        private static bool ExpressionIsParameter(
            ExpressionSyntax expression,
            ParameterListSyntax parameterList)
        {
            if (expression.Kind() == SyntaxKind.SimpleMemberAccessExpression)
            {
                var memberAccessExpression = (MemberAccessExpressionSyntax)expression;

                if (memberAccessExpression.Expression is IdentifierNameSyntax identifierName)
                {
                    string name = identifierName.Identifier.ValueText;

                    foreach (ParameterSyntax parameter in parameterList.Parameters)
                    {
                        if (name == parameter.Identifier.ValueText)
                            return true;
                    }
                }
            }

            return false;
        }

        private static bool CheckInvokeMethod(
            AnonymousFunctionExpressionSyntax anonymousFunction,
            IMethodSymbol methodSymbol,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            var typeSymbol = semanticModel.GetTypeInfo(anonymousFunction, cancellationToken).ConvertedType as INamedTypeSymbol;

            IMethodSymbol invokeMethod = typeSymbol?.DelegateInvokeMethod;

            if (invokeMethod == null)
                return false;

            if (invokeMethod.ReturnType.IsVoid()
                && !methodSymbol.ReturnType.IsVoid())
            {
                return false;
            }

            ImmutableArray<IParameterSymbol> parameters = methodSymbol.Parameters;

            ImmutableArray<IParameterSymbol> parameters2 = invokeMethod.Parameters;

            if (parameters.Length != parameters2.Length)
                return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (!SymbolEqualityComparer.Default.Equals(parameters[i].Type, parameters2[i].Type))
                    return false;
            }

            return true;
        }

        private static bool CheckParameters(
            SeparatedSyntaxList<ParameterSyntax> parameters,
            SeparatedSyntaxList<ArgumentSyntax> arguments,
            ImmutableArray<IParameterSymbol> parameterSymbols)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                if (!CheckParameter(parameters[i], arguments[i].Expression, parameterSymbols[i]))
                    return false;
            }

            return true;
        }

        private static bool CheckParameter(
            ParameterSyntax parameter,
            ExpressionSyntax expression,
            IParameterSymbol parameterSymbol)
        {
            return parameterSymbol.RefKind == RefKind.None
                && !parameterSymbol.IsParams
                && string.Equals(
                    parameter.Identifier.ValueText,
                    (expression as IdentifierNameSyntax)?.Identifier.ValueText,
                    StringComparison.Ordinal);
        }

        private static bool CheckSpeculativeSymbol(
            AnonymousFunctionExpressionSyntax anonymousFunction,
            ExpressionSyntax expression,
            IMethodSymbol methodSymbol,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            SyntaxNode argumentExpression = anonymousFunction.WalkUpParentheses();

            if (argumentExpression.Parent is ArgumentSyntax argument)
            {
                if (argument.Parent is BaseArgumentListSyntax)
                {
                    SyntaxNode node = argument.Parent.Parent;

                    // related to https://github.com/dotnet/roslyn/issues/25262
                    if (CSharpUtility.IsConditionallyAccessed(node))
                        return false;

                    SyntaxNode newNode = node.ReplaceNode(argument.Expression, expression);

                    SymbolInfo symbolInfo = semanticModel.GetSpeculativeSymbolInfo(node.SpanStart, newNode, SpeculativeBindingOption.BindAsExpression);

                    methodSymbol = semanticModel.GetSymbol(node, cancellationToken) as IMethodSymbol;

                    return methodSymbol != null
                        && CheckSpeculativeSymbol(symbolInfo);
                }
            }
            else
            {
                SymbolInfo symbolInfo = semanticModel.GetSpeculativeSymbolInfo(anonymousFunction.SpanStart, expression, SpeculativeBindingOption.BindAsExpression);

                return CheckSpeculativeSymbol(symbolInfo);
            }

            return false;

            bool CheckSpeculativeSymbol(SymbolInfo symbolInfo)
            {
                return SymbolEqualityComparer.Default.Equals(symbolInfo.Symbol, methodSymbol)
                    || SymbolEqualityComparer.Default.Equals(symbolInfo.CandidateSymbols.SingleOrDefault(shouldThrow: false), methodSymbol);
            }
        }

        private static bool IsSimpleInvocation(ExpressionSyntax expression)
        {
            while (true)
            {
                switch (expression?.Kind())
                {
                    case SyntaxKind.IdentifierName:
                        {
                            return true;
                        }
                    case SyntaxKind.SimpleMemberAccessExpression:
                        {
                            expression = ((MemberAccessExpressionSyntax)expression).Expression;
                            break;
                        }
                    default:
                        {
                            return false;
                        }
                }
            }
        }

        internal static InvocationExpressionSyntax GetInvocationExpression(SyntaxNode node)
        {
            ExpressionSyntax expression = GetExpression(node)?.WalkDownParentheses();

            if (expression?.Kind() == SyntaxKind.InvocationExpression)
                return (InvocationExpressionSyntax)expression;

            return null;
        }

        private static ExpressionSyntax GetExpression(SyntaxNode node)
        {
            if (node is BlockSyntax block)
            {
                StatementSyntax statement = block.Statements.SingleOrDefault(shouldThrow: false);

                switch (statement?.Kind())
                {
                    case SyntaxKind.ExpressionStatement:
                        return ((ExpressionStatementSyntax)statement).Expression;
                    case SyntaxKind.ReturnStatement:
                        return ((ReturnStatementSyntax)statement).Expression;
                    default:
                        return null;
                }
            }

            return node as ExpressionSyntax;
        }

        private static void FadeOut(
            SyntaxNodeAnalysisContext context,
            ParameterSyntax parameter,
            ParameterListSyntax parameterList,
            BlockSyntax block,
            ArgumentListSyntax argumentList,
            SyntaxToken arrowTokenOrDelegateKeyword,
            MemberAccessExpressionSyntax memberAccessExpression)
        {
            DiagnosticDescriptor fadeOutDescriptor = DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersaFadeOut;

            if (parameter != null)
                DiagnosticHelpers.ReportNode(context, fadeOutDescriptor, parameter);

            if (parameterList != null)
                DiagnosticHelpers.ReportNode(context, fadeOutDescriptor, parameterList);

            if (!arrowTokenOrDelegateKeyword.IsKind(SyntaxKind.None))
                DiagnosticHelpers.ReportToken(context, fadeOutDescriptor, arrowTokenOrDelegateKeyword);

            if (block != null)
            {
                CSharpDiagnosticHelpers.ReportBraces(context, fadeOutDescriptor, block);

                if (block.Statements.SingleOrDefault(shouldThrow: false) is ReturnStatementSyntax returnStatement)
                    DiagnosticHelpers.ReportToken(context, fadeOutDescriptor, returnStatement.ReturnKeyword);
            }

            if (memberAccessExpression != null)
            {
                DiagnosticHelpers.ReportNode(context, fadeOutDescriptor, memberAccessExpression.Expression);
                DiagnosticHelpers.ReportToken(context, fadeOutDescriptor, memberAccessExpression.OperatorToken);
            }

            DiagnosticHelpers.ReportNode(context, fadeOutDescriptor, argumentList);
        }

        private static void AnalyzeArgument(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa.IsEffective(context))
                return;

            if (!AnalyzerOptions.ConvertMethodGroupToAnonymousFunction.IsEnabled(context))
                return;

            var argument = (ArgumentSyntax)context.Node;

            ExpressionSyntax expression = argument.Expression.WalkDownParentheses();

            if (!expression.IsKind(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression))
                return;

            IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(expression, context.CancellationToken);

            if (methodSymbol == null)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertMethodGroupToAnonymousFunction, expression, AnalyzerOptions.ConvertMethodGroupToAnonymousFunction);
        }

        private static void AnalyzeEqualsValueClause(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa.IsEffective(context))
                return;

            if (!AnalyzerOptions.ConvertMethodGroupToAnonymousFunction.IsEnabled(context))
                return;

            var argument = (EqualsValueClauseSyntax)context.Node;

            ExpressionSyntax expression = argument.Value.WalkDownParentheses();

            if (!expression.IsKind(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression))
                return;

            IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(expression, context.CancellationToken);

            if (methodSymbol == null)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertMethodGroupToAnonymousFunction, expression, AnalyzerOptions.ConvertMethodGroupToAnonymousFunction);
        }

        private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa.IsEffective(context))
                return;

            if (!AnalyzerOptions.ConvertMethodGroupToAnonymousFunction.IsEnabled(context))
                return;

            var argument = (AssignmentExpressionSyntax)context.Node;

            ExpressionSyntax expression = argument.Right.WalkDownParentheses();

            if (!expression.IsKind(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression))
                return;

            IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(expression, context.CancellationToken);

            if (methodSymbol == null)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertMethodGroupToAnonymousFunction, expression, AnalyzerOptions.ConvertMethodGroupToAnonymousFunction);
        }

        private static void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa.IsEffective(context))
                return;

            if (!AnalyzerOptions.ConvertMethodGroupToAnonymousFunction.IsEnabled(context))
                return;

            var returnStatement = (ReturnStatementSyntax)context.Node;

            ExpressionSyntax expression = returnStatement.Expression?.WalkDownParentheses();

            if (!expression.IsKind(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression))
                return;

            IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(expression, context.CancellationToken);

            if (methodSymbol == null)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertMethodGroupToAnonymousFunction, expression, AnalyzerOptions.ConvertMethodGroupToAnonymousFunction);
        }

        private static void AnalyzeYieldReturnStatement(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa.IsEffective(context))
                return;

            if (!AnalyzerOptions.ConvertMethodGroupToAnonymousFunction.IsEnabled(context))
                return;

            var yieldReturnStatement = (YieldStatementSyntax)context.Node;

            ExpressionSyntax expression = yieldReturnStatement.Expression?.WalkDownParentheses();

            if (!expression.IsKind(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression))
                return;

            IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(expression, context.CancellationToken);

            if (methodSymbol == null)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertMethodGroupToAnonymousFunction, expression, AnalyzerOptions.ConvertMethodGroupToAnonymousFunction);
        }

        private static void AnalyzeArrowExpressionClause(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa.IsEffective(context))
                return;

            if (!AnalyzerOptions.ConvertMethodGroupToAnonymousFunction.IsEnabled(context))
                return;

            var arrowExpressionClause = (ArrowExpressionClauseSyntax)context.Node;

            ExpressionSyntax expression = arrowExpressionClause.Expression?.WalkDownParentheses();

            if (!expression.IsKind(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression))
                return;

            IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(expression, context.CancellationToken);

            if (methodSymbol == null)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertMethodGroupToAnonymousFunction, expression, AnalyzerOptions.ConvertMethodGroupToAnonymousFunction);
        }

        //TODO: test
        private static void AnalyzeSwitchExpressionArm(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa.IsEffective(context))
                return;

            if (!AnalyzerOptions.ConvertMethodGroupToAnonymousFunction.IsEnabled(context))
                return;

            var switchExpressionArm = (SwitchExpressionArmSyntax)context.Node;

            ExpressionSyntax expression = switchExpressionArm.Expression?.WalkDownParentheses();

            if (!expression.IsKind(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression))
                return;

            IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(expression, context.CancellationToken);

            if (methodSymbol == null)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertMethodGroupToAnonymousFunction, expression, AnalyzerOptions.ConvertMethodGroupToAnonymousFunction);
        }

        private static void AnalyzeArrayInitializer(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa.IsEffective(context))
                return;

            if (!AnalyzerOptions.ConvertMethodGroupToAnonymousFunction.IsEnabled(context))
                return;

            var initializer = (InitializerExpressionSyntax)context.Node;

            foreach (ExpressionSyntax expression in initializer.Expressions)
            {
                ExpressionSyntax expression2 = expression?.WalkDownParentheses();

                if (expression2.IsKind(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression))
                {
                    IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(expression2, context.CancellationToken);

                    if (methodSymbol != null)
                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ReportOnly.ConvertMethodGroupToAnonymousFunction, expression2, AnalyzerOptions.ConvertMethodGroupToAnonymousFunction);
                }
            }
        }
#if DEBUG
        private static void AnalyzeIdentifierName(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa.IsEffective(context))
                return;

            if (!AnalyzerOptions.ConvertMethodGroupToAnonymousFunction.IsEnabled(context))
                return;

            var identifierName = (IdentifierNameSyntax)context.Node;

            ConvertMethodGroupToAnonymousFunctionAnalysis.IsFixable(identifierName, context.SemanticModel, context.CancellationToken);
        }

        private static void AnalyzeSimpleMemberAccessExpression(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertAnonymousFunctionToMethodGroupOrViceVersa.IsEffective(context))
                return;

            if (!AnalyzerOptions.ConvertMethodGroupToAnonymousFunction.IsEnabled(context))
                return;

            var simpleMemberAccess = (MemberAccessExpressionSyntax)context.Node;

            ConvertMethodGroupToAnonymousFunctionAnalysis.IsFixable(simpleMemberAccess, context.SemanticModel, context.CancellationToken);
        }
#endif
    }
}
