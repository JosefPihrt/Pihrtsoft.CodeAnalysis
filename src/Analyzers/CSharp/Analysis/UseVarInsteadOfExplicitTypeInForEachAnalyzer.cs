// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseVarInsteadOfExplicitTypeInForEachAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UseVarInsteadOfExplicitTypeInForEach);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeForEachStatement(f), SyntaxKind.ForEachStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeForEachVariableStatement(f), SyntaxKind.ForEachVariableStatement);
        }

        private static void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (ForEachStatementSyntax)context.Node;

            TypeAnalysis analysis = CSharpTypeAnalysis.AnalyzeType(forEachStatement, context.SemanticModel);

            if (analysis.IsExplicit
                && analysis.SupportsImplicit)
            {
                ReportDiagnostic(context, forEachStatement.Type);
            }
        }

        private static void AnalyzeForEachVariableStatement(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (ForEachVariableStatementSyntax)context.Node;

            TypeAnalysis analysis = CSharpTypeAnalysis.AnalyzeType(forEachStatement, context.SemanticModel);

            if (analysis.IsExplicit
                && analysis.SupportsImplicit)
            {
                ReportDiagnostic(context, forEachStatement.Variable);
            }
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, SyntaxNode node)
        {
            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.UseVarInsteadOfExplicitTypeInForEach,
                node);
        }
    }
}
