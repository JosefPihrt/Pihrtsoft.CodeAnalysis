// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RemoveFileWithNoCodeAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveFileWithNoCode);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeCompilationUnit(f), SyntaxKind.CompilationUnit);
        }

        private static void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext context)
        {
            var compilationUnit = (CompilationUnitSyntax)context.Node;

            SyntaxToken token = compilationUnit.EndOfFileToken;

            if (compilationUnit.Span == token.Span
                && !token.HasTrailingTrivia
                && token.LeadingTrivia.All(f => !f.IsDirective))
            {
                SyntaxTree syntaxTree = compilationUnit.SyntaxTree;

                if (!GeneratedCodeUtility.IsGeneratedCodeFile(syntaxTree.FilePath))
                {
                    DiagnosticHelpers.ReportDiagnostic(
                        context,
                        DiagnosticRules.RemoveFileWithNoCode,
                        Location.Create(syntaxTree, default(TextSpan)));
                }
            }
        }
    }
}
