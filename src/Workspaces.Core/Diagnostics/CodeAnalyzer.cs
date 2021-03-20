// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Diagnostics.Telemetry;
using Roslynator.Host.Mef;
using static Roslynator.Logger;

namespace Roslynator.Diagnostics
{
    internal class CodeAnalyzer
    {
        private readonly AnalyzerAssemblyList _analyzerAssemblies = new AnalyzerAssemblyList();

        private readonly AnalyzerAssemblyList _analyzerReferences = new AnalyzerAssemblyList();

        internal static readonly TimeSpan MinimalExecutionTime = TimeSpan.FromMilliseconds(1);

        public CodeAnalyzer(
            IEnumerable<AnalyzerAssembly> analyzerAssemblies = null,
            IFormatProvider formatProvider = null,
            CodeAnalyzerOptions options = null)
        {
            if (analyzerAssemblies != null)
                _analyzerAssemblies.AddRange(analyzerAssemblies);

            FormatProvider = formatProvider;
            Options = options ?? CodeAnalyzerOptions.Default;
        }

        public IFormatProvider FormatProvider { get; }

        public CodeAnalyzerOptions Options { get; }

        public async Task<ImmutableArray<ProjectAnalysisResult>> AnalyzeSolutionAsync(
            Solution solution,
            Func<Project, bool> predicate,
            CancellationToken cancellationToken = default)
        {
            foreach (string id in Options.IgnoredDiagnosticIds.OrderBy(f => f))
                WriteLine($"Ignore diagnostic '{id}'", Verbosity.Diagnostic);

            ImmutableArray<ProjectId> projectIds = solution
                .GetProjectDependencyGraph()
                .GetTopologicallySortedProjects(cancellationToken)
                .ToImmutableArray();

            WriteLine($"Analyze solution '{solution.FilePath}'", ConsoleColor.Cyan, Verbosity.Minimal);

            var results = new List<ProjectAnalysisResult>();

            Stopwatch stopwatch = Stopwatch.StartNew();

            TimeSpan lastElapsed = TimeSpan.Zero;

            for (int i = 0; i < projectIds.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Project project = solution.GetProject(projectIds[i]);

                if (predicate == null || predicate(project))
                {
                    WriteLine($"Analyze '{project.Name}' {$"{i + 1}/{projectIds.Length}"}", Verbosity.Minimal);

                    ProjectAnalysisResult result = await AnalyzeProjectCoreAsync(project, cancellationToken).ConfigureAwait(false);

                    results.Add(result);
                }
                else
                {
                    WriteLine($"Skip '{project.Name}' {$"{i + 1}/{projectIds.Length}"}", ConsoleColor.DarkGray, Verbosity.Minimal);
                }

                lastElapsed = stopwatch.Elapsed;
            }

            stopwatch.Stop();

            WriteLine($"Done analyzing solution '{solution.FilePath}' in {stopwatch.Elapsed:mm\\:ss\\.ff}", Verbosity.Minimal);

            if (results.Count > 0)
                WriteProjectAnalysisResults(results, cancellationToken);

            return results.ToImmutableArray();
        }

