// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CodeMetrics;
using Roslynator.Host.Mef;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal class PhysicalLinesOfCodeCommand : AbstractLinesOfCodeCommand
    {
        public PhysicalLinesOfCodeCommand(PhysicalLinesOfCodeCommandLineOptions options, in ProjectFilter projectFilter) : base(projectFilter)
        {
            Options = options;
        }

        public PhysicalLinesOfCodeCommandLineOptions Options { get; }

        public override async Task<CommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            var codeMetricsOptions = new CodeMetricsOptions(
                includeGenerated: Options.IncludeGeneratedCode,
                includeWhitespace: Options.IncludeWhitespace,
                includeComments: Options.IncludeComments,
                includePreprocessorDirectives: Options.IncludePreprocessorDirectives,
                ignoreBlockBoundary: Options.IgnoreBlockBoundary);

            int count;

            if (projectOrSolution.IsProject)
            {
                Project project = projectOrSolution.AsProject();

                ICodeMetricsService service = MefWorkspaceServices.Default.GetService<ICodeMetricsService>(project.Language);

                if (service != null)
                {
                    CodeMetricsInfo codeMetrics = await CountLinesAsync(project, service, codeMetricsOptions, cancellationToken);

                    count = codeMetrics.TotalLineCount;
                }
                else
                {
                    WriteLine($"Cannot count lines for '{project.FilePath}', language '{project.Language}' is not supported", ConsoleColor.Yellow, Verbosity.Minimal);
                    return CommandResult.Fail;
                }
            }
            else
            {
                ImmutableDictionary<ProjectId, CodeMetricsInfo> codeMetrics = CountLines(projectOrSolution.AsSolution(), codeMetricsOptions, cancellationToken);

                count = codeMetrics.Sum(f => f.Value.TotalLineCount);
            }

            return (count > 0) ? CommandResult.Success : CommandResult.NotSuccess;
        }

        private async Task<CodeMetricsInfo> CountLinesAsync(Project project, ICodeMetricsService service, CodeMetricsOptions options, CancellationToken cancellationToken)
        {
            WriteLine($"Count lines for '{project.Name}'", ConsoleColor.Cyan, Verbosity.Minimal);

            Stopwatch stopwatch = Stopwatch.StartNew();

            CodeMetricsInfo codeMetrics = await service.CountLinesAsync(project, LinesOfCodeKind.Physical, options, cancellationToken);

            stopwatch.Stop();

            WriteLine(Verbosity.Minimal);

            WriteMetrics(
                codeMetrics.CodeLineCount,
                codeMetrics.BlockBoundaryLineCount,
                codeMetrics.WhitespaceLineCount,
                codeMetrics.CommentLineCount,
                codeMetrics.PreprocessorDirectiveLineCount,
                codeMetrics.TotalLineCount);

            WriteLine(Verbosity.Minimal);
            WriteLine($"Done counting lines for '{project.FilePath}' in {stopwatch.Elapsed:mm\\:ss\\.ff}", Verbosity.Normal);

            return codeMetrics;
        }

        private ImmutableDictionary<ProjectId, CodeMetricsInfo> CountLines(Solution solution, CodeMetricsOptions options, CancellationToken cancellationToken)
        {
            WriteLine($"Count lines for solution '{solution.FilePath}'", ConsoleColor.Cyan, Verbosity.Minimal);

            IEnumerable<Project> projects = FilterProjects(solution);

            Stopwatch stopwatch = Stopwatch.StartNew();

            ImmutableDictionary<ProjectId, CodeMetricsInfo> codeMetrics = CountLinesInParallel(projects, LinesOfCodeKind.Physical, options, cancellationToken);

            stopwatch.Stop();

            if (codeMetrics.Count > 0)
            {
                WriteLine(Verbosity.Normal);
                WriteLine("Lines of code by project:", Verbosity.Normal);

                WriteLinesOfCode(solution, codeMetrics);
            }

            WriteLine(Verbosity.Minimal);

            WriteMetrics(
                codeMetrics.Sum(f => f.Value.CodeLineCount),
                codeMetrics.Sum(f => f.Value.BlockBoundaryLineCount),
                codeMetrics.Sum(f => f.Value.WhitespaceLineCount),
                codeMetrics.Sum(f => f.Value.CommentLineCount),
                codeMetrics.Sum(f => f.Value.PreprocessorDirectiveLineCount),
                codeMetrics.Sum(f => f.Value.TotalLineCount));

            WriteLine(Verbosity.Minimal);
            WriteLine($"Done counting lines for solution '{solution.FilePath}' in {stopwatch.Elapsed:mm\\:ss\\.ff}", Verbosity.Minimal);

            return codeMetrics;
        }

        private void WriteMetrics(int totalCodeLineCount, int totalBlockBoundaryLineCount, int totalWhitespaceLineCount, int totalCommentLineCount, int totalPreprocessorDirectiveLineCount, int totalLineCount)
        {
            string totalCodeLines = totalCodeLineCount.ToString("n0");
            string totalBlockBoundaryLines = totalBlockBoundaryLineCount.ToString("n0");
            string totalWhitespaceLines = totalWhitespaceLineCount.ToString("n0");
            string totalCommentLines = totalCommentLineCount.ToString("n0");
            string totalPreprocessorDirectiveLines = totalPreprocessorDirectiveLineCount.ToString("n0");
            string totalLines = totalLineCount.ToString("n0");

            int maxDigits = Math.Max(
                totalCodeLines.Length,
                Math.Max(
                    totalBlockBoundaryLines.Length,
                    Math.Max(
                        totalWhitespaceLines.Length,
                        Math.Max(
                            totalCommentLines.Length,
                            Math.Max(totalPreprocessorDirectiveLines.Length, totalLines.Length)))));

            if (Options.IgnoreBlockBoundary
                || !Options.IncludeWhitespace
                || !Options.IncludeComments
                || !Options.IncludePreprocessorDirectives)
            {
                WriteLine($"{totalCodeLines.PadLeft(maxDigits)} {totalCodeLineCount / (double)totalLineCount,4:P0} lines of code", ConsoleColor.Green, Verbosity.Minimal);
            }
            else
            {
                WriteLine($"{totalCodeLines.PadLeft(maxDigits)} lines of code", ConsoleColor.Green, Verbosity.Minimal);
            }

            if (Options.IgnoreBlockBoundary)
                WriteLine($"{totalBlockBoundaryLines.PadLeft(maxDigits)} {totalBlockBoundaryLineCount / (double)totalLineCount,4:P0} block boundary lines", Verbosity.Minimal);

            if (!Options.IncludeWhitespace)
                WriteLine($"{totalWhitespaceLines.PadLeft(maxDigits)} {totalWhitespaceLineCount / (double)totalLineCount,4:P0} white-space lines", Verbosity.Minimal);

            if (!Options.IncludeComments)
                WriteLine($"{totalCommentLines.PadLeft(maxDigits)} {totalCommentLineCount / (double)totalLineCount,4:P0} comment lines", Verbosity.Minimal);

            if (!Options.IncludePreprocessorDirectives)
                WriteLine($"{totalPreprocessorDirectiveLines.PadLeft(maxDigits)} {totalPreprocessorDirectiveLineCount / (double)totalLineCount,4:P0} preprocessor directive lines", Verbosity.Minimal);

            WriteLine($"{totalLines.PadLeft(maxDigits)} {totalLineCount / (double)totalLineCount,4:P0} total lines", Verbosity.Minimal);
        }

        protected override void OperationCanceled(OperationCanceledException ex)
        {
            WriteLine("Lines counting was canceled.", Verbosity.Quiet);
        }
    }
}
