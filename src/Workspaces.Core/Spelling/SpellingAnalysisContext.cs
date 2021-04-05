// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Spelling
{
    internal class SpellingAnalysisContext
    {
        private readonly Action<Diagnostic> _reportDiagnostic;

        private readonly Spellchecker _spellchecker;

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

            _spellchecker = new Spellchecker(spellingData, new SpellcheckerOptions(options.SplitMode, options.MinWordLength));
        }

        public void AnalyzeText(string value, TextSpan textSpan, SyntaxTree syntaxTree)
        {
            ImmutableArray<SpellingMatch> matches = _spellchecker.AnalyzeText(value);

            ProcessMatches(matches, textSpan, syntaxTree);
        }

        public void AnalyzeIdentifier(
            SyntaxToken identifier,
            int prefixLength = 0)
        {
            ImmutableArray<SpellingMatch> matches = _spellchecker.AnalyzeIdentifier(identifier.ValueText, prefixLength);

            ProcessMatches(matches, identifier.Span, identifier.SyntaxTree);
        }

        private void ProcessMatches(
            ImmutableArray<SpellingMatch> matches,
            TextSpan span,
            SyntaxTree syntaxTree)
        {
            foreach (SpellingMatch match in matches)
            {
                int index = span.Start + match.Index;

                ImmutableDictionary<string, string> properties;

                if (match.Parent != null)
                {
                    properties = ImmutableDictionary.CreateRange(new[]
                        {
                            new KeyValuePair<string, string>("Value", match.Value),
                            new KeyValuePair<string, string>("Parent", match.Parent),
                            new KeyValuePair<string, string>("ParentIndex", (span.Start + match.ParentIndex).ToString(CultureInfo.InvariantCulture)),
                        });
                }
                else
                {
                    properties = ImmutableDictionary.CreateRange(new[]
                        {
                            new KeyValuePair<string, string>("Value", match.Value),
                        });
                }

                Diagnostic diagnostic = Diagnostic.Create(
                    SpellingAnalyzer.DiagnosticDescriptor,
                    Location.Create(syntaxTree, new TextSpan(index, match.Value.Length)),
                    properties: properties,
                    messageArgs: match.Value);

                _reportDiagnostic(diagnostic);
            }
        }
    }
}
