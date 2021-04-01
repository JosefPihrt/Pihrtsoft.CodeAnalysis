// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Spelling;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal class SpellcheckCommand : MSBuildWorkspaceCommand
    {
        public SpellcheckCommand(
            SpellcheckCommandLineOptions options,
            in ProjectFilter projectFilter,
            SpellingData spellingData,
            string newWordsPath = null,
            string newFixesPath = null) : base(projectFilter)
        {
            Options = options;
            SpellingData = spellingData;

            OriginalFixList = spellingData.FixList;
            NewWordsPath = newWordsPath;
            NewFixesPath = newFixesPath;
        }

        public SpellcheckCommandLineOptions Options { get; }

        public SpellingData SpellingData { get; }

        private FixList OriginalFixList { get; }

        private string NewWordsPath { get; }

        private string NewFixesPath { get; }

        public override async Task<CommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            AssemblyResolver.Register();

            var options = new SpellingFixerOptions(
                splitMode: SplitMode.CaseAndHyphen,
                includeLocal: false,
                includeGeneratedCode: Options.IncludeGeneratedCode,
                interactive: Options.Interactive,
                dryRun: Options.DryRun);

            CultureInfo culture = (Options.Culture != null) ? CultureInfo.GetCultureInfo(Options.Culture) : null;

            var projectFilter = new ProjectFilter(Options.Projects, Options.IgnoredProjects, Language);

            return await FixAsync(projectOrSolution, options, projectFilter, culture, cancellationToken);
        }

        internal async Task<CommandResult> FixAsync(
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
            SaveNewValues(spellingFixer.SpellingData, OriginalFixList, NewWordsPath, NewFixesPath, cancellationToken);
#endif
            return CommandResult.Success;

            SpellingFixer GetSpellingFixer(Solution solution)
            {
                return new SpellingFixer(
                    solution,
                    spellingData: SpellingData,
                    formatProvider: formatProvider,
                    options: options);
            }
        }

        public void SaveNewValues(
            SpellingData data,
            FixList originalFixList,
            string wordListNewPath = null,
            string fixListNewPath = null,
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, List<SpellingFix>> dic = data.FixList.Items.ToDictionary(
                f => f.Key,
                f => f.Value.ToList(),
                WordList.DefaultComparer);

            if (dic.Count > 0)
            {
                if (File.Exists(fixListNewPath))
                {
                    foreach (KeyValuePair<string, ImmutableHashSet<SpellingFix>> kvp in FixList.LoadFile(fixListNewPath).Items)
                    {
                        if (dic.TryGetValue(kvp.Key, out List<SpellingFix> list))
                        {
                            list.AddRange(kvp.Value);
                        }
                        else
                        {
                            dic[kvp.Key] = kvp.Value.ToList();
                        }
                    }
                }

                foreach (KeyValuePair<string, ImmutableHashSet<SpellingFix>> kvp in originalFixList.Items)
                {
                    if (dic.TryGetValue(kvp.Key, out List<SpellingFix> list))
                    {
                        list.RemoveAll(f => kvp.Value.Contains(f, SpellingFixComparer.InvariantCultureIgnoreCase));

                        if (list.Count == 0)
                            dic.Remove(kvp.Key);
                    }
                }
            }

            const StringComparison comparison = StringComparison.InvariantCulture;
            StringComparer comparer = SpellingUtility.CreateStringComparer(comparison);

            if (wordListNewPath != null)
            {
                HashSet<string> values = data.IgnoreList.Values.ToHashSet(comparer);

                if (values.Count > 0)
                {
                    if (File.Exists(wordListNewPath))
                        values.UnionWith(WordList.LoadFile(wordListNewPath, comparison).Values);

                    IEnumerable<string> newValues = values
                        .Except(data.FixList.Items.Select(f => f.Key), WordList.DefaultComparer)
                        .Distinct(StringComparer.CurrentCulture)
                        .OrderBy(f => f)
                        .Select(f =>
                        {
                            string value = f.ToLowerInvariant();

                            var fixes = new List<string>();

                            fixes.AddRange(SpellingFixProvider.SwapMatches(
                                value,
                                data));

                            if (fixes.Count == 0
                                && value.Length >= 8)
                            {
                                fixes.AddRange(SpellingFixProvider.FuzzyMatches(
                                    value,
                                    data,
                                    cancellationToken));
                            }

                            if (fixes.Count > 0)
                            {
                                IEnumerable<SpellingFix> spellingFixes = fixes
                                    .Select(fix => new SpellingFix(fix, SpellingFixKind.None));

                                if (dic.TryGetValue(value, out List<SpellingFix> list))
                                {
                                    list.AddRange(spellingFixes);
                                }
                                else
                                {
                                    dic[value] = new List<SpellingFix>(spellingFixes);
                                }

                                return null;
                            }

                            return f;
                        })
                        .Where(f => f != null)
                        .Select(f => f!);

                    WordList.Save(wordListNewPath, newValues, comparer);
                }
            }

            ImmutableDictionary<string, ImmutableHashSet<SpellingFix>> fixes = dic.ToImmutableDictionary(
                f => f.Key.ToLowerInvariant(),
                f => f.Value
                    .Select(f => f.WithValue(f.Value.ToLowerInvariant()))
                    .Distinct(SpellingFixComparer.InvariantCultureIgnoreCase)
                    .ToImmutableHashSet(SpellingFixComparer.InvariantCultureIgnoreCase));

            if (fixListNewPath != null
                && fixes.Count > 0)
            {
                FixList.Save(fixListNewPath, fixes);
            }
        }

        protected override void OperationCanceled(OperationCanceledException ex)
        {
            WriteLine("Fixing was canceled.", Verbosity.Minimal);
        }
    }
}