        public async Task<ProjectAnalysisResult> AnalyzeProjectAsync(Project project, CancellationToken cancellationToken = default)
        {
            WriteLine($"Analyze '{project.Name}'", ConsoleColor.Cyan, Verbosity.Minimal);

            Stopwatch stopwatch = Stopwatch.StartNew();

            ProjectAnalysisResult result = await AnalyzeProjectCoreAsync(project, cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();

            WriteLine($"Done analyzing project '{project.FilePath}' in {stopwatch.Elapsed:mm\\:ss\\.ff}", Verbosity.Minimal);

            WriteProjectAnalysisResults(new ProjectAnalysisResult[] { result }, cancellationToken);

            return result;
        }

        private async Task<ProjectAnalysisResult> AnalyzeProjectCoreAsync(Project project, CancellationToken cancellationToken = default)
        {
            ImmutableArray<DiagnosticAnalyzer> analyzers = CodeAnalysisHelpers.GetAnalyzers(
                project: project,
                analyzerAssemblies: _analyzerAssemblies,
                analyzerReferences: _analyzerReferences,
                options: Options);

            if (!analyzers.Any())
                WriteLine($"  No analyzers found to analyze '{project.Name}'", ConsoleColor.DarkGray, Verbosity.Normal);

            if (analyzers.Any()
                || !Options.IgnoreCompilerDiagnostics)
            {
                return await AnalyzeProjectCoreAsync(project, analyzers, cancellationToken).ConfigureAwait(false);
            }

            return new ProjectAnalysisResult(project.Id);
        }

        private async Task<ProjectAnalysisResult> AnalyzeProjectCoreAsync(Project project, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken = default)
        {
            LogHelpers.WriteUsedAnalyzers(analyzers, null, Options, ConsoleColor.DarkGray, Verbosity.Diagnostic);

            cancellationToken.ThrowIfCancellationRequested();

            Compilation compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

            ImmutableArray<Diagnostic> compilerDiagnostics = (Options.IgnoreCompilerDiagnostics)
                ? ImmutableArray<Diagnostic>.Empty
                : compilation.GetDiagnostics(cancellationToken);

            compilerDiagnostics = FilterDiagnostics(compilerDiagnostics, cancellationToken).ToImmutableArray();

            ImmutableArray<Diagnostic> diagnostics = ImmutableArray<Diagnostic>.Empty;

            ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo> telemetry = ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo>.Empty;

            if (analyzers.Any())
            {
                var compilationWithAnalyzersOptions = new CompilationWithAnalyzersOptions(
                    options: default(AnalyzerOptions),
                    onAnalyzerException: default(Action<Exception, DiagnosticAnalyzer, Diagnostic>),
                    concurrentAnalysis: Options.ConcurrentAnalysis,
                    logAnalyzerExecutionTime: Options.LogAnalyzerExecutionTime,
                    reportSuppressedDiagnostics: Options.ReportSuppressedDiagnostics);

                var compilationWithAnalyzers = new CompilationWithAnalyzers(compilation, analyzers, compilationWithAnalyzersOptions);

                if (Options.LogAnalyzerExecutionTime)
                {
                    AnalysisResult analysisResult = await compilationWithAnalyzers.GetAnalysisResultAsync(cancellationToken).ConfigureAwait(false);

                    diagnostics = analysisResult.GetAllDiagnostics();
                    telemetry = analysisResult.AnalyzerTelemetryInfo;
                }
                else
                {
                    diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            string projectDirectoryPath = Path.GetDirectoryName(project.FilePath);

            LogHelpers.WriteDiagnostics(FilterDiagnostics(diagnostics.Where(f => f.IsAnalyzerExceptionDiagnostic()), cancellationToken).ToImmutableArray(), baseDirectoryPath: projectDirectoryPath, formatProvider: FormatProvider, indentation: "  ", verbosity: Verbosity.Detailed);
#if DEBUG
            if (ConsoleOut.Verbosity >= Verbosity.Detailed
                && diagnostics.Any(f => f.IsAnalyzerExceptionDiagnostic()))
            {
                Console.Write("Stop (Y/N)? ");

                if (char.ToUpperInvariant((char)Console.Read()) == 'Y')
                    throw new OperationCanceledException();
            }
#endif
            diagnostics = FilterDiagnostics(diagnostics.Where(f => !f.IsAnalyzerExceptionDiagnostic()), cancellationToken).ToImmutableArray();

            LogHelpers.WriteDiagnostics(compilerDiagnostics, baseDirectoryPath: projectDirectoryPath, formatProvider: FormatProvider, indentation: "  ", verbosity: Verbosity.Normal);

            LogHelpers.WriteDiagnostics(diagnostics, baseDirectoryPath: projectDirectoryPath, formatProvider: FormatProvider, indentation: "  ", verbosity: Verbosity.Normal);

            return new ProjectAnalysisResult(
                project.Id,
                analyzers,
                compilerDiagnostics,
                diagnostics,
                telemetry);
        }

        private IEnumerable<Diagnostic> FilterDiagnostics(IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken = default)
        {
            foreach (Diagnostic diagnostic in diagnostics)
            {
                if (Options.IsSupportedDiagnostic(diagnostic)
                    && (Options.ReportNotConfigurable || !diagnostic.Descriptor.CustomTags.Contains(WellKnownDiagnosticTags.NotConfigurable)))
                {
                    if (diagnostic.Descriptor.CustomTags.Contains(WellKnownDiagnosticTags.Compiler))
                    {
                        Debug.Assert(diagnostic.Id.StartsWith("CS", "VB", StringComparison.Ordinal), diagnostic.Id);

                        SyntaxTree tree = diagnostic.Location.SourceTree;

                        if (tree == null
                            || !GeneratedCodeUtility.IsGeneratedCode(tree, f => MefWorkspaceServices.Default.GetService<ISyntaxFactsService>(tree.Options.Language).IsComment(f), cancellationToken))
                        {
                            yield return diagnostic;
                        }
                    }
                    else
                    {
                        yield return diagnostic;
                    }
                }
            }
        }

        private void WriteProjectAnalysisResults(
            IList<ProjectAnalysisResult> results,
            CancellationToken cancellationToken)
        {
            if (Options.LogAnalyzerExecutionTime)
                WriteExecutionTime();

            int totalCount = 0;

            foreach (ProjectAnalysisResult result in results)
            {
                IEnumerable<Diagnostic> diagnostics = result.Diagnostics
                    .Where(f => !f.IsAnalyzerExceptionDiagnostic())
                    .Concat(result.CompilerDiagnostics);

                totalCount += FilterDiagnostics(diagnostics, cancellationToken).Count();
            }

            if (totalCount > 0)
            {
                WriteLine(Verbosity.Normal);

                Dictionary<DiagnosticDescriptor, int> diagnosticsByDescriptor = results
                    .SelectMany(f => FilterDiagnostics(f.Diagnostics.Concat(f.CompilerDiagnostics), cancellationToken))
                    .GroupBy(f => f.Descriptor, DiagnosticDescriptorComparer.Id)
                    .ToDictionary(f => f.Key, f => f.Count());

                int maxCountLength = Math.Max(totalCount.ToString().Length, diagnosticsByDescriptor.Max(f => f.Value.ToString().Length));
                int maxIdLength = diagnosticsByDescriptor.Max(f => f.Key.Id.Length);

                foreach (KeyValuePair<DiagnosticDescriptor, int> kvp in diagnosticsByDescriptor.OrderBy(f => f.Key.Id))
                {
                    WriteLine($"{kvp.Value.ToString().PadLeft(maxCountLength)} {kvp.Key.Id.PadRight(maxIdLength)} {kvp.Key.Title}", Verbosity.Normal);
                }
            }

            WriteLine(Verbosity.Minimal);
            WriteLine($"{totalCount} {((totalCount == 1) ? "diagnostic" : "diagnostics")} found", ConsoleColor.Green, Verbosity.Minimal);

            void WriteExecutionTime()
            {
                var telemetryInfos = new Dictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo>();

                foreach (ProjectAnalysisResult result in results)
                {
                    foreach (KeyValuePair<DiagnosticAnalyzer, AnalyzerTelemetryInfo> kvp in result.Telemetry)
                    {
                        DiagnosticAnalyzer analyzer = kvp.Key;

                        if (!telemetryInfos.TryGetValue(analyzer, out AnalyzerTelemetryInfo telemetryInfo))
                            telemetryInfo = new AnalyzerTelemetryInfo();

                        telemetryInfo.Add(kvp.Value);

                        telemetryInfos[analyzer] = telemetryInfo;
                    }
                }

                using (IEnumerator<KeyValuePair<DiagnosticAnalyzer, AnalyzerTelemetryInfo>> en = telemetryInfos
                    .Where(f => f.Value.ExecutionTime >= MinimalExecutionTime)
                    .OrderByDescending(f => f.Value.ExecutionTime)
                    .GetEnumerator())
                {
                    if (en.MoveNext())
                    {
                        WriteLine(Verbosity.Minimal);

                        do
                        {
                            WriteLine($"{en.Current.Value.ExecutionTime:mm\\:ss\\.fff} '{en.Current.Key.GetType().FullName}'", Verbosity.Minimal);

                        } while (en.MoveNext());
                    }
                }
            }
        }
    }
}
