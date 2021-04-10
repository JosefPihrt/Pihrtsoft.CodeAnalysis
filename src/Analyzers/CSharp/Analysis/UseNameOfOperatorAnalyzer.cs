// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseNameOfOperatorAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.UseNameOfOperator,
                        DiagnosticRules.UseNameOfOperatorFadeOut);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (((CSharpCompilation)startContext.Compilation).LanguageVersion < LanguageVersion.CSharp6)
                    return;

                startContext.RegisterSyntaxNodeAction(
                    c =>
                    {
                        if (DiagnosticRules.UseNameOfOperator.IsEffective(c))
                            AnalyzeArgument(c);
                    },
                    SyntaxKind.Argument);
            });
        }

        public static void Analyze(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            ExpressionSyntax expression = invocationInfo.Expression;

            if (expression.Kind() != SyntaxKind.SimpleMemberAccessExpression)
                return;

            var memberAccessExpression = (MemberAccessExpressionSyntax)expression;

            if (!(context.SemanticModel.GetSymbol(memberAccessExpression, context.CancellationToken) is IFieldSymbol fieldSymbol))
                return;

            INamedTypeSymbol containingType = fieldSymbol.ContainingType;

            if (containingType?.TypeKind != TypeKind.Enum)
                return;

            if (containingType.HasAttribute(MetadataNames.System_FlagsAttribute))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseNameOfOperator, invocationInfo.InvocationExpression);
        }

        private static void AnalyzeArgument(SyntaxNodeAnalysisContext context)
        {
            var argument = (ArgumentSyntax)context.Node;

            ExpressionSyntax expression = argument.Expression;

            if (expression?.IsKind(SyntaxKind.StringLiteralExpression) != true)
                return;

            ParameterInfo parameterInfo = GetNextParametersInScope(argument);

            if (parameterInfo.Success)
            {
                Analyze(context, argument, expression, parameterInfo);
            }
            else
            {
                Analyze(context, argument, expression);
            }
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            ArgumentSyntax argument,
            ExpressionSyntax expression,
            in ParameterInfo parameterInfo)
        {
            var literalExpression = (LiteralExpressionSyntax)expression;

            ParameterSyntax parameter = FindMatchingParameter(parameterInfo, literalExpression.Token.ValueText);

            if (parameter == null)
                return;

            IParameterSymbol parameterSymbol = context.SemanticModel.DetermineParameter(argument, allowParams: true, allowCandidate: false, cancellationToken: context.CancellationToken);

            if (parameterSymbol == null)
                return;

            string parameterName = parameterSymbol.Name;

            if (!string.Equals(parameterName, "paramName", StringComparison.Ordinal)
                && !string.Equals(parameterName, "parameterName", StringComparison.Ordinal))
            {
                return;
            }

            ReportDiagnostic(context, literalExpression, parameter.Identifier.Text);
        }

        private static ParameterSyntax FindMatchingParameter(ParameterInfo parameterInfo, string name)
        {
            do
            {
                if (parameterInfo.Parameter != null)
                {
                    ParameterSyntax parameter = parameterInfo.Parameter;

                    if (string.Equals(parameter.Identifier.ValueText, name, StringComparison.Ordinal))
                        return parameter;
                }
                else if (parameterInfo.ParameterList != null)
                {
                    foreach (ParameterSyntax parameter in parameterInfo.ParameterList.Parameters)
                    {
                        if (string.Equals(parameter.Identifier.ValueText, name, StringComparison.Ordinal))
                            return parameter;
                    }
                }

                parameterInfo = GetNextParametersInScope(parameterInfo.Node);

            } while (parameterInfo.Success);

            return null;
        }

        private static ParameterInfo GetNextParametersInScope(SyntaxNode node)
        {
            for (SyntaxNode current = node.Parent; current != null; current = current.Parent)
            {
                ParameterInfo info = ParameterInfo.Create(current);

                if (info.Success)
                    return info;
            }

            return ParameterInfo.Empty;
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, ExpressionSyntax expression)
        {
            AccessorDeclarationSyntax accessor = argument.FirstAncestor<AccessorDeclarationSyntax>();

            if (accessor?.IsKind(SyntaxKind.SetAccessorDeclaration) != true)
                return;

            PropertyDeclarationSyntax property = accessor.FirstAncestor<PropertyDeclarationSyntax>();

            if (property == null)
                return;

            var literalExpression = (LiteralExpressionSyntax)expression;

            string text = literalExpression.Token.ValueText;

            if (!string.Equals(property.Identifier.ValueText, text, StringComparison.Ordinal))
                return;

            IParameterSymbol parameterSymbol = context.SemanticModel.DetermineParameter(argument, allowParams: true, allowCandidate: false, cancellationToken: context.CancellationToken);

            if (parameterSymbol == null)
                return;

            if (!string.Equals(parameterSymbol.Name, "propertyName", StringComparison.Ordinal))
                return;

            ReportDiagnostic(context, literalExpression, property.Identifier.Text);
        }

        private static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            LiteralExpressionSyntax literalExpression,
            string identifier)
        {
            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.UseNameOfOperator,
                literalExpression.GetLocation(),
                ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>("Identifier", identifier) }));

            string text = literalExpression.Token.Text;

            if (text.Length >= 2)
            {
                SyntaxTree syntaxTree = literalExpression.SyntaxTree;
                TextSpan span = literalExpression.Span;

                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticRules.UseNameOfOperatorFadeOut,
                    Location.Create(syntaxTree, new TextSpan(span.Start, (text[0] == '@') ? 2 : 1)));

                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticRules.UseNameOfOperatorFadeOut,
                    Location.Create(syntaxTree, new TextSpan(span.End - 1, 1)));
            }
        }

        private readonly struct ParameterInfo
        {
            public static ParameterInfo Empty { get; } = new ParameterInfo();

            private ParameterInfo(SyntaxNode node, ParameterSyntax parameter)
            {
                Node = node;
                Parameter = parameter;
                ParameterList = null;
            }

            private ParameterInfo(SyntaxNode node, BaseParameterListSyntax parameterList)
            {
                Node = node;
                Parameter = null;
                ParameterList = parameterList;
            }

            public ParameterSyntax Parameter { get; }

            public BaseParameterListSyntax ParameterList { get; }

            public SyntaxNode Node { get; }

            public bool Success
            {
                get { return Node != null; }
            }

            public static ParameterInfo Create(SyntaxNode node)
            {
                switch (node.Kind())
                {
                    case SyntaxKind.MethodDeclaration:
                        return new ParameterInfo(node, ((MethodDeclarationSyntax)node).ParameterList);
                    case SyntaxKind.ConstructorDeclaration:
                        return new ParameterInfo(node, ((ConstructorDeclarationSyntax)node).ParameterList);
                    case SyntaxKind.IndexerDeclaration:
                        return new ParameterInfo(node, ((IndexerDeclarationSyntax)node).ParameterList);
                    case SyntaxKind.SimpleLambdaExpression:
                        return new ParameterInfo(node, ((SimpleLambdaExpressionSyntax)node).Parameter);
                    case SyntaxKind.ParenthesizedLambdaExpression:
                        return new ParameterInfo(node, ((ParenthesizedLambdaExpressionSyntax)node).ParameterList);
                    case SyntaxKind.AnonymousMethodExpression:
                        return new ParameterInfo(node, ((AnonymousMethodExpressionSyntax)node).ParameterList);
                    case SyntaxKind.LocalFunctionStatement:
                        return new ParameterInfo(node, ((LocalFunctionStatementSyntax)node).ParameterList);
                }

                return Empty;
            }
        }
    }
}
