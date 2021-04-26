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
        /// <param name="data"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAsync(
            DiagnosticTestData data,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TAnalyzer analyzer = Activator.CreateInstance<TAnalyzer>();
            ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = analyzer.SupportedDiagnostics;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = CreateDocument(workspace.CurrentSolution, data.Source, data.AdditionalFiles, options, data.Descriptor);

                SyntaxTree tree = await document.GetSyntaxTreeAsync();

                ImmutableArray<Diagnostic> expectedDiagnostics = data.GetDiagnostics(tree);

                VerifySupportedDiagnostics(analyzer, expectedDiagnostics);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                ImmutableArray<Diagnostic> diagnostics = await GetAnalyzerDiagnosticsAsync(compilation, analyzer, DiagnosticComparer.SpanStart, cancellationToken);

                if (diagnostics.Length > 0
                    && supportedDiagnostics.Length > 1)
                {
                    VerifyDiagnostics(data, analyzer, expectedDiagnostics, FilterDiagnostics(diagnostics, expectedDiagnostics), cancellationToken);
                }
                else
                {
                    VerifyDiagnostics(data, analyzer, expectedDiagnostics, diagnostics, cancellationToken);
                }

                if (expectedDocuments.Any())
                    await VerifyAdditionalDocumentsAsync(document.Project, expectedDocuments, cancellationToken);
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
        /// <param name="data"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoDiagnosticAsync(
            DiagnosticTestData data,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TAnalyzer analyzer = Activator.CreateInstance<TAnalyzer>();

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> _) = CreateDocument(workspace.CurrentSolution, data.Source, data.AdditionalFiles, options, data.Descriptor);

                SyntaxTree tree = await document.GetSyntaxTreeAsync();

                ImmutableArray<Diagnostic> expectedDiagnostics = data.GetDiagnostics(tree);

                VerifySupportedDiagnostics(analyzer, expectedDiagnostics);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                ImmutableArray<Diagnostic> actualDiagnostics = await GetAnalyzerDiagnosticsAsync(compilation, analyzer, DiagnosticComparer.SpanStart, cancellationToken);

                actualDiagnostics = actualDiagnostics
                    .Where(diagnostic => string.Equals(diagnostic.Id, data.Descriptor.Id))
                    .ToImmutableArray();

                if (!actualDiagnostics.IsEmpty)
                    Fail("No diagnostic expected.", actualDiagnostics);
            }
        }

        /// <summary>
        /// Verifies that specified source will produce specified diagnostic and that the diagnostic will be fixed.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="expected"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAndFixAsync(
            DiagnosticTestData data,
            ExpectedTestState expected,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            await VerifyDiagnosticAsync(data, options, cancellationToken);
            await VerifyFixAsync(data, expected, options, cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce specified diagnostic and that the diagnostic will not be fixed.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAndNoFixAsync(
            DiagnosticTestData data,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            await VerifyDiagnosticAsync(data, options, cancellationToken);
            await VerifyNoFixAsync(data, options, cancellationToken);
        }

        private async Task VerifyFixAsync(
            DiagnosticTestData data,
            ExpectedTestState expected,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (expected == null)
                throw new ArgumentNullException(nameof(expected));

            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TAnalyzer analyzer = Activator.CreateInstance<TAnalyzer>();
            TFixProvider fixProvider = Activator.CreateInstance<TFixProvider>();

            ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = analyzer.SupportedDiagnostics;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = CreateDocument(workspace.CurrentSolution, data.Source, data.AdditionalFiles, options, data.Descriptor);

                Project project = document.Project;

                SyntaxTree tree = await document.GetSyntaxTreeAsync();

                ImmutableArray<Diagnostic> expectedDiagnostics = data.GetDiagnostics(tree);

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
                    {
                        if (!fixRegistered)
                            Fail("No diagnostic found.");

                        break;
                    }

                    if (DiagnosticDeepEqualityComparer.Equals(diagnostics, previousDiagnostics))
                        Fail("Same diagnostics returned before and after the fix was applied.", diagnostics);

                    if (DiagnosticDeepEqualityComparer.Equals(diagnostics, previousPreviousDiagnostics))
                        Fail("Infinite loop detected: Reported diagnostics have been previously fixed.", diagnostics);

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
                    {
                        if (!fixRegistered)
                            Fail($"No diagnostic with ID '{data.Descriptor.Id}' found.", diagnostics);

                        break;
                    }

                    CodeAction action = null;
                    List<CodeAction> candidateActions = null;

                    var context = new CodeFixContext(
                        document,
                        diagnostic,
                        (a, d) =>
                        {
                            if ((data.EquivalenceKey == null
                                || string.Equals(data.EquivalenceKey, a.EquivalenceKey, StringComparison.Ordinal))
                                && d.Contains(diagnostic))
                            {
                                if (action != null)
                                    Fail($"Multiple fixes registered by '{fixProvider.GetType().Name}'.", new CodeAction[] { action, a });

                                action = a;
                            }
                            else
                            {
                                (candidateActions ??= new List<CodeAction>()).Add(a);
                            }
                        },
                        cancellationToken);

                    await fixProvider.RegisterCodeFixesAsync(context);

                    if (action == null)
                        Fail("No code fix has been registered.", candidateActions);

                    fixRegistered = true;

                    document = await VerifyAndApplyCodeActionAsync(document, action, expected.CodeActionTitle);
                    compilation = await document.Project.GetCompilationAsync(cancellationToken);

                    ImmutableArray<Diagnostic> newCompilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                    VerifyCompilerDiagnostics(newCompilerDiagnostics, options);
                    VerifyNoNewCompilerDiagnostics(compilerDiagnostics, newCompilerDiagnostics, options);

                    previousPreviousDiagnostics = previousDiagnostics;
                    previousDiagnostics = diagnostics;
                }

                await VerifyExpectedDocument(expected, document, cancellationToken);

                if (expectedDocuments.Any())
                    await VerifyAdditionalDocumentsAsync(document.Project, expectedDocuments, cancellationToken);
            }
        }

        private async Task VerifyNoFixAsync(
            DiagnosticTestData data,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TAnalyzer analyzer = Activator.CreateInstance<TAnalyzer>();
            TFixProvider fixProvider = Activator.CreateInstance<TFixProvider>();

            ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = analyzer.SupportedDiagnostics;
            ImmutableArray<string> fixableDiagnosticIds = fixProvider.FixableDiagnosticIds;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> _) = CreateDocument(workspace.CurrentSolution, data.Source, data.AdditionalFiles, options, data.Descriptor);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                SyntaxTree tree = await document.GetSyntaxTreeAsync();

                ImmutableArray<Diagnostic> expectedDiagnostics = data.GetDiagnostics(tree);

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
                                if ((data.EquivalenceKey == null
                                    || string.Equals(a.EquivalenceKey, data.EquivalenceKey, StringComparison.Ordinal))
                                    && d.Contains(diagnostic))
                                {
                                    Fail("No code fix expected.");
                                }
                            },
                            cancellationToken);

                        await fixProvider.RegisterCodeFixesAsync(context);
                    }
                }
            }
        }

        private void VerifyDiagnostics(
            DiagnosticTestData data,
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
                    Fail("Diagnostic's location not found in a source text.");

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
                            data.DiagnosticMessage,
                            data.FormatProvider,
                            verifyAdditionalLocations: data.AlwaysVerifyAdditionalLocations || !data.AdditionalSpans.IsEmpty);
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
                    Fail($"No diagnostic found, expected: {expectedCount}.");
                }
                else
                {
                    Fail($"Mismatch between number of diagnostics, expected: {expectedCount} actual: {actualCount}.", actualDiagnostics);
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
                Fail($"Diagnostic's ID expected to be \"{expectedDiagnostic.Id}\", actual: \"{actualDiagnostic.Id}\"{GetMessage()}");

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
                    Fail($"{expectedCount} additional location(s) expected, actual: {actualCount}{GetMessage()}");

                for (int j = 0; j < actualCount; j++)
                    VerifyLocation(expected[j], actual[j]);
            }

            void VerifyFileLinePositionSpan(
                FileLinePositionSpan expected,
                FileLinePositionSpan actual)
            {
                if (expected.Path != actual.Path)
                    Fail($"Diagnostic expected to be in file \"{expected.Path}\", actual: \"{actual.Path}\"{GetMessage()}");

                string message = VerifyLinePositionSpan(expected.Span, actual.Span);

                if (message != null)
                    Fail($"Diagnostic{message}{GetMessage()}");
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
                Fail($"An exception occurred in analyzer '{analyzer.GetType()}'.{Environment.NewLine}{exception}");

            return (comparer != null)
                ? diagnostics.Sort(comparer)
                : diagnostics;
        }
    }
}
