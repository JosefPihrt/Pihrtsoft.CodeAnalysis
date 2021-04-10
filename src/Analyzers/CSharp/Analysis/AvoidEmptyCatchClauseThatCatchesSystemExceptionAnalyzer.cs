// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidEmptyCatchClauseThatCatchesSystemExceptionAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AvoidEmptyCatchClauseThatCatchesSystemException);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                INamedTypeSymbol exceptionSymbol = startContext.Compilation.GetTypeByMetadataName("System.Exception");

                if (exceptionSymbol == null)
                    return;

                startContext.RegisterSyntaxNodeAction(nodeContext => AnalyzeCatchClause(nodeContext, exceptionSymbol), SyntaxKind.CatchClause);
            });
        }

        private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context, ITypeSymbol exceptionSymbol)
        {
            var catchClause = (CatchClauseSyntax)context.Node;

            if (catchClause.ContainsDiagnostics)
                return;

            if (catchClause.Filter != null)
                return;

            if (catchClause.Block?.Statements.Any() != false)
                return;

            TypeSyntax type = catchClause.Declaration?.Type;

            if (type == null)
                return;

            ITypeSymbol typeSymbol = context.SemanticModel.GetTypeSymbol(type, context.CancellationToken);

            if (!SymbolEqualityComparer.Default.Equals(typeSymbol, exceptionSymbol))
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.AvoidEmptyCatchClauseThatCatchesSystemException,
                catchClause.CatchKeyword);
        }
    }
}
