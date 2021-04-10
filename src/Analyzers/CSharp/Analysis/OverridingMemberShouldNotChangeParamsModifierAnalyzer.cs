// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OverridingMemberShouldNotChangeParamsModifierAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.OverridingMemberShouldNotChangeParamsModifier);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSymbolAction(f => AnalyzeMethodSymbol(f), SymbolKind.Method);
            context.RegisterSymbolAction(f => AnalyzePropertySymbol(f), SymbolKind.Property);
        }

        private static void AnalyzeMethodSymbol(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            IParameterSymbol lastParameterSymbol = methodSymbol.OverriddenMethod?.Parameters.LastOrDefault();

            if (lastParameterSymbol == null)
                return;

            if (!(methodSymbol.GetSyntaxOrDefault(context.CancellationToken) is MethodDeclarationSyntax methodDeclaration))
                return;

            ParameterSyntax lastParameter = methodDeclaration.ParameterList?.Parameters.LastOrDefault();

            if (lastParameter == null)
                return;

            if (lastParameter.Modifiers.Contains(SyntaxKind.ParamsKeyword) == lastParameterSymbol.IsParams)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.OverridingMemberShouldNotChangeParamsModifier, lastParameter);
        }

        private static void AnalyzePropertySymbol(SymbolAnalysisContext context)
        {
            var propertySymbol = (IPropertySymbol)context.Symbol;

            if (!propertySymbol.IsIndexer)
                return;

            IParameterSymbol lastParameterSymbol = propertySymbol.OverriddenProperty?.Parameters.LastOrDefault();

            if (lastParameterSymbol == null)
                return;

            if (!(propertySymbol.GetSyntaxOrDefault(context.CancellationToken) is IndexerDeclarationSyntax indexerDeclaration))
                return;

            ParameterSyntax lastParameter = indexerDeclaration.ParameterList?.Parameters.LastOrDefault();

            if (lastParameter == null)
                return;

            if (lastParameter.Modifiers.Contains(SyntaxKind.ParamsKeyword) == lastParameterSymbol.IsParams)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.OverridingMemberShouldNotChangeParamsModifier, lastParameter);
        }
    }
}
