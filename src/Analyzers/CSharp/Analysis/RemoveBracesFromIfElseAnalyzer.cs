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
    public sealed class RemoveBracesFromIfElseAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.RemoveBracesFromIfElse,
                        DiagnosticRules.RemoveBracesFromIfElseFadeOut);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.RemoveBracesFromIfElse.IsEffective(c))
                        AnalyzeIfStatement(c);
                },
                SyntaxKind.IfStatement);
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            if (ifStatement.IsParentKind(SyntaxKind.ElseClause))
                return;

            if (ifStatement.Else == null)
                return;

            BracesAnalysis analysis = BracesAnalysis.AnalyzeBraces(ifStatement);

            if (!analysis.RemoveBraces)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveBracesFromIfElse, ifStatement);

            foreach (IfStatementOrElseClause ifOrElse in ifStatement.AsCascade())
            {
                if (ifOrElse.Statement is BlockSyntax block)
                    CSharpDiagnosticHelpers.ReportBraces(context, DiagnosticRules.RemoveBracesFromIfElseFadeOut, block);
            }
        }
    }
}
