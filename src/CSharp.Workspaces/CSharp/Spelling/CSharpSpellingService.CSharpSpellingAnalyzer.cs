// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.Spelling;

namespace Roslynator.CSharp.Spelling
{
    internal partial class CSharpSpellingService
    {
        [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1001:Missing diagnostic analyzer attribute.")]
        private class CSharpSpellingAnalyzer : DiagnosticAnalyzer
        {
            private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = ImmutableArray.Create(SpellingAnalyzer.DiagnosticDescriptor);

            private readonly SpellingData _spellingData;
            private readonly SpellingFixerOptions _options;

            public CSharpSpellingAnalyzer(
                SpellingData spellingData,
                SpellingFixerOptions options)
            {
                _spellingData = spellingData;
                _options = options;
            }

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;

            public override void Initialize(AnalysisContext context)
            {
                context.EnableConcurrentExecution();

                context.ConfigureGeneratedCodeAnalysis((_options.IncludeGeneratedCode)
                    ? GeneratedCodeAnalysisFlags.ReportDiagnostics
                    : GeneratedCodeAnalysisFlags.None);

                context.RegisterSyntaxTreeAction(f => AnalyzeSyntaxTree(f));
            }

            private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
            {
                SyntaxTree tree = context.Tree;

                SyntaxNode root = tree.GetRoot(context.CancellationToken);

                var analysisContext = new SpellingAnalysisContext(
                    diagnostic => context.ReportDiagnostic(diagnostic),
                    _spellingData,
                    _options,
                    context.CancellationToken);

                CSharpSpellingWalker walker = CSharpSpellingWalker.Create(analysisContext);

                walker.Visit(root);
            }
        }
    }
}
