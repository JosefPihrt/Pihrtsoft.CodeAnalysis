// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Spelling;

namespace Roslynator.CSharp.Spelling
{
    [Export(typeof(ILanguageService))]
    [ExportMetadata("Language", LanguageNames.CSharp)]
    [ExportMetadata("ServiceType", "Roslynator.Spelling.ISpellingService")]
    internal class CSharpSpellingService : SpellingService
    {
        public override ISyntaxFactsService SyntaxFacts => CSharpSyntaxFactsService.Instance;

        public override DiagnosticAnalyzer CreateAnalyzer(
            SpellingData spellingData,
            SpellingFixerOptions options)
        {
            return new CSharpSpellingAnalyzer(spellingData, options);
        }

        public override ImmutableArray<Diagnostic> AnalyzeSpelling(
            SyntaxNode node,
            SpellingData spellingData,
            SpellingFixerOptions options,
            CancellationToken cancellationToken = default)
        {
            var diagnostics = new List<Diagnostic>();

            var analysisContext = new SpellingAnalysisContext(
                diagnostic => diagnostics.Add(diagnostic),
                spellingData,
                options,
                cancellationToken);

            var walker = new CSharpSpellingWalker(analysisContext);

            walker.Visit(node);

            return diagnostics.ToImmutableArray();
        }

        public override SpellingDiagnostic CreateSpellingDiagnostic(Diagnostic diagnostic)
        {
            Location location = diagnostic.Location;
            SyntaxTree syntaxTree = location.SourceTree;
            SyntaxNode root = syntaxTree.GetRoot();
            TextSpan span = location.SourceSpan;

            string value = diagnostic.Properties["Value"];
            int index = location.SourceSpan.Start;
            string parent = diagnostic.Properties.GetValueOrDefault("Parent");

            int parentIndex = (diagnostic.Properties.TryGetValue("ParentIndex", out string parentIndexText))
                ? int.Parse(parentIndexText)
                : 0;

            SyntaxTrivia trivia = root.FindTrivia(span.Start, findInsideTrivia: true);

            if (trivia.IsKind(
                SyntaxKind.SingleLineCommentTrivia,
                SyntaxKind.MultiLineCommentTrivia,
                SyntaxKind.PreprocessingMessageTrivia))
            {
                Debug.Assert(value == trivia.ToString().Substring(span.Start - trivia.SpanStart, span.Length), value);

                return new CSharpSpellingDiagnostic(diagnostic, value, index, parent, parentIndex);
            }

            SyntaxToken token = root.FindToken(span.Start, findInsideTrivia: true);

            if (token.IsKind(
                SyntaxKind.IdentifierToken,
                SyntaxKind.XmlTextLiteralToken))
            {
                Debug.Assert(value == token.ToString().Substring(span.Start - token.SpanStart, span.Length), value);

                return new CSharpSpellingDiagnostic(
                    diagnostic,
                    value,
                    index,
                    parent,
                    parentIndex,
                    (token.IsKind(SyntaxKind.IdentifierToken)) ? token : default);
            }

            throw new InvalidOperationException();
        }

        [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1001:Missing diagnostic analyzer attribute.")]
        private class CSharpSpellingAnalyzer : DiagnosticAnalyzer
        {
            private readonly SpellingData _spellingData;
            private readonly SpellingFixerOptions _options;

            public CSharpSpellingAnalyzer(
                SpellingData spellingData,
                SpellingFixerOptions options)
            {
                _spellingData = spellingData;
                _options = options;
            }

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get { return ImmutableArray.Create(SpellingAnalyzer.DiagnosticDescriptor); }
            }

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

                var walker = new CSharpSpellingWalker(analysisContext);

                walker.Visit(root);
            }
        }
    }
}
