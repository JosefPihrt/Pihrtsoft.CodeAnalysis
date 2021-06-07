// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Roslynator.Spelling
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public static class WordListLoader
    {
        private static readonly Regex _splitRegex = new Regex(" +");

        public static WordListLoaderResult Load(
            IEnumerable<string> paths,
            int minWordLength = -1,
            WordListLoadOptions options = WordListLoadOptions.None,
            CancellationToken cancellationToken = default)
        {
            LoadState state = LoadState.Create(options);

            foreach (string filePath in GetFiles(paths))
            {
                cancellationToken.ThrowIfCancellationRequested();

                LoadFile(filePath, minWordLength, state);
            }

            Dictionary<string, HashSet<string>> fixes = state.Fixes;

            foreach (string word in state.Words)
                fixes.Remove(word);

            if (state.CaseSensitiveWords != null)
            {
                foreach (string word in state.CaseSensitiveWords)
                    fixes.Remove(word);
            }

            foreach (HashSet<string> values in fixes.Values)
            {
                foreach (string value in values)
                    fixes.Remove(value);
            }

            return new WordListLoaderResult(
                new WordList(state.Words, state.Sequences),
                new WordList(state.CaseSensitiveWords, state.CaseSensitiveSequences),
                FixList.Create(fixes));
        }

        internal static WordListLoaderResult LoadParallel(
            IEnumerable<string> paths,
            int minWordLength = -1,
            WordListLoadOptions options = WordListLoadOptions.None,
            CancellationToken cancellationToken = default)
        {
            var states = new ConcurrentBag<LoadState>();

            var parallelOptions = new ParallelOptions() { CancellationToken = cancellationToken };

            Parallel.ForEach(
                GetFiles(paths),
                parallelOptions,
                path =>
                {
                    LoadState state = LoadState.Create(options);
                    LoadFile(path, minWordLength, state);
                    states.Add(state);
                });

            bool isCaseSensitive = (options & WordListLoadOptions.IgnoreCase) == 0;

            var words = new List<string>(states.Sum(f => f.Words.Count));

            List<string> caseSensitiveWords = null;
            List<WordSequence> caseSensitiveSequences = null;

            if (isCaseSensitive)
            {
                caseSensitiveWords = new List<string>(states.Sum(f => f.CaseSensitiveWords!.Count));
                caseSensitiveSequences = states.SelectMany(f => f.CaseSensitiveSequences).ToList();
            }

            Dictionary<string, HashSet<string>> fixes = states.SelectMany(f => f.Fixes).ToDictionary(f => f.Key, f => f.Value);
            List<WordSequence> sequences = states.SelectMany(f => f.Sequences).ToList();

            foreach (LoadState state in states)
            {
                foreach (string word in state.Words)
                {
                    fixes.Remove(word);
                    words.Add(word);
                }

                if (isCaseSensitive)
                {
                    foreach (string word in state.CaseSensitiveWords!)
                    {
                        fixes.Remove(word);
                        caseSensitiveWords!.Add(word);
                    }
                }
            }

            foreach (HashSet<string> values in fixes.Values)
            {
                foreach (string value in values)
                    fixes.Remove(value);
            }

            return new WordListLoaderResult(
                new WordList(words, sequences),
                new WordList(caseSensitiveWords, caseSensitiveSequences),
                FixList.Create(fixes));
        }

        private static IEnumerable<string> GetFiles(IEnumerable<string> paths)
        {
            foreach (string path in paths)
            {
                if (File.Exists(path))
                {
                    yield return path;
                }
                else if (Directory.Exists(path))
                {
                    foreach (string filePath in Directory.EnumerateFiles(
                        path,
                        "*.*",
                        SearchOption.AllDirectories))
                    {
                        yield return filePath;
                    }
                }
            }
        }

        public static WordListLoaderResult LoadFile(
            string path,
            int minWordLength = -1,
            WordListLoadOptions options = WordListLoadOptions.None)
        {
            LoadState state = LoadState.Create(options);

            LoadFile(path, minWordLength, state);

            return new WordListLoaderResult(
                new WordList(state.Words, state.Sequences),
                ((options & WordListLoadOptions.IgnoreCase) == 0)
                    ? new WordList(state.CaseSensitiveWords, state.CaseSensitiveSequences)
                    : WordList.CaseSensitive,
                FixList.Create(state.Fixes));
        }

        private static void LoadFile(
            string path,
            int minWordLength,
            LoadState state)
        {
            List<string> words = state.Words;
            List<WordSequence> sequences = state.Sequences;
            List<string> caseSensitiveWords = state.CaseSensitiveWords;
            List<WordSequence> caseSensitiveSequences = state.CaseSensitiveSequences;
            Dictionary<string, HashSet<string>> fixes = state.Fixes;

            foreach (string line in File.ReadLines(path))
            {
                int i = 0;
                int startIndex = 0;
                int endIndex = line.Length;
                int separatorIndex = -1;
                int whitespaceIndex = -1;

                while (i < line.Length
                    && char.IsWhiteSpace(line[i]))
                {
                    startIndex++;
                    i++;
                }

                while (i < line.Length)
                {
                    char ch = line[i];

                    if (ch == '#')
                    {
                        endIndex = i;
                        break;
                    }
                    else if (separatorIndex == -1)
                    {
                        if (ch == ':')
                        {
                            separatorIndex = i;
                        }
                        else if (whitespaceIndex == -1
                            && char.IsWhiteSpace(ch))
                        {
                            whitespaceIndex = i;
                        }
                    }

                    i++;
                }

                int j = endIndex - 1;

                while (j >= startIndex
                    && char.IsWhiteSpace(line[j]))
                {
                    endIndex--;
                    j--;
                }

                if (separatorIndex >= 0)
                {
                    string key = line.Substring(startIndex, separatorIndex - startIndex);

                    if (key.Length >= minWordLength)
                    {
                        startIndex = separatorIndex + 1;

                        while (startIndex < endIndex
                            && char.IsWhiteSpace(line[startIndex]))
                        {
                            startIndex++;
                        }

                        string value = line.Substring(startIndex, endIndex - startIndex);

                        Debug.Assert(value.Length > 0);

                        if (fixes.TryGetValue(key, out HashSet<string> fixes2))
                        {
                            Debug.Assert(!fixes2.Contains(value), $"Fix list already contains {key}={value}");

                            fixes2.Add(value);
                        }
                        else
                        {
                            fixes[key] = new HashSet<string>(WordList.DefaultComparer) { value };
                        }
                    }
                }
                else
                {
                    string value = line.Substring(startIndex, endIndex - startIndex);

                    if (whitespaceIndex >= 0
                        && whitespaceIndex < endIndex)
                    {
                        string[] s = _splitRegex.Split(value);

                        Debug.Assert(s.Length > 1, s.Length.ToString());

                        if (s.Length > 0)
                        {
                            if (caseSensitiveSequences != null
                                && !IsLower(value))
                            {
                                caseSensitiveSequences.Add(new WordSequence(s.ToImmutableArray()));
                            }
                            else
                            {
                                sequences.Add(new WordSequence(s.ToImmutableArray()));
                            }
                        }
                    }
                    else if (value.Length >= minWordLength)
                    {
                        if (caseSensitiveWords != null
                            && !IsLower(value))
                        {
                            caseSensitiveWords.Add(value);
                        }
                        else
                        {
                            words.Add(value);
                        }
                    }
                }
            }
        }

        private static bool IsLower(string value)
        {
            foreach (char ch in value)
            {
                if (!char.IsLower(ch)
                    && !char.IsWhiteSpace(ch))
                {
                    return false;
                }
            }

            return true;
        }

        private class LoadState
        {
            private LoadState(
                List<string> words,
                List<string> caseSensitiveWords,
                List<WordSequence> sequences,
                List<WordSequence> caseSensitiveSequences,
                Dictionary<string, HashSet<string>> fixes)
            {
                Words = words;
                CaseSensitiveWords = caseSensitiveWords;
                Sequences = sequences;
                CaseSensitiveSequences = caseSensitiveSequences;
                Fixes = fixes;
            }

            public static LoadState Create(WordListLoadOptions options)
            {
                var words = new List<string>();
                var sequences = new List<WordSequence>();
                var fixes = new Dictionary<string, HashSet<string>>();

                List<string> caseSensitiveWords = null;
                List<WordSequence> caseSensitiveSequences = null;

                if ((options & WordListLoadOptions.IgnoreCase) == 0)
                {
                    caseSensitiveWords = new List<string>();
                    caseSensitiveSequences = new List<WordSequence>();
                }

                return new LoadState(words, caseSensitiveWords, sequences, caseSensitiveSequences, fixes);
            }

            public List<string> Words { get; }

            public List<string> CaseSensitiveWords { get; }

            public List<WordSequence> Sequences { get; }

            public List<WordSequence> CaseSensitiveSequences { get; }

            public Dictionary<string, HashSet<string>> Fixes { get; }
        }
    }
}
