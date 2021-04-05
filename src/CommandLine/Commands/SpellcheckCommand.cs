// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
            Visibility visibility,
            string newWordsPath = null,
            string newFixesPath = null,
            string outputPath = null) : base(projectFilter)
        {
            Options = options;
            SpellingData = spellingData;
            Visibility = visibility;
            NewWordsPath = newWordsPath;
            NewFixesPath = newFixesPath;
            OutputPath = outputPath;

            OriginalFixes = spellingData.Fixes;
        }

        public SpellcheckCommandLineOptions Options { get; }

        public SpellingData SpellingData { get; }

        public Visibility Visibility { get; }

        private string NewWordsPath { get; }

        private string NewFixesPath { get; }

        public string OutputPath { get; }

        private FixList OriginalFixes { get; }

        public override async Task<CommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            AssemblyResolver.Register();

            VisibilityFilter visibilityFilter = Visibility switch
            {
                Visibility.Public => VisibilityFilter.All,
                Visibility.Internal => VisibilityFilter.Internal | VisibilityFilter.Private,
                Visibility.Private => VisibilityFilter.Private,
                _ => throw new InvalidOperationException()
            };

            var options = new SpellingFixerOptions(
                symbolVisibility: visibilityFilter,
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

            SaveNewValues(
                spellingFixer.SpellingData,
                OriginalFixes,
                spellingFixer.NewWords,
                NewWordsPath,
                NewFixesPath,
                OutputPath,
                cancellationToken);

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

        protected override void OperationCanceled(OperationCanceledException ex)
        {
            WriteLine("Spellchecking was canceled.", Verbosity.Minimal);
        }

        public void SaveNewValues(
            SpellingData spellingData,
            FixList originalFixList,
            List<NewWord> newWords,
            string newWordsPath = null,
            string newFixesPath = null,
            string outputPath = null,
            CancellationToken cancellationToken = default)
        {
            Debug.WriteLine("Saving new values");

            Dictionary<string, List<SpellingFix>> dic = spellingData.Fixes.Items.ToDictionary(
                f => f.Key,
                f => f.Value.ToList(),
                WordList.DefaultComparer);

            if (dic.Count > 0)
            {
                if (File.Exists(newFixesPath))
                {
                    foreach (KeyValuePair<string, ImmutableHashSet<SpellingFix>> kvp in FixList.LoadFile(newFixesPath!).Items)
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
            StringComparer comparer = StringComparerUtility.FromComparison(comparison);

            if (newWordsPath != null)
            {
                HashSet<string> values = spellingData.IgnoredValues.ToHashSet(comparer);

                if (values.Count > 0)
                {
                    if (File.Exists(newWordsPath))
                        values.UnionWith(WordListLoader.LoadFile(newWordsPath).List.Values);

                    IEnumerable<string> newValues = values
                        .Except(spellingData.Fixes.Items.Select(f => f.Key), WordList.DefaultComparer)
                        .Distinct(comparer)
                        .OrderBy(f => f, comparer);

                    if (newFixesPath != null)
                    {
                        newValues = newValues
                            .Select(f =>
                            {
                                string value = f.ToLowerInvariant();

                                var fixes = new List<string>();

                                fixes.AddRange(SpellingFixProvider.SwapLetters(value, spellingData));

                                if (fixes.Count == 0
                                    && value.Length >= 8)
                                {
                                    fixes.AddRange(SpellingFixProvider.Fuzzy(value, spellingData, cancellationToken));
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
                    }

                    IEnumerable<string> compoundWords = newWords
                        .Select(f => f.ContainingValue)
                        .Where(f => f != null);

                    WordList.Save(newWordsPath, newValues.Concat(compoundWords), comparer);
                }
            }

            if (newFixesPath != null)
            {
                ImmutableDictionary<string, ImmutableHashSet<SpellingFix>> fixes = dic.ToImmutableDictionary(
                    f => f.Key.ToLowerInvariant(),
                    f => f.Value
                        .Select(f => f.WithValue(f.Value.ToLowerInvariant()))
                        .Distinct(SpellingFixComparer.InvariantCultureIgnoreCase)
                        .ToImmutableHashSet(SpellingFixComparer.InvariantCultureIgnoreCase));

                if (fixes.Count > 0)
                    FixList.Save(newFixesPath, fixes);
            }

            if (outputPath != null
                && newWords.Count > 0)
            {
                using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
                {
                    foreach (IGrouping<string, NewWord> grouping in newWords
                        .GroupBy(f => f.Value, comparer)
                        .OrderBy(f => f.Key, comparer))
                    {
                        writer.WriteLine(grouping.Key);

                        foreach (IGrouping<string, NewWord> grouping2 in grouping
                            .GroupBy(f => f.LineSpan.Path)
                            .OrderBy(f => f.Key, comparer))
                        {
                            writer.Write("  ");
                            writer.WriteLine(grouping2.Key);

                            foreach (NewWord newWord in grouping2)
                            {
                                writer.Write("    ");
                                writer.Write(newWord.LineSpan.StartLine() + 1);
                                writer.Write(" ");

                                string line = newWord.Line;
                                string value = newWord.Value;
                                int lineCharIndex = newWord.LineSpan.StartLinePosition.Character;
                                int endIndex = lineCharIndex + value.Length;

                                writer.Write(line.Substring(0, lineCharIndex));
                                writer.Write(">>>>>");
                                writer.Write(line.Substring(lineCharIndex, value.Length));
                                writer.Write("<<<<<");
                                writer.WriteLine(line.Substring(endIndex, line.Length - endIndex));
                            }
                        }
                    }
                }
            }
        }
    }
}
