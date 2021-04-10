// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RemoveRedundantSealedModifierAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveRedundantSealedModifier);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSymbolAction(f => AnalyzeMethod(f), SymbolKind.Method);
            context.RegisterSymbolAction(f => AnalyzeProperty(f), SymbolKind.Property);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            ISymbol symbol = context.Symbol;

            if (((IMethodSymbol)symbol).MethodKind != MethodKind.Ordinary)
                return;

            Analyze(context, symbol);
        }

        private static void AnalyzeProperty(SymbolAnalysisContext context)
        {
            Analyze(context, context.Symbol);
        }

        private static void Analyze(SymbolAnalysisContext context, ISymbol symbol)
        {
            if (symbol.IsImplicitlyDeclared)
                return;

            if (!symbol.IsSealed)
                return;

            if (symbol.ContainingType?.IsSealed != true)
                return;

            Debug.Assert(symbol.ContainingType.TypeKind == TypeKind.Class, symbol.ContainingType.TypeKind.ToString());

            SyntaxNode node = symbol.GetSyntax(context.CancellationToken);

            Debug.Assert(node.IsKind(SyntaxKind.MethodDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.IndexerDeclaration), node.Kind().ToString());

            ModifierListInfo info = SyntaxInfo.ModifierListInfo(node);

            Debug.Assert(info.IsSealed, info.Modifiers.ToString());

            if (!info.IsSealed)
                return;

            SyntaxToken sealedKeyword = info.Modifiers.Find(SyntaxKind.SealedKeyword);

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveRedundantSealedModifier, sealedKeyword);
        }
    }
}
