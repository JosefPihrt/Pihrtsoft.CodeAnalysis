// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CommandLine.Xml;
using Roslynator.Diagnostics;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal class AnalyzeCommand : MSBuildWorkspaceCommand<AnalyzeCommandResult>
    {
        public AnalyzeCommand(AnalyzeCommandLineOptions options, DiagnosticSeverity severityLevel, in ProjectFilter projectFilter) : base(projectFilter)
        {
            Options = options;
            SeverityLevel = severityLevel;
        }

        public AnalyzeCommandLineOptions Options { get; }

        public DiagnosticSeverity SeverityLevel { get; }

        public override async Task<AnalyzeCommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            AssemblyResolver.Register();

            var codeAnalyzerOptions = new CodeAnalyzerOptions(
                ignoreAnalyzerReferences: Options.IgnoreAnalyzerReferences,
                ignoreCompilerDiagnostics: Options.IgnoreCompilerDiagnostics,
                reportNotConfigurable: Options.ReportNotConfigurable,
                reportSuppressedDiagnostics: Options.ReportSuppressedDiagnostics,
                logAnalyzerExecutionTime: Options.ExecutionTime,
                severityLevel: SeverityLevel,
                supportedDiagnosticIds: Options.SupportedDiagnostics,
                ignoredDiagnosticIds: Options.IgnoredDiagnostics);

            IEnumerable<AnalyzerAssembly> analyzerAssemblies = Options.AnalyzerAssemblies
                .SelectMany(path => AnalyzerAssemblyLoader.LoadFrom(path, loadFixers: false).Select(info => info.AnalyzerAssembly));

            CultureInfo culture = (Options.Culture != null) ? CultureInfo.GetCultureInfo(Options.Culture) : null;

            var codeAnalyzer = new CodeAnalyzer(
                analyzerAssemblies: analyzerAssemblies,
                formatProvider: culture,
                options: codeAnalyzerOptions);

            ImmutableArray<ProjectAnalysisResult> results;

            if (projectOrSolution.IsProject)
            {
                Project project = projectOrSolution.AsProject();

                ProjectAnalysisResult result = await codeAnalyzer.AnalyzeProjectAsync(project, cancellationToken);

                if (Options.Output != null
                    && result.Diagnostics.Any())
                {
                    DiagnosticXmlSerializer.Serialize(result, project, Options.Output, culture);
                }

                results = ImmutableArray.Create(result);
            }
            else
            {
                Solution solution = projectOrSolution.AsSolution();

                var projectFilter = new ProjectFilter(Options.Projects, Options.IgnoredProjects, Language);

                results = await codeAnalyzer.AnalyzeSolutionAsync(solution, f => projectFilter.IsMatch(f), cancellationToken);

                if (Options.Output != null
                    && results.Any(f => f.Diagnostics.Any()))
                {
                    DiagnosticXmlSerializer.Serialize(results, solution, Options.Output, culture);
                }
            }

            return new AnalyzeCommandResult(
                (results.Any(f => f.Diagnostics.Length > 0)) ? CommandStatus.Success : CommandStatus.NotSuccess,
                SimpleAnalysisResult.Create(results));
        }

        protected override void ProcessResults(IEnumerable<AnalyzeCommandResult> results)
        {
            WriteAnalysisResults(results.Select(f => f.AnalysisResult));
        }

        private static void WriteAnalysisResults(IEnumerable<SimpleAnalysisResult> results)
        {
            ImmutableDictionary<DiagnosticDescriptor, int> diagnostics = results
                .SelectMany(f => f.Diagnostics.Concat(f.CompilerDiagnostics))
                .GroupBy(f => f.Key, DiagnosticDescriptorComparer.Id)
                .ToImmutableDictionary(f => f.Key, f => f.Sum(f => f.Value), DiagnosticDescriptorComparer.Id);

            int totalCount = diagnostics.Sum(f => f.Value);

            if (totalCount > 0)
            {
                WriteLine(Verbosity.Normal);

                int maxCountLength = Math.Max(totalCount.ToString().Length, diagnostics.Max(f => f.Value.ToString().Length));
                int maxIdLength = diagnostics.Max(f => f.Key.Id.Length);

                foreach (KeyValuePair<DiagnosticDescriptor, int> kvp in diagnostics
                    .OrderBy(f => f.Key.Id))
                {
                    WriteLine($"{kvp.Value.ToString().PadLeft(maxCountLength)} {kvp.Key.Id.PadRight(maxIdLength)} {kvp.Key.Title}", Verbosity.Normal);
                }
            }

            WriteLine(Verbosity.Minimal);
            WriteLine($"{totalCount} {((totalCount == 1) ? "diagnostic" : "diagnostics")} found", ConsoleColor.Green, Verbosity.Minimal);
        }

        protected override void OperationCanceled(OperationCanceledException ex)
        {
            WriteLine("Analysis was canceled.", Verbosity.Quiet);
        }
    }
}
