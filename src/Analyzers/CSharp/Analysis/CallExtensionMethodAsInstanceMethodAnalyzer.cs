// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CallExtensionMethodAsInstanceMethodAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.CallExtensionMethodAsInstanceMethod);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeInvocationExpression(f), SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (invocation.ContainsDiagnostics)
                return;

            if (invocation.SpanContainsDirectives())
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            CallExtensionMethodAsInstanceMethodAnalysisResult analysis = CallExtensionMethodAsInstanceMethodAnalysis.Analyze(invocation, semanticModel, allowAnyExpression: false, cancellationToken: cancellationToken);

            if (!analysis.Success)
                return;

            if (SymbolEqualityComparer.Default.Equals(
                semanticModel.GetEnclosingNamedType(analysis.InvocationExpression.SpanStart, cancellationToken),
                analysis.MethodSymbol.ContainingType))
            {
                return;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.CallExtensionMethodAsInstanceMethod, invocation);
        }
    }
}
