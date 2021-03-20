// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Spelling;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal class FixSpellingCommand : MSBuildWorkspaceCommand
    {
        public FixSpellingCommand(FixSpellingCommandLineOptions options, in ProjectFilter projectFilter) : base(projectFilter)
        {
            Options = options;
        }

        public FixSpellingCommandLineOptions Options { get; }

        public override async Task<CommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            AssemblyResolver.Register();

            SpellingFixerOptions options;
            if (Options.Interactive)
            {
                options = new SpellingFixerOptions(
                    splitMode: SplitMode.CaseAndHyphen,
                    includeLocal: false,
                    includeGeneratedCode: Options.IncludeGeneratedCode,
                    interactive: Options.Interactive);
            }
            else
            {
                options = new SpellingFixerOptions(
                    splitMode: SplitMode.CaseAndHyphen,
                    includeLocal: true,
                    includeGeneratedCode: Options.IncludeGeneratedCode,
                    interactive: Options.Interactive,
                    dryRun: true);
            }

            CultureInfo culture = (Options.Culture != null) ? CultureInfo.GetCultureInfo(Options.Culture) : null;

            var projectFilter = new ProjectFilter(Options.Projects, Options.IgnoredProjects, Language);

            return await FixAsync(projectOrSolution, options, projectFilter, culture, cancellationToken);
        }

        internal static async Task<CommandResult> FixAsync(
            ProjectOrSolution projectOrSolution,
            SpellingFixerOptions options,
            ProjectFilter projectFilter,
            IFormatProvider formatProvider = null,
            CancellationToken cancellationToken = default)
        {
            SpellingFixer spellingFixer;

            if (projectOrSolution.IsProject)
            {
                Project project = projectOrSolution.AsProject();

                Solution solution = project.Solution;

                spellingFixer = GetSpellingFixer(solution);

                WriteLine($"Fix '{project.Name}'", ConsoleColor.Cyan, Verbosity.Minimal);

                Stopwatch stopwatch = Stopwatch.StartNew();

                ImmutableArray<SpellingFixResult> results = await spellingFixer.FixProjectAsync(project, cancellationToken);

                stopwatch.Stop();

                WriteLine($"Done fixing project '{project.FilePath}' in {stopwatch.Elapsed:mm\\:ss\\.ff}", Verbosity.Minimal);
            }
            else
            {
                Solution solution = projectOrSolution.AsSolution();

                spellingFixer = GetSpellingFixer(solution);

                await spellingFixer.FixSolutionAsync(f => projectFilter.IsMatch(f), cancellationToken);
            }
#if DEBUG
            Console.WriteLine("Saving new values");
            WordListHelpers.SaveNewValues(spellingFixer.SpellingData, cancellationToken);
#endif
            return CommandResult.Success;

            SpellingFixer GetSpellingFixer(Solution solution)
            {
                SpellingData spellingData = SpellingData.Empty;

                string assemblyPath = typeof(FixCommand).Assembly.Location;

                if (!string.IsNullOrEmpty(assemblyPath))
                {
                    IEnumerable<string> wordListPaths = Directory.EnumerateFiles(
                        Path.Combine(Path.GetDirectoryName(assemblyPath), "_words"),
                        "*.txt",
                        SearchOption.AllDirectories);

                    WordList wordList = WordList.LoadFiles(wordListPaths);

                    IEnumerable<string> fixListPaths = Directory.EnumerateFiles(
                        Path.Combine(Path.GetDirectoryName(assemblyPath), "_fixes"),
                        "*.txt",
                        SearchOption.AllDirectories);

                    FixList fixList = FixList.LoadFiles(fixListPaths);

                    spellingData = new SpellingData(wordList, fixList, default);
                }

                return new SpellingFixer(
                    solution,
                    spellingData: spellingData,
                    formatProvider: formatProvider,
                    options: options);
            }
        }

        protected override void OperationCanceled(OperationCanceledException ex)
        {
            WriteLine("Fixing was canceled.", Verbosity.Minimal);
        }
    }
}
