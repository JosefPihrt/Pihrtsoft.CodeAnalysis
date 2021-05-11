// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Roslynator.Spelling
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public static class WordListLoader
    {
        private static readonly Regex _splitRegex = new Regex(" +");

        public static WordListLoaderResult Load(
            IEnumerable<string> paths,
            int minWordLength = -1,
            WordListLoadOptions options = WordListLoadOptions.None)
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

            foreach (string filePath in GetFiles())
            {
                LoadFile(filePath, minWordLength, ref words, ref sequences, ref caseSensitiveWords, ref caseSensitiveSequences, ref fixes);
            }

            return new WordListLoaderResult(
                new WordList(words, sequences),
                new WordList(caseSensitiveWords, caseSensitiveSequences),
                FixList.Create(fixes));

            IEnumerable<string> GetFiles()
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
        }

        public static WordListLoaderResult LoadFile(
            string path,
            int minWordLength = -1,
            WordListLoadOptions options = WordListLoadOptions.None)
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

            LoadFile(path, minWordLength, ref words, ref sequences, ref caseSensitiveWords, ref caseSensitiveSequences, ref fixes);

            return new WordListLoaderResult(
                new WordList(words, sequences),
                ((options & WordListLoadOptions.IgnoreCase) == 0)
                    ? new WordList(caseSensitiveWords, caseSensitiveSequences)
                    : WordList.CaseSensitive,
                FixList.Create(fixes));
        }

        private static void LoadFile(
            string path,
            int minWordLength,
            ref List<string> words,
            ref List<WordSequence> sequences,
            ref List<string> caseSensitiveWords,
            ref List<WordSequence> caseSensitiveSequences,
            ref Dictionary<string, HashSet<string>> fixes)
        {
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
                        if (ch == '=')
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

                        string value = line.Substring(startIndex, endIndex - startIndex);

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
                if (!char.IsLower(ch))
                    return false;
            }

            return true;
        }
    }
}
