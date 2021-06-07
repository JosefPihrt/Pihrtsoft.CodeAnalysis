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
            SpellingScopeFilter scopeFilter) : base(projectFilter)
        {
            Options = options;
            SpellingData = spellingData;
            Visibility = visibility;
            ScopeFilter = scopeFilter;
        }

        public SpellcheckCommandLineOptions Options { get; }

        public SpellingData SpellingData { get; }

        public Visibility Visibility { get; }

        public SpellingScopeFilter ScopeFilter { get; }

        public string OutputPath { get; }

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
                scopeFilter: ScopeFilter,
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

            WriteSummary(spellingFixer);

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

        private void WriteSummary(SpellingFixer fixer)
        {
            if (!ShouldWrite(Verbosity.Normal))
                return;

            List<NewWord> newWords = fixer.NewWords.Distinct(NewWordComparer.Instance).ToList();

            if (newWords.Count == 0)
                return;

            var isFirst = true;
            bool isDetailed = ShouldWrite(Verbosity.Detailed);
            StringComparer comparer = StringComparer.InvariantCulture;

            foreach (IGrouping<string, NewWord> grouping in newWords
                .GroupBy(f => f.Value, comparer)
                .OrderBy(f => f.Key, comparer))
            {
                if (isFirst)
                {
                    WriteLine(Verbosity.Normal);
                    WriteLine("Unknown words:", Verbosity.Normal);
                    isFirst = false;
                }

                Write(grouping.Key, Verbosity.Normal);

                var isFix = false;

                if (fixer.SpellingData.Fixes.TryGetValue(grouping.Key, out ImmutableHashSet<SpellingFix> possibleFixes))
                {
                    SpellingFix fix = possibleFixes.SingleOrDefault(
                        f => (TextUtility.GetTextCasing(f.Value) != TextCasing.Undefined
                            || string.Equals(grouping.Key, f.Value, StringComparison.OrdinalIgnoreCase)),
                        shouldThrow: false);

                    if (!fix.IsDefault)
                    {
                        Write("=", ConsoleColor.Gray, Verbosity.Normal);

                        WriteLine(
                            TextUtility.SetTextCasing(fix.Value, TextUtility.GetTextCasing(grouping.Key)),
                            ConsoleColor.Cyan,
                            Verbosity.Normal);

                        isFix = true;
                    }
                }

                if (!isFix)
                    WriteLine(Verbosity.Normal);

                if (isDetailed)
                {
                    foreach (IGrouping<string, NewWord> grouping2 in grouping
                        .GroupBy(f => f.FilePath)
                        .OrderBy(f => f.Key, comparer))
                    {
                        Write("  ", Verbosity.Detailed);
                        WriteLine(grouping2.Key, Verbosity.Detailed);

                        foreach (NewWord newWord in grouping2.OrderBy(f => f.LineNumber))
                        {
                            Write("    ", Verbosity.Detailed);
                            Write(newWord.LineNumber.ToString(), ConsoleColor.Cyan, Verbosity.Detailed);
                            Write(" ", Verbosity.Detailed);

                            string line = newWord.Line;
                            string value = newWord.Value;
                            int lineCharIndex = newWord.LineSpan.StartLinePosition.Character;
                            int endIndex = lineCharIndex + value.Length;

                            Write(line.Substring(0, lineCharIndex), Verbosity.Detailed);
                            Out?.Write(">>>", Verbosity.Detailed);
                            Write(line.Substring(lineCharIndex, value.Length), ConsoleColor.Cyan, Verbosity.Detailed);
                            Out?.Write("<<<", Verbosity.Detailed);
                            WriteLine(line.Substring(endIndex, line.Length - endIndex), Verbosity.Detailed);
                        }
                    }
                }
            }

            if (ShouldWrite(Verbosity.Detailed))
            {
                isFirst = true;

                foreach (string containingValue in newWords
                    .Select(f => f.ContainingValue)
                    .Where(f => f != null)
                    .Select(f => f!)
                    .Distinct()
                    .OrderBy(f => f))
                {
                    if (isFirst)
                    {
                        WriteLine(Verbosity.Normal);
                        WriteLine("Words containing unknown words:", Verbosity.Normal);
                        isFirst = false;
                    }

                    WriteLine(containingValue, Verbosity.Normal);
                }
            }
        }

