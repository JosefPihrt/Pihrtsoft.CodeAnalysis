// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncSuffixAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.AsynchronousMethodNameShouldEndWithAsync,
                    DiagnosticDescriptors.NonAsynchronousMethodNameShouldNotEndWithAsync,
                    DiagnosticDescriptors.NonAsynchronousMethodNameShouldNotEndWithAsyncFadeOut);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                INamedTypeSymbol asyncAction = startContext.Compilation.GetTypeByMetadataName("Windows.Foundation.IAsyncAction");

                bool shouldCheckWindowsRuntimeTypes = asyncAction != null;

                startContext.RegisterSyntaxNodeAction(
                    c =>
                    {
                        if (DiagnosticHelpers.IsAnyEffective(
                            c,
                            DiagnosticDescriptors.AsynchronousMethodNameShouldEndWithAsync,
                            DiagnosticDescriptors.NonAsynchronousMethodNameShouldNotEndWithAsync))
                        {
                            AnalyzeMethodDeclaration(c, shouldCheckWindowsRuntimeTypes);
                        }
                    },
                    SyntaxKind.MethodDeclaration);
            });
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context, bool shouldCheckWindowsRuntimeTypes)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (methodDeclaration.Modifiers.Contains(SyntaxKind.OverrideKeyword))
                return;

            if (methodDeclaration.Identifier.ValueText.EndsWith("Async", StringComparison.Ordinal))
            {
                IMethodSymbol methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

                if (methodSymbol.IsAsync)
                    return;

                if (!methodSymbol.Name.EndsWith("Async", StringComparison.Ordinal))
                    return;

                if (SymbolUtility.IsAwaitable(methodSymbol.ReturnType, shouldCheckWindowsRuntimeTypes))
                    return;

                SyntaxToken identifier = methodDeclaration.Identifier;

                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticDescriptors.NonAsynchronousMethodNameShouldNotEndWithAsync,
                    identifier);

                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticDescriptors.NonAsynchronousMethodNameShouldNotEndWithAsyncFadeOut,
                    Location.Create(identifier.SyntaxTree, TextSpan.FromBounds(identifier.Span.End - 5, identifier.Span.End)));
            }
            else
            {
                IMethodSymbol methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

                if (methodSymbol.Name.EndsWith("Async", StringComparison.Ordinal))
                    return;

                if (SymbolUtility.CanBeEntryPoint(methodSymbol))
                    return;

                if (!SymbolUtility.IsAwaitable(methodSymbol.ReturnType, shouldCheckWindowsRuntimeTypes))
                    return;

                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.AsynchronousMethodNameShouldEndWithAsync, methodDeclaration.Identifier);
            }
        }
    }
}
