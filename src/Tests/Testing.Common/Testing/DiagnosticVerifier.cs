// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Testing.Text;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents verifier for a diagnostic that is produced by <see cref="DiagnosticAnalyzer"/>.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class DiagnosticVerifier : CodeVerifier
    {
        private ImmutableArray<DiagnosticAnalyzer> _analyzers;
        private ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        internal DiagnosticVerifier(WorkspaceFactory workspaceFactory, IAssert assert) : base(workspaceFactory, assert)
        {
        }

        /// <summary>
        /// Gets a <see cref="DiagnosticDescriptor"/> that describes diagnostic that should be verified.
        /// </summary>
        public abstract DiagnosticDescriptor Descriptor { get; }

        /// <summary>
        /// Gets an analyzer that can produce a diagnostic that should be verified.
        /// </summary>
        protected abstract DiagnosticAnalyzer Analyzer { get; }

        /// <summary>
        /// Gets a collection of additional analyzers that can produce a diagnostic that should be verified.
        /// Override this property if a diagnostic that should be verified can be produced by more than one analyzer.
        /// </summary>
        protected virtual ImmutableArray<DiagnosticAnalyzer> AdditionalAnalyzers { get; } = ImmutableArray<DiagnosticAnalyzer>.Empty;

        /// <summary>
        /// A collection of analyzers that can produce a diagnostic that should be verified.
        /// </summary>
        public ImmutableArray<DiagnosticAnalyzer> Analyzers
        {
            get
            {
                if (_analyzers.IsDefault)
                    ImmutableInterlocked.InterlockedInitialize(ref _analyzers, CreateAnalyzers());

                return _analyzers;

                ImmutableArray<DiagnosticAnalyzer> CreateAnalyzers()
                {
                    if (AdditionalAnalyzers.IsDefaultOrEmpty)
                    {
                        return ImmutableArray.Create(Analyzer);
                    }
                    else
                    {
                        ImmutableArray<DiagnosticAnalyzer>.Builder builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>(AdditionalAnalyzers.Length + 1);

                        builder.Add(Analyzer);
                        builder.AddRange(AdditionalAnalyzers);

                        return builder.ToImmutable();
                    }
                }
            }
        }

        internal ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    ImmutableInterlocked.InterlockedInitialize(ref _supportedDiagnostics, Analyzers.SelectMany(f => f.SupportedDiagnostics).ToImmutableArray());

                return _supportedDiagnostics;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get { return $"{Descriptor.Id} {string.Join(", ", Analyzers.Select(f => f.GetType().Name))}"; }
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens [| and |] represents start and end of selection respectively.</param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAsync(
            string source,
            CodeVerificationOptions options = null,
            CancellationToken cancellationToken = default)
        {
            TextParserResult result = TextParser.GetSpans(source);

            await VerifyDiagnosticAsync(
                result.Text,
                result.Spans.Select(f => CreateDiagnostic(f.Span, f.LineSpan)),
                additionalSources: null,
                options: options,
                cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source">Source text that contains placeholder [||] to be replaced with <paramref name="sourceData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAsync(
            string source,
            string sourceData,
            CodeVerificationOptions options = null,
            CancellationToken cancellationToken = default)
        {
            (TextSpan span, string text) = TextParser.ReplaceEmptySpan(source, sourceData);

            TextParserResult result = TextParser.GetSpans(text);

            if (result.Spans.Any())
            {
                await VerifyDiagnosticAsync(result.Text, result.Spans.Select(f => f.Span), options, cancellationToken);
            }
            else
            {
                await VerifyDiagnosticAsync(text, span, options, cancellationToken);
            }
        }

        internal async Task VerifyDiagnosticAsync(
            string source,
            TextSpan span,
            CodeVerificationOptions options = null,
            CancellationToken cancellationToken = default)
        {
            await VerifyDiagnosticAsync(
                source,
                CreateDiagnostic(source, span),
                options,
                cancellationToken);
        }

        internal async Task VerifyDiagnosticAsync(
            string source,
            IEnumerable<TextSpan> spans,
            CodeVerificationOptions options = null,
            CancellationToken cancellationToken = default)
        {
            await VerifyDiagnosticAsync(
                source,
                spans.Select(span => CreateDiagnostic(source, span)),
                additionalSources: null,
                options: options,
                cancellationToken);
        }

        internal async Task VerifyDiagnosticAsync(
            string source,
            Diagnostic expectedDiagnostic,
            CodeVerificationOptions options = null,
            CancellationToken cancellationToken = default)
        {
            await VerifyDiagnosticAsync(
                source,
                new Diagnostic[] { expectedDiagnostic },
                additionalSources: null,
                options: options,
                cancellationToken);
        }

        internal async Task VerifyDiagnosticAsync(
            string source,
            IEnumerable<Diagnostic> expectedDiagnostics,
            IEnumerable<string> additionalSources = null,
            CodeVerificationOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            using (Workspace workspace = new AdhocWorkspace())
            {
                Document document = WorkspaceFactory.CreateDocument(workspace.CurrentSolution, source, additionalSources, options);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                compilation = UpdateCompilation(compilation);

                ImmutableArray<Diagnostic> diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(Analyzers, DiagnosticComparer.SpanStart, cancellationToken);

                if (diagnostics.Length > 0
                    && SupportedDiagnostics.Length > 1)
                {
                    VerifyDiagnostics(FilterDiagnostics(diagnostics), expectedDiagnostics, cancellationToken);
                }
                else
                {
                    VerifyDiagnostics(diagnostics, expectedDiagnostics, cancellationToken);
                }
            }

            IEnumerable<Diagnostic> FilterDiagnostics(ImmutableArray<Diagnostic> diagnostics)
            {
                foreach (Diagnostic diagnostic in diagnostics)
                {
                    var success = false;
                    foreach (Diagnostic expectedDiagnostic in expectedDiagnostics)
                    {
                        if (DiagnosticComparer.Id.Equals(diagnostic, expectedDiagnostic))
                        {
                            success = true;
                            break;
                        }
                    }

                    if (success)
                        yield return diagnostic;
                }
            }
        }

        /// <summary>
        /// Updates compilation that will be used during verification.
        /// </summary>
        /// <param name="compilation"></param>
        protected virtual Compilation UpdateCompilation(Compilation compilation)
        {
            if (!Descriptor.IsEnabledByDefault)
                compilation = compilation.EnsureEnabled(Descriptor);

            return compilation;
        }

        /// <summary>
        /// Verifies that specified source will not produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source">Source text that contains placeholder [||] to be replaced with <paramref name="sourceData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoDiagnosticAsync(
            string source,
            string sourceData,
            CodeVerificationOptions options = null,
            CancellationToken cancellationToken = default)
        {
            (_, string text) = TextParser.ReplaceEmptySpan(source, sourceData);

            await VerifyNoDiagnosticAsync(
                source: text,
                additionalSources: null,
                options: options,
                cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will not produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens [| and |] represents start and end of selection respectively.</param>
        /// <param name="additionalSources"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoDiagnosticAsync(
            string source,
            IEnumerable<string> additionalSources = null,
            CodeVerificationOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            if (SupportedDiagnostics.IndexOf(Descriptor, DiagnosticDescriptorComparer.Id) == -1)
                  Assert.True(false, $"Diagnostic \"{Descriptor.Id}\" is not supported by analyzer(s) {string.Join(", ", Analyzers.Select(f => f.GetType().Name))}.");

            using (Workspace workspace = new AdhocWorkspace())
            {
                Document document = WorkspaceFactory.CreateDocument(workspace.CurrentSolution, source, additionalSources, options);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                compilation = UpdateCompilation(compilation);

                ImmutableArray<Diagnostic> analyzerDiagnostics = await compilation.GetAnalyzerDiagnosticsAsync(Analyzers, DiagnosticComparer.SpanStart, cancellationToken);

                foreach (Diagnostic diagnostic in analyzerDiagnostics)
                {
                    if (string.Equals(diagnostic.Id, Descriptor.Id, StringComparison.Ordinal))
                        Assert.True(false, $"No diagnostic expected{analyzerDiagnostics.Where(f => string.Equals(f.Id, Descriptor.Id, StringComparison.Ordinal)).ToDebugString()}");
                }
            }
        }

        private void VerifyDiagnostics(
            IEnumerable<Diagnostic> actualDiagnostics,
            IEnumerable<Diagnostic> expectedDiagnostics,
            CancellationToken cancellationToken = default)
        {
            VerifyDiagnostics(actualDiagnostics, expectedDiagnostics, checkAdditionalLocations: false, cancellationToken: cancellationToken);
        }

        private void VerifyDiagnostics(
            IEnumerable<Diagnostic> actualDiagnostics,
            IEnumerable<Diagnostic> expectedDiagnostics,
            bool checkAdditionalLocations,
            CancellationToken cancellationToken = default)
        {
            int expectedCount = 0;
            int actualCount = 0;

            using (IEnumerator<Diagnostic> expectedEnumerator = expectedDiagnostics.OrderBy(f => f, DiagnosticComparer.SpanStart).GetEnumerator())
            using (IEnumerator<Diagnostic> actualEnumerator = actualDiagnostics.OrderBy(f => f, DiagnosticComparer.SpanStart).GetEnumerator())
            {
                if (!expectedEnumerator.MoveNext())
                    Assert.True(false, "Diagnostic's location not found in a source text.");

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    expectedCount++;

                    Diagnostic expectedDiagnostic = expectedEnumerator.Current;

                    if (SupportedDiagnostics.IndexOf(expectedDiagnostic.Descriptor, DiagnosticDescriptorComparer.Id) == -1)
                        Assert.True(false, $"Diagnostic \"{expectedDiagnostic.Id}\" is not supported by analyzer(s) {string.Join(", ", Analyzers.Select(f => f.GetType().Name))}.");

                    if (actualEnumerator.MoveNext())
                    {
                        actualCount++;

                        VerifyDiagnostic(actualEnumerator.Current, expectedDiagnostic, checkAdditionalLocations: checkAdditionalLocations);
                    }
                    else
                    {
                        while (expectedEnumerator.MoveNext())
                            expectedCount++;

                        ReportMismatch(actualDiagnostics, actualCount, expectedCount);
                    }

                } while (expectedEnumerator.MoveNext());

                if (actualEnumerator.MoveNext())
                {
                    actualCount++;

                    while (actualEnumerator.MoveNext())
                        actualCount++;

                    ReportMismatch(actualDiagnostics, actualCount, expectedCount);
                }
            }

            void ReportMismatch(IEnumerable<Diagnostic> actualDiagnostics, int actualCount, int expectedCount)
            {
                if (actualCount == 0)
                {
                    Assert.True(false, $"No diagnostic found, expected: {expectedCount}.");
                }
                else
                {
                    Assert.True(false, $"Mismatch between number of diagnostics, expected: {expectedCount} actual: {actualCount}{actualDiagnostics.ToDebugString()}");
                }
            }
        }

        private void VerifyDiagnostic(
            Diagnostic actualDiagnostic,
            Diagnostic expectedDiagnostic,
            bool checkAdditionalLocations = false)
        {
            if (actualDiagnostic.Id != expectedDiagnostic.Id)
                Assert.True(false, $"Diagnostic id expected to be \"{expectedDiagnostic.Id}\", actual: \"{actualDiagnostic.Id}\"{GetMessage()}");

            VerifyLocation(actualDiagnostic.Location, expectedDiagnostic.Location, actualDiagnostic.Properties);

            if (checkAdditionalLocations)
                VerifyAdditionalLocations(actualDiagnostic.AdditionalLocations, expectedDiagnostic.AdditionalLocations);

            void VerifyLocation(
                Location actualLocation,
                Location expectedLocation,
                 ImmutableDictionary<string, string> properties = default)
            {
                VerifyFileLinePositionSpan(actualLocation.GetLineSpan(), expectedLocation.GetLineSpan(), properties);
            }

            void VerifyAdditionalLocations(
                IReadOnlyList<Location> actual,
                IReadOnlyList<Location> expected)
            {
                int actualCount = actual.Count;
                int expectedCount = expected.Count;

                if (actualCount != expectedCount)
                    Assert.True(false, $"{expectedCount} additional location(s) expected, actual: {actualCount}{GetMessage()}");

                for (int j = 0; j < actualCount; j++)
                    VerifyLocation(actual[j], expected[j]);
            }

            void VerifyFileLinePositionSpan(
                FileLinePositionSpan actual,
                FileLinePositionSpan expected,
                 ImmutableDictionary<string, string> properties = default)
            {
                if (actual.Path != expected.Path)
                    Assert.True(false, $"Diagnostic expected to be in file \"{expected.Path}\", actual: \"{actual.Path}\"{GetMessage()}");

                VerifyLinePosition(actual.StartLinePosition, expected.StartLinePosition, "start", properties);

                VerifyLinePosition(actual.EndLinePosition, expected.EndLinePosition, "end", properties);
            }

            void VerifyLinePosition(
                LinePosition actual,
                LinePosition expected,
                string startOrEnd,
                ImmutableDictionary<string, string> properties=default
                )
            {
                int actualLine = actual.Line;
                int expectedLine = expected.Line;

                if (actualLine != expectedLine)
                    Assert.True(false, $"Diagnostic expected to {startOrEnd} on line {expectedLine + 1}, actual: {actualLine + 1}{GetMessage()}");

                if (properties.TryGetValue("MethodName", out string methodName1)
                                 && methodName1 == "IdenitifierKeyValue")
                    return;

                int actualCharacter = actual.Character;
                int expectedCharacter = expected.Character;

                if (actualCharacter != expectedCharacter)
                    Assert.True(false, $"Diagnostic expected to {startOrEnd} at column {expectedCharacter + 1}, actual: {actualCharacter + 1}{GetMessage()}");
            }

            string GetMessage()
            {
                return $"\r\n\r\nExpected diagnostic:\r\n{expectedDiagnostic}\r\n\r\nActual diagnostic:\r\n{actualDiagnostic}\r\n";
            }
        }

        private protected Diagnostic CreateDiagnostic(string source, TextSpan span)
        {
            LinePositionSpan lineSpan = span.ToLinePositionSpan(source);

            return CreateDiagnostic(span, lineSpan);
        }

        private protected Diagnostic CreateDiagnostic(TextSpan span, LinePositionSpan lineSpan)
        {
            Location location = Location.Create(WorkspaceFactory.DefaultDocumentName, span, lineSpan);

            return Diagnostic.Create(Descriptor, location);
        }
    }
}
