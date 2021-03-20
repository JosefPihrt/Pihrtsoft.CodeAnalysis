// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Spelling
{
    internal class SpellingAnalysisContext
    {
        private readonly Action<Diagnostic> _reportDiagnostic;

        private readonly SpellingParser _parser;

        public SpellingData SpellingData { get; }

        public SpellingFixerOptions Options { get; }

        public CancellationToken CancellationToken { get; }

        public SpellingAnalysisContext(
            Action<Diagnostic> reportDiagnostic,
            SpellingData spellingData,
            SpellingFixerOptions options,
            CancellationToken cancellationToken)
        {
            SpellingData = spellingData;
            Options = options;
            CancellationToken = cancellationToken;

            _reportDiagnostic = reportDiagnostic;

            _parser = new SpellingParser(spellingData, new SpellingParserOptions(options.SplitMode, options.MinWordLength), cancellationToken);
        }

        public void AnalyzeText(string value, TextSpan textSpan, SyntaxTree syntaxTree)
        {
            ImmutableArray<SpellingMatch> matches = _parser.AnalyzeText(value);

            ProcessMatches(matches, textSpan, syntaxTree);
        }

        public void AnalyzeIdentifier(
            SyntaxToken identifier,
            int prefixLength = 0)
        {
            ImmutableArray<SpellingMatch> matches = _parser.AnalyzeIdentifier(identifier.ValueText, prefixLength);

            ProcessMatches(matches, identifier.Span, identifier.SyntaxTree);
        }

        private void ProcessMatches(
            ImmutableArray<SpellingMatch> matches,
            TextSpan span,
            SyntaxTree syntaxTree)
        {
            foreach (SpellingMatch match in matches)
            {
                Diagnostic diagnostic = Diagnostic.Create(
                    SpellingAnalyzer.DiagnosticDescriptor,
                    Location.Create(syntaxTree, new TextSpan(span.Start + match.Index, match.Value.Length)),
                    match.Value);

                _reportDiagnostic(diagnostic);
            }
        }
    }
}
