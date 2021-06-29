// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Roslynator.CodeFixes;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal class FixCommand : MSBuildWorkspaceCommand<FixCommandResult>
    {
        public FixCommand(
            FixCommandLineOptions options,
            DiagnosticSeverity severityLevel,
            IEnumerable<KeyValuePair<string, string>> diagnosticFixMap,
            IEnumerable<KeyValuePair<string, string>> diagnosticFixerMap,
            FixAllScope fixAllScope,
            in ProjectFilter projectFilter) : base(projectFilter)
        {
            Options = options;
            SeverityLevel = severityLevel;
            DiagnosticFixMap = diagnosticFixMap;
            DiagnosticFixerMap = diagnosticFixerMap;
            FixAllScope = fixAllScope;
        }

        public FixCommandLineOptions Options { get; }

        public DiagnosticSeverity SeverityLevel { get; }

        public IEnumerable<KeyValuePair<string, string>> DiagnosticFixMap { get; }

        public IEnumerable<KeyValuePair<string, string>> DiagnosticFixerMap { get; }

        public FixAllScope FixAllScope { get; }

        public override async Task<FixCommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            AssemblyResolver.Register();

            var codeFixerOptions = new CodeFixerOptions(
                severityLevel: SeverityLevel,
                ignoreCompilerErrors: Options.IgnoreCompilerErrors,
                ignoreAnalyzerReferences: Options.IgnoreAnalyzerReferences,
                supportedDiagnosticIds: Options.SupportedDiagnostics,
                ignoredDiagnosticIds: Options.IgnoredDiagnostics,
                ignoredCompilerDiagnosticIds: Options.IgnoredCompilerDiagnostics,
                diagnosticIdsFixableOneByOne: Options.DiagnosticsFixableOneByOne,
                diagnosticFixMap: DiagnosticFixMap,
                diagnosticFixerMap: DiagnosticFixerMap,
                fixAllScope: FixAllScope,
                fileBanner: Options.FileBanner,
                maxIterations: Options.MaxIterations,
                batchSize: Options.BatchSize,
                format: Options.Format);

            IEnumerable<AnalyzerAssembly> analyzerAssemblies = Options.AnalyzerAssemblies
                .SelectMany(path => AnalyzerAssemblyLoader.LoadFrom(path).Select(info => info.AnalyzerAssembly));

            CultureInfo culture = (Options.Culture != null) ? CultureInfo.GetCultureInfo(Options.Culture) : null;

            var projectFilter = new ProjectFilter(Options.Projects, Options.IgnoredProjects, Language);

            return await FixAsync(projectOrSolution, analyzerAssemblies, codeFixerOptions, projectFilter, culture, cancellationToken);
        }

        private static async Task<FixCommandResult> FixAsync(
            ProjectOrSolution projectOrSolution,
            IEnumerable<AnalyzerAssembly> analyzerAssemblies,
            CodeFixerOptions codeFixerOptions,
            ProjectFilter projectFilter,
            IFormatProvider formatProvider = null,
            CancellationToken cancellationToken = default)
        {
            foreach (string id in codeFixerOptions.IgnoredCompilerDiagnosticIds.OrderBy(f => f))
                WriteLine($"Ignore compiler diagnostic '{id}'", Verbosity.Diagnostic);

            foreach (string id in codeFixerOptions.IgnoredDiagnosticIds.OrderBy(f => f))
                WriteLine($"Ignore diagnostic '{id}'", Verbosity.Diagnostic);

            ImmutableArray<ProjectFixResult> results;

            if (projectOrSolution.IsProject)
            {
                Project project = projectOrSolution.AsProject();

                Solution solution = project.Solution;

                CodeFixer codeFixer = GetCodeFixer(solution);

                WriteLine($"Fix '{project.Name}'", ConsoleColor.Cyan, Verbosity.Minimal);

                Stopwatch stopwatch = Stopwatch.StartNew();

                ProjectFixResult result = await codeFixer.FixProjectAsync(project, cancellationToken);

                stopwatch.Stop();

                WriteLine($"Done fixing project '{project.FilePath}' in {stopwatch.Elapsed:mm\\:ss\\.ff}", Verbosity.Minimal);

                results = ImmutableArray.Create(result);
            }
            else
            {
                Solution solution = projectOrSolution.AsSolution();

                CodeFixer codeFixer = GetCodeFixer(solution);

                results = await codeFixer.FixSolutionAsync(f => projectFilter.IsMatch(f), cancellationToken);
            }

            LogHelpers.WriteProjectFixResults(results, codeFixerOptions, formatProvider);

            return new FixCommandResult(
                (results.Any(f => f.FixedDiagnostics.Length > 0)) ? CommandStatus.Success : CommandStatus.NotSuccess,
                SimpleFixResult.Create(results));

            CodeFixer GetCodeFixer(Solution solution)
            {
                return new CodeFixer(
                    solution,
                    analyzerAssemblies: analyzerAssemblies,
                    formatProvider: formatProvider,
                    options: codeFixerOptions);
            }
        }

        protected override void ProcessResults(IEnumerable<FixCommandResult> results)
        {
            WriteFixResults(results.Select(f => f.FixResult));
        }

        private static void WriteFixResults(
            IEnumerable<SimpleFixResult> results,
            IFormatProvider formatProvider = null)
        {
            int numberOfAddedFileBanners = results.Sum(f => f.NumberOfAddedFileBanners);
            if (numberOfAddedFileBanners >= 0)
            {
                WriteLine(Verbosity.Normal);
                WriteLine($"{numberOfAddedFileBanners} file {((numberOfAddedFileBanners == 1) ? "banner" : "banners")} added", Verbosity.Normal);
            }

            int numberOfFormattedDocuments = results.Sum(f => f.NumberOfFormattedDocuments);
            if (numberOfFormattedDocuments >= 0)
            {
                WriteLine(Verbosity.Normal);
                WriteLine($"{numberOfFormattedDocuments} {((numberOfFormattedDocuments == 1) ? "document" : "documents")} formatted", Verbosity.Normal);
            }

            WriteDiagnostics(results.SelectMany(f => f.UnfixableDiagnostics), "Unfixable diagnostics:");
            WriteDiagnostics(results.SelectMany(f => f.UnfixedDiagnostics), "Unfixed diagnostics:");
            WriteDiagnostics(results.SelectMany(f => f.FixedDiagnostics), "Fixed diagnostics:");

            int fixedCount = results.Sum(f => f.FixedDiagnostics.Sum(f => f.Value));

            WriteLine(Verbosity.Minimal);
            WriteLine($"{fixedCount} {((fixedCount == 1) ? "diagnostic" : "diagnostics")} fixed", ConsoleColor.Green, Verbosity.Minimal);

            void WriteDiagnostics(
                IEnumerable<KeyValuePair<DiagnosticDescriptor, int>> diagnostics,
                string title)
            {
                List<(DiagnosticDescriptor descriptor, int count)> diagnosticsById = diagnostics
                    .GroupBy(f => f.Key, DiagnosticDescriptorComparer.Id)
                    .Select(f => (descriptor: f.Key, count: f.Sum(f => f.Value)))
                    .OrderByDescending(f => f.count)
                    .ThenBy(f => f.descriptor.Id)
                    .ToList();

                if (diagnosticsById.Count > 0)
                {
                    WriteLine(Verbosity.Normal);
                    WriteLine(title, Verbosity.Normal);

                    int maxIdLength = diagnosticsById.Max(f => f.descriptor.Id.Length);
                    int maxCountLength = diagnosticsById.Max(f => f.count.ToString("n0").Length);

                    foreach ((DiagnosticDescriptor descriptor, int count) in diagnosticsById)
                    {
                        WriteLine($"  {count.ToString("n0").PadLeft(maxCountLength)} {descriptor.Id.PadRight(maxIdLength)} {descriptor.Title.ToString(formatProvider)}", Verbosity.Normal);
                    }
                }
            }
        }

        protected override void OperationCanceled(OperationCanceledException ex)
        {
            WriteLine("Fixing was canceled.", Verbosity.Quiet);
        }
    }
}