#if DEBUG
        public void SaveNewValues(
            SpellingData spellingData,
            FixList originalFixList,
            List<NewWord> newWords,
            string newWordsPath = null,
            string newFixesPath = null,
            string outputPath = null,
            CancellationToken cancellationToken = default)
        {
            if (newFixesPath != null)
            {
                Dictionary<string, List<SpellingFix>> fixes = spellingData.Fixes.Items.ToDictionary(
                    f => f.Key,
                    f => f.Value.ToList(),
                    WordList.DefaultComparer);

                if (fixes.Count > 0)
                {
                    if (File.Exists(newFixesPath))
                    {
                        foreach (KeyValuePair<string, ImmutableHashSet<SpellingFix>> kvp in FixList.LoadFile(newFixesPath!).Items)
                        {
                            if (fixes.TryGetValue(kvp.Key, out List<SpellingFix> list))
                            {
                                list.AddRange(kvp.Value);
                            }
                            else
                            {
                                fixes[kvp.Key] = kvp.Value.ToList();
                            }
                        }
                    }

                    foreach (KeyValuePair<string, ImmutableHashSet<SpellingFix>> kvp in originalFixList.Items)
                    {
                        if (fixes.TryGetValue(kvp.Key, out List<SpellingFix> list))
                        {
                            list.RemoveAll(f => kvp.Value.Contains(f, SpellingFixComparer.InvariantCultureIgnoreCase));

                            if (list.Count == 0)
                                fixes.Remove(kvp.Key);
                        }
                    }
                }

                ImmutableDictionary<string, ImmutableHashSet<SpellingFix>> newFixes = fixes.ToImmutableDictionary(
                    f => f.Key.ToLowerInvariant(),
                    f => f.Value
                        .Select(f => f.WithValue(f.Value.ToLowerInvariant()))
                        .Distinct(SpellingFixComparer.InvariantCultureIgnoreCase)
                        .ToImmutableHashSet(SpellingFixComparer.InvariantCultureIgnoreCase));

                if (newFixes.Count > 0)
                    FixList.Save(newFixesPath, newFixes);
            }

            const StringComparison comparison = StringComparison.InvariantCulture;
            StringComparer comparer = StringComparerUtility.FromComparison(comparison);

            if (newWordsPath != null)
            {
                HashSet<string> newValues = spellingData.IgnoredValues
                    .Concat(newWords.Select(f => f.Value))
                    .Except(spellingData.Fixes.Items.Select(f => f.Key), WordList.DefaultComparer)
                    .ToHashSet(comparer);

                var possibleNewFixes = new List<string>();

                foreach (string value in newValues)
                {
                    string valueLower = value.ToLowerInvariant();

                    ImmutableArray<string> possibleFixes = SpellingFixProvider.SwapLetters(valueLower, spellingData);

                    if (possibleFixes.Length == 0
                        && value.Length >= 8)
                    {
                        possibleFixes = SpellingFixProvider.Fuzzy(valueLower, spellingData, cancellationToken);
                    }

                    possibleNewFixes.AddRange(possibleFixes.Select(f => FixList.GetItemText(value, f)));
                }

                IEnumerable<string> compoundWords = newWords
                    .Select(f => f.ContainingValue)
                    .Where(f => f != null)
                    .Select(f => f!);

                WordList.Save(newWordsPath, newValues.Concat(compoundWords).Concat(possibleNewFixes), comparer, merge: true);
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
                            .GroupBy(f => f.FilePath)
                            .OrderBy(f => f.Key, comparer))
                        {
                            writer.Write("  ");
                            writer.WriteLine(grouping2.Key);

                            foreach (NewWord newWord in grouping2.OrderBy(f => f.LineSpan.StartLine()))
                            {
                                writer.Write("    ");
                                writer.Write(newWord.LineSpan.StartLine() + 1);
                                writer.Write(" ");

                                string line = newWord.Line;
                                string value = newWord.Value;
                                int lineCharIndex = newWord.LineSpan.StartLinePosition.Character;
                                int endIndex = lineCharIndex + value.Length;

                                writer.Write(line.Substring(0, lineCharIndex));
                                writer.Write(">>>");
                                writer.Write(line.Substring(lineCharIndex, value.Length));
                                writer.Write("<<<");
                                writer.WriteLine(line.Substring(endIndex, line.Length - endIndex));
                            }
                        }
                    }
                }
            }
        }
#endif
    }
}
