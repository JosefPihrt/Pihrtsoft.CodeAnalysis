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
    internal class LogicalLinesOfCodeCommand : AbstractLinesOfCodeCommand
    {
        public LogicalLinesOfCodeCommand(LogicalLinesOfCodeCommandLineOptions options, in ProjectFilter projectFilter) : base(projectFilter)
        {
            Options = options;
        }

        public LogicalLinesOfCodeCommandLineOptions Options { get; }

        public override async Task<CommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            var codeMetricsOptions = new CodeMetricsOptions(includeGenerated: Options.IncludeGeneratedCode);

            int count;

            if (projectOrSolution.IsProject)
            {
                Project project = projectOrSolution.AsProject();

                ICodeMetricsService service = MefWorkspaceServices.Default.GetService<ICodeMetricsService>(project.Language);

                if (service != null)
                {
                    CodeMetricsInfo codeMetrics = await CountLogicalLinesAsync(project, service, codeMetricsOptions, cancellationToken);

                    count = codeMetrics.TotalLineCount;
                }
                else
                {
                    WriteLine($"Cannot count logical lines for language '{project.Language}'", ConsoleColor.Yellow, Verbosity.Minimal);
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

        private static async Task<CodeMetricsInfo> CountLogicalLinesAsync(Project project, ICodeMetricsService service, CodeMetricsOptions options, CancellationToken cancellationToken)
        {
            WriteLine($"Count logical lines for '{project.Name}'", ConsoleColor.Cyan, Verbosity.Minimal);

            Stopwatch stopwatch = Stopwatch.StartNew();

            CodeMetricsInfo codeMetrics = await service.CountLinesAsync(project, LinesOfCodeKind.Logical, options, cancellationToken);

            stopwatch.Stop();

            WriteLine(Verbosity.Minimal);

            WriteMetrics(
                codeMetrics.CodeLineCount,
                codeMetrics.WhitespaceLineCount,
                codeMetrics.CommentLineCount,
                codeMetrics.PreprocessorDirectiveLineCount,
                codeMetrics.TotalLineCount);

            WriteLine(Verbosity.Minimal);
            WriteLine($"Done counting logical lines for '{project.FilePath}' in {stopwatch.Elapsed:mm\\:ss\\.ff}", Verbosity.Normal);

            return codeMetrics;
        }

        private ImmutableDictionary<ProjectId, CodeMetricsInfo> CountLines(Solution solution, CodeMetricsOptions options, CancellationToken cancellationToken)
        {
            WriteLine($"Count logical lines for solution '{solution.FilePath}'", ConsoleColor.Cyan, Verbosity.Minimal);

            IEnumerable<Project> projects = FilterProjects(solution);

            Stopwatch stopwatch = Stopwatch.StartNew();

            ImmutableDictionary<ProjectId, CodeMetricsInfo> codeMetrics = CountLinesInParallel(projects, LinesOfCodeKind.Logical, options, cancellationToken);

            stopwatch.Stop();

            if (codeMetrics.Count > 0)
            {
                WriteLine(Verbosity.Normal);
                WriteLine("Logical lines of code by project:", Verbosity.Normal);

                WriteLinesOfCode(solution, codeMetrics);
            }

            WriteLine(Verbosity.Minimal);

            WriteMetrics(
                codeMetrics.Sum(f => f.Value.CodeLineCount),
                codeMetrics.Sum(f => f.Value.WhitespaceLineCount),
                codeMetrics.Sum(f => f.Value.CommentLineCount),
                codeMetrics.Sum(f => f.Value.PreprocessorDirectiveLineCount),
                codeMetrics.Sum(f => f.Value.TotalLineCount));

            WriteLine(Verbosity.Minimal);
            WriteLine($"Done counting logical lines for solution '{solution.FilePath}' in {stopwatch.Elapsed:mm\\:ss\\.ff}", Verbosity.Normal);

            return codeMetrics;
        }

        private static void WriteMetrics(
            int totalCodeLineCount,
            int totalWhitespaceLineCount,
            int totalCommentLineCount,
            int totalPreprocessorDirectiveLineCount,
            int totalLineCount)
        {
            string totalCodeLines = totalCodeLineCount.ToString("n0");
            string totalWhitespaceLines = totalWhitespaceLineCount.ToString("n0");
            string totalCommentLines = totalCommentLineCount.ToString("n0");
            string totalPreprocessorDirectiveLines = totalPreprocessorDirectiveLineCount.ToString("n0");
            string totalLines = totalLineCount.ToString("n0");

            int maxDigits = Math.Max(
                totalCodeLines.Length,
                Math.Max(
                    totalWhitespaceLines.Length,
                    Math.Max(
                        totalCommentLines.Length,
                        Math.Max(totalPreprocessorDirectiveLines.Length, totalLines.Length))));

            WriteLine($"{totalCodeLines.PadLeft(maxDigits)} {totalCodeLineCount / (double)totalLineCount,4:P0} logical lines of code", ConsoleColor.Green, Verbosity.Minimal);
            WriteLine($"{totalWhitespaceLines.PadLeft(maxDigits)} {totalWhitespaceLineCount / (double)totalLineCount,4:P0} white-space lines", Verbosity.Minimal);
            WriteLine($"{totalCommentLines.PadLeft(maxDigits)} {totalCommentLineCount / (double)totalLineCount,4:P0} comment lines", Verbosity.Minimal);
            WriteLine($"{totalPreprocessorDirectiveLines.PadLeft(maxDigits)} {totalPreprocessorDirectiveLineCount / (double)totalLineCount,4:P0} preprocessor directive lines", Verbosity.Minimal);
            WriteLine($"{totalLines.PadLeft(maxDigits)} {totalLineCount / (double)totalLineCount,4:P0} total lines", Verbosity.Minimal);
        }

        protected override void OperationCanceled(OperationCanceledException ex)
        {
            WriteLine("Logical lines counting was canceled.", Verbosity.Quiet);
        }
    }
}
