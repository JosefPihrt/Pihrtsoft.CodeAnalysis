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

        public static WordListLoaderResult Load(IEnumerable<string> paths, WordListLoadOptions options = WordListLoadOptions.None)
        {
            var words = new List<string>();
            var sequences = new List<WordSequence>();

            List<string> caseSensitiveWords = null;
            List<WordSequence> caseSensitiveSequences = null;

            if ((options & WordListLoadOptions.IgnoreCase) == 0)
            {
                caseSensitiveWords = new List<string>();
                caseSensitiveSequences = new List<WordSequence>();
            }

            foreach (string filePath in GetFiles())
            {
                LoadFile(filePath, ref words, ref sequences, ref caseSensitiveWords, ref caseSensitiveSequences);
            }

            return new WordListLoaderResult(new WordList(words, sequences), new WordList(caseSensitiveWords, caseSensitiveSequences));

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

        public static WordListLoaderResult LoadFile(string path, WordListLoadOptions options = WordListLoadOptions.None)
        {
            var words = new List<string>();
            var sequences = new List<WordSequence>();

            List<string> caseSensitiveWords = null;
            List<WordSequence> caseSensitiveSequences = null;

            if ((options & WordListLoadOptions.IgnoreCase) == 0)
            {
                caseSensitiveWords = new List<string>();
                caseSensitiveSequences = new List<WordSequence>();
            }

            LoadFile(path, ref words, ref sequences, ref caseSensitiveWords, ref caseSensitiveSequences);

            return new WordListLoaderResult(
                new WordList(words, sequences),
                ((options & WordListLoadOptions.IgnoreCase) == 0)
                    ? new WordList(caseSensitiveWords, caseSensitiveSequences)
                    : WordList.CaseSensitive);
        }

        private static void LoadFile(
            string path,
            ref List<string> words,
            ref List<WordSequence> sequences,
            ref List<string> caseSensitiveWords,
            ref List<WordSequence> caseSensitiveSequences)
        {
            foreach (string line in File.ReadLines(path))
            {
                int startIndex = line.IndexOf('#');

                string[] s = _splitRegex.Split((startIndex >= 0) ? line.Remove(startIndex) : line);

                if (s.Length > 0)
                {
                    if (s.Length == 1)
                    {
                        if (caseSensitiveWords != null
                            && !IsLower(line))
                        {
                            caseSensitiveWords.Add(s[0]);
                        }
                        else
                        {
                            words.Add(s[0]);
                        }
                    }
                    else if (caseSensitiveSequences != null
                        && !IsLower(line))
                    {
                        caseSensitiveSequences.Add(new WordSequence(s.ToImmutableArray()));
                    }
                    else
                    {
                        sequences.Add(new WordSequence(s.ToImmutableArray()));
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
