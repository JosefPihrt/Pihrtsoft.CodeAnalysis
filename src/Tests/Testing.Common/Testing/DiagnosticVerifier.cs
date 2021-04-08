// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents verifier for a diagnostic that is produced by <see cref="DiagnosticAnalyzer"/>.
    /// </summary>
    public abstract class DiagnosticVerifier<TAnalyzer, TFixProvider> : CodeVerifier
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TFixProvider : CodeFixProvider, new()
    {
        internal DiagnosticVerifier(IAssert assert) : base(assert)
        {
        }

        /// <summary>
        /// Verifies that specified source will produce specified diagnostic(s).
        /// </summary>
        /// <param name="state"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAsync(
            DiagnosticTestState state,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TAnalyzer analyzer = Activator.CreateInstance<TAnalyzer>();
            ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = analyzer.SupportedDiagnostics;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = CreateDocument(workspace.CurrentSolution, state.Source, state.AdditionalFiles, options, state.Descriptor);

                SyntaxTree tree = await document.GetSyntaxTreeAsync();

                ImmutableArray<Diagnostic> expectedDiagnostics = state.GetDiagnostics(tree);

                VerifySupportedDiagnostics(analyzer, expectedDiagnostics);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                ImmutableArray<Diagnostic> diagnostics = await GetAnalyzerDiagnosticsAsync(compilation, analyzer, DiagnosticComparer.SpanStart, cancellationToken);

                if (diagnostics.Length > 0
                    && supportedDiagnostics.Length > 1)
                {
                    VerifyDiagnostics(state, analyzer, expectedDiagnostics, FilterDiagnostics(diagnostics, expectedDiagnostics), cancellationToken);
                }
                else
                {
                    VerifyDiagnostics(state, analyzer, expectedDiagnostics, diagnostics, cancellationToken);
                }
            }

            static IEnumerable<Diagnostic> FilterDiagnostics(
                ImmutableArray<Diagnostic> diagnostics,
                ImmutableArray<Diagnostic> expectedDiagnostics)
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
        /// Verifies that specified source will not produce specified diagnostic.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoDiagnosticAsync(
            DiagnosticTestState state,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TAnalyzer analyzer = Activator.CreateInstance<TAnalyzer>();

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = CreateDocument(workspace.CurrentSolution, state.Source, state.AdditionalFiles, options, state.Descriptor);

                SyntaxTree tree = await document.GetSyntaxTreeAsync();

                ImmutableArray<Diagnostic> expectedDiagnostics = state.GetDiagnostics(tree);

                VerifySupportedDiagnostics(analyzer, expectedDiagnostics);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                ImmutableArray<Diagnostic> actualDiagnostics = await GetAnalyzerDiagnosticsAsync(compilation, analyzer, DiagnosticComparer.SpanStart, cancellationToken);

                actualDiagnostics = actualDiagnostics
                    .Where(diagnostic => string.Equals(diagnostic.Id, state.Descriptor.Id))
                    .ToImmutableArray();

                if (!actualDiagnostics.IsEmpty)
                    Assert.True(false, $"No diagnostic expected{actualDiagnostics.ToDebugString()}");
            }
        }

        /// <summary>
        /// Verifies that specified source will produce specified diagnostic and that the diagnostic will be fixed.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="expected"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAndFixAsync(
            DiagnosticTestState state,
            ExpectedTestState expected,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            await VerifyDiagnosticAsync(state, options, cancellationToken);
            await VerifyFixAsync(state, expected, options, cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce specified diagnostic and that the diagnostic will not be fixed.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAndNoFixAsync(
            DiagnosticTestState state,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            await VerifyDiagnosticAsync(state, options, cancellationToken);
            await VerifyNoFixAsync(state, options, cancellationToken);
        }

        private async Task VerifyFixAsync(
            DiagnosticTestState state,
            ExpectedTestState expected,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TAnalyzer analyzer = Activator.CreateInstance<TAnalyzer>();
            TFixProvider fixProvider = Activator.CreateInstance<TFixProvider>();

            ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = analyzer.SupportedDiagnostics;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = CreateDocument(workspace.CurrentSolution, state.Source, state.AdditionalFiles, options, state.Descriptor);

                Project project = document.Project;

                SyntaxTree tree = await document.GetSyntaxTreeAsync();

                ImmutableArray<Diagnostic> expectedDiagnostics = state.GetDiagnostics(tree);

                foreach (Diagnostic diagnostic in expectedDiagnostics)
                    VerifyFixableDiagnostics(fixProvider, diagnostic.Id);

                VerifySupportedDiagnostics(analyzer, expectedDiagnostics);

                Compilation compilation = await project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                ImmutableArray<Diagnostic> previousDiagnostics = ImmutableArray<Diagnostic>.Empty;

                ImmutableArray<Diagnostic> previousPreviousDiagnostics = ImmutableArray<Diagnostic>.Empty;

                var fixRegistered = false;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    ImmutableArray<Diagnostic> diagnostics = await GetAnalyzerDiagnosticsAsync(compilation, analyzer, DiagnosticComparer.SpanStart, cancellationToken);

                    int length = diagnostics.Length;

                    if (length == 0)
                        break;

                    if (DiagnosticDeepEqualityComparer.Equals(diagnostics, previousDiagnostics))
                        Assert.True(false, "Same diagnostics returned before and after the fix was applied." + diagnostics.ToDebugString());

                    if (DiagnosticDeepEqualityComparer.Equals(diagnostics, previousPreviousDiagnostics))
                        Assert.True(false, "Infinite loop detected: Reported diagnostics have been previously fixed." + diagnostics.ToDebugString());

                    Diagnostic diagnostic = FindDiagnosticToFix(diagnostics, expectedDiagnostics);

                    static Diagnostic FindDiagnosticToFix(
                        ImmutableArray<Diagnostic> diagnostics,
                        ImmutableArray<Diagnostic> expectedDiagnostics)
                    {
                        foreach (Diagnostic diagnostic in diagnostics)
                        {
                            foreach (Diagnostic diagnostic2 in expectedDiagnostics)
                            {
                                if (diagnostic.Id == diagnostic2.Id)
                                    return diagnostic;
                            }
                        }

                        return null;
                    }

                    if (diagnostic == null)
                        break;

                    CodeAction action = null;

                    var context = new CodeFixContext(
                        document,
                        diagnostic,
                        (a, d) =>
                        {
                            if ((state.EquivalenceKey == null
                                || string.Equals(state.EquivalenceKey, a.EquivalenceKey, StringComparison.Ordinal))
                                && d.Contains(diagnostic))
                            {
                                if (action != null)
                                    Assert.True(false, "Multiple fixes available.");

                                action = a;
                            }
                        },
                        cancellationToken);

                    await fixProvider.RegisterCodeFixesAsync(context);

                    if (action == null)
                        break;

                    fixRegistered = true;

                    document = await VerifyAndApplyCodeActionAsync(document, action, expected.CodeActionTitle);
                    compilation = await document.Project.GetCompilationAsync(cancellationToken);

                    ImmutableArray<Diagnostic> newCompilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                    VerifyCompilerDiagnostics(newCompilerDiagnostics, options);
                    VerifyNoNewCompilerDiagnostics(compilerDiagnostics, newCompilerDiagnostics, options);

                    previousPreviousDiagnostics = previousDiagnostics;
                    previousDiagnostics = diagnostics;
                }

                Assert.True(fixRegistered, "No code fix has been registered.");

                await VerifyExpectedDocument(expected, document, cancellationToken);

                if (expectedDocuments.Any())
                    await VerifyAdditionalDocumentsAsync(document.Project, expectedDocuments, cancellationToken);
            }
        }

        private async Task VerifyNoFixAsync(
            DiagnosticTestState state,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TAnalyzer analyzer = Activator.CreateInstance<TAnalyzer>();
            TFixProvider fixProvider = Activator.CreateInstance<TFixProvider>();

            ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = analyzer.SupportedDiagnostics;
            ImmutableArray<string> fixableDiagnosticIds = fixProvider.FixableDiagnosticIds;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = CreateDocument(workspace.CurrentSolution, state.Source, state.AdditionalFiles, options, state.Descriptor);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                SyntaxTree tree = await document.GetSyntaxTreeAsync();

                ImmutableArray<Diagnostic> expectedDiagnostics = state.GetDiagnostics(tree);

                VerifySupportedDiagnostics(analyzer, expectedDiagnostics);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                ImmutableArray<Diagnostic> diagnostics = await GetAnalyzerDiagnosticsAsync(compilation, analyzer, DiagnosticComparer.SpanStart, cancellationToken);

                foreach (Diagnostic diagnostic in diagnostics)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (expectedDiagnostics.IndexOf(diagnostic, DiagnosticComparer.Id) >= 0
                        && fixableDiagnosticIds.Contains(diagnostic.Id))
                    {
                        var context = new CodeFixContext(
                            document,
                            diagnostic,
                            (a, d) =>
                            {
                                if ((state.EquivalenceKey == null
                                    || string.Equals(a.EquivalenceKey, state.EquivalenceKey, StringComparison.Ordinal))
                                    && d.Contains(diagnostic))
                                {
                                    Assert.True(false, "No code fix expected.");
                                }
                            },
                            cancellationToken);

                        await fixProvider.RegisterCodeFixesAsync(context);
                    }
                }
            }
        }

        private void VerifyDiagnostics(
            DiagnosticTestState state,
            TAnalyzer analyzer,
            IEnumerable<Diagnostic> expectedDiagnostics,
            IEnumerable<Diagnostic> actualDiagnostics,
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

                    VerifySupportedDiagnostics(analyzer, expectedDiagnostic);

                    if (actualEnumerator.MoveNext())
                    {
                        actualCount++;

                        VerifyDiagnostic(
                            expectedDiagnostic,
                            actualEnumerator.Current,
                            state.DiagnosticMessage,
                            state.FormatProvider,
                            verifyAdditionalLocations: state.AlwaysVerifyAdditionalLocations || !state.AdditionalSpans.IsEmpty);
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
            Diagnostic expectedDiagnostic,
            Diagnostic actualDiagnostic,
            string message,
            IFormatProvider formatProvider,
            bool verifyAdditionalLocations = false)
        {
            if (expectedDiagnostic.Id != actualDiagnostic.Id)
                Assert.True(false, $"Diagnostic's ID expected to be \"{expectedDiagnostic.Id}\", actual: \"{actualDiagnostic.Id}\"{GetMessage()}");

            VerifyLocation(expectedDiagnostic.Location, actualDiagnostic.Location);

            if (verifyAdditionalLocations)
                VerifyAdditionalLocations(expectedDiagnostic.AdditionalLocations, actualDiagnostic.AdditionalLocations);

            if (message != null)
                Assert.Equal(message, actualDiagnostic.GetMessage(formatProvider));

            void VerifyLocation(
                Location expectedLocation,
                Location actualLocation)
            {
                VerifyFileLinePositionSpan(expectedLocation.GetLineSpan(), actualLocation.GetLineSpan());
            }

            void VerifyAdditionalLocations(
                IReadOnlyList<Location> expected,
                IReadOnlyList<Location> actual)
            {
                int expectedCount = expected.Count;
                int actualCount = actual.Count;

                if (expectedCount != actualCount)
                    Assert.True(false, $"{expectedCount} additional location(s) expected, actual: {actualCount}{GetMessage()}");

                for (int j = 0; j < actualCount; j++)
                    VerifyLocation(expected[j], actual[j]);
            }

            void VerifyFileLinePositionSpan(
                FileLinePositionSpan expected,
                FileLinePositionSpan actual)
            {
                if (expected.Path != actual.Path)
                    Assert.True(false, $"Diagnostic expected to be in file \"{expected.Path}\", actual: \"{actual.Path}\"{GetMessage()}");

                string message = VerifyLinePositionSpan(expected.Span, actual.Span);

                if (message != null)
                    Assert.True(false, $"Diagnostic{message}{GetMessage()}");
            }

            string GetMessage()
            {
                return $"\r\n\r\nExpected diagnostic:\r\n{expectedDiagnostic}\r\n\r\nActual diagnostic:\r\n{actualDiagnostic}\r\n";
            }
        }

        private Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(
            Compilation compilation,
            DiagnosticAnalyzer analyzer,
            IComparer<Diagnostic> comparer = null,
            CancellationToken cancellationToken = default)
        {
            return GetAnalyzerDiagnosticsAsync(
                compilation,
                ImmutableArray.Create(analyzer),
                comparer,
                cancellationToken);
        }

        private async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(
            Compilation compilation,
            ImmutableArray<DiagnosticAnalyzer> analyzers,
            IComparer<Diagnostic> comparer = null,
            CancellationToken cancellationToken = default)
        {
            Exception exception = null;
            DiagnosticAnalyzer analyzer = null;

            var options = new CompilationWithAnalyzersOptions(
                options: null,
                onAnalyzerException: (e, a, _) =>
                {
                    exception = e;
                    analyzer = a;
                },
                concurrentAnalysis: true,
                logAnalyzerExecutionTime: false,
                reportSuppressedDiagnostics: false,
                analyzerExceptionFilter: null);

            CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(analyzers, options);

            ImmutableArray<Diagnostic> diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken);

            if (exception != null)
                Assert.True(false, $"An exception occurred in analyzer '{analyzer.GetType()}'.{Environment.NewLine}{exception}");

            return (comparer != null)
                ? diagnostics.Sort(comparer)
                : diagnostics;
        }
    }
}
