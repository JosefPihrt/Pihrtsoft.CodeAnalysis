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
    internal class AnalyzeCommand : MSBuildWorkspaceCommand
    {
        public AnalyzeCommand(AnalyzeCommandLineOptions options, DiagnosticSeverity severityLevel, in ProjectFilter projectFilter) : base(projectFilter)
        {
            Options = options;
            SeverityLevel = severityLevel;
        }

        public AnalyzeCommandLineOptions Options { get; }

        public DiagnosticSeverity SeverityLevel { get; }

        public override async Task<CommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
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

            var success = false;

            if (projectOrSolution.IsProject)
            {
                Project project = projectOrSolution.AsProject();

                ProjectAnalysisResult result = await codeAnalyzer.AnalyzeProjectAsync(project, cancellationToken);

                success = result.Diagnostics.Length > 0;

                if (Options.Output != null
                    && result.Diagnostics.Any())
                {
                    DiagnosticXmlSerializer.Serialize(result, project, Options.Output, culture);
                }
            }
            else
            {
                Solution solution = projectOrSolution.AsSolution();

                var projectFilter = new ProjectFilter(Options.Projects, Options.IgnoredProjects, Language);

                ImmutableArray<ProjectAnalysisResult> results = await codeAnalyzer.AnalyzeSolutionAsync(solution, f => projectFilter.IsMatch(f), cancellationToken);

                success = results.Any(f => f.Diagnostics.Length > 0);

                if (Options.Output != null
                    && results.Any(f => f.Diagnostics.Any()))
                {
                    DiagnosticXmlSerializer.Serialize(results, solution, Options.Output, culture);
                }
            }

            return (success) ? CommandResult.Success : CommandResult.NotSuccess;
        }

        protected override void OperationCanceled(OperationCanceledException ex)
        {
            WriteLine("Analysis was canceled.", Verbosity.Quiet);
        }
    }
}
