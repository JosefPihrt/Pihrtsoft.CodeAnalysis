// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Roslynator.Spelling
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class WordList
    {
        private WordCharMap _charIndexMap;
        private WordCharMap _reversedCharIndexMap;
        private ImmutableDictionary<string, ImmutableHashSet<string>> _charMap;

        public static StringComparison DefaultComparison { get; } = StringComparison.InvariantCultureIgnoreCase;

        public static StringComparer DefaultComparer { get; } = SpellingUtility.CreateStringComparer(DefaultComparison);

        public static WordList Default { get; } = new WordList(null, DefaultComparison);

        public static WordList CaseSensitive { get; } = new WordList(
            null,
            StringComparison.InvariantCulture);

        public WordList(IEnumerable<string> values, StringComparison? comparison = null)
        {
            Comparer = SpellingUtility.CreateStringComparer(comparison ?? DefaultComparison);
            Values = values?.ToImmutableHashSet(Comparer) ?? ImmutableHashSet<string>.Empty;
            Comparison = comparison ?? DefaultComparison;
        }

        public ImmutableHashSet<string> Values { get; }

        public StringComparison Comparison { get; }

        public StringComparer Comparer { get; }

        public int Count => Values.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"Count = {Values.Count}";

        public WordCharMap CharIndexMap
        {
            get
            {
                if (_charIndexMap == null)
                    Interlocked.CompareExchange(ref _charIndexMap, WordCharMap.CreateCharIndexMap(this), null);

                return _charIndexMap;
            }
        }

        public WordCharMap ReversedCharIndexMap
        {
            get
            {
                if (_reversedCharIndexMap == null)
                    Interlocked.CompareExchange(ref _reversedCharIndexMap, WordCharMap.CreateCharIndexMap(this, reverse: true), null);

                return _reversedCharIndexMap;
            }
        }

        public ImmutableDictionary<string, ImmutableHashSet<string>> CharMap
        {
            get
            {
                if (_charMap == null)
                    Interlocked.CompareExchange(ref _charMap, Create(), null);

                return _charMap;

                ImmutableDictionary<string, ImmutableHashSet<string>> Create()
                {
                    return Values
                        .Select(s =>
                        {
                            char[] arr = s.ToCharArray();

                            Array.Sort(arr, (x, y) => x.CompareTo(y));

                            return (value: s, value2: new string(arr));
                        })
                        .GroupBy(f => f.value, Comparer)
                        .ToImmutableDictionary(f => f.Key, f => f.Select(f => f.value2).ToImmutableHashSet(Comparer));
                }
            }
        }

        public static WordList Load(IEnumerable<string> paths)
        {
            IEnumerable<string> values = GetFiles().SelectMany(path => ReadWords(path));

            return new WordList(values, DefaultComparison);

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

        public static WordList LoadFiles(IEnumerable<string> filePaths)
        {
            WordList wordList = Default;

            foreach (string filePath in filePaths)
                wordList = wordList.AddValues(LoadFile(filePath));

            return wordList;
        }

        public static WordList LoadFile(string path, StringComparison? stringComparison = null)
        {
            IEnumerable<string> values = ReadWords(path);

            return new WordList(values, stringComparison);
        }

        public static WordList LoadText(string text, StringComparison? comparison = null)
        {
            IEnumerable<string> values = ReadLines();

            values = ReadWords(values);

            return new WordList(values, comparison);

            IEnumerable<string> ReadLines()
            {
                using (var sr = new StringReader(text))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                        yield return line;
                }
            }
        }

        private static IEnumerable<string> ReadWords(string path)
        {
            return ReadWords(File.ReadLines(path));
        }

        private static IEnumerable<string> ReadWords(IEnumerable<string> values)
        {
            return values
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Select(f => f.Trim())
                .Where(f => !f.StartsWith("#"));
        }

        public WordList Intersect(WordList wordList, params WordList[] additionalWordLists)
        {
            IEnumerable<string> intersect = Values.Intersect(wordList.Values, Comparer);

            if (additionalWordLists?.Length > 0)
            {
                intersect = intersect
                    .Intersect(additionalWordLists.SelectMany(f => f.Values), Comparer);
            }

            return WithValues(intersect);
        }

        public WordList Except(WordList wordList, params WordList[] additionalWordLists)
        {
            IEnumerable<string> except = Values.Except(wordList.Values, Comparer);

            if (additionalWordLists?.Length > 0)
            {
                except = except
                    .Except(additionalWordLists.SelectMany(f => f.Values), Comparer);
            }

            return WithValues(except);
        }

        public bool Contains(string value)
        {
            return Values.Contains(value);
        }

        public WordList AddValue(string value)
        {
            return new WordList(Values.Add(value), Comparison);
        }

        public WordList AddValues(IEnumerable<string> values)
        {
            values = Values.Concat(values).Distinct(Comparer);

            return new WordList(values, Comparison);
        }

        public WordList AddValues(WordList wordList, params WordList[] additionalWordLists)
        {
            IEnumerable<string> concat = Values.Concat(wordList.Values);

            if (additionalWordLists?.Length > 0)
                concat = concat.Concat(additionalWordLists.SelectMany(f => f.Values));

            return WithValues(concat.Distinct(Comparer));
        }

        public WordList WithValues(IEnumerable<string> values)
        {
            return new WordList(values, Comparison);
        }

        public static void Save(
            string path,
            WordList wordList)
        {
            Save(path, wordList.Values, wordList.Comparer);
        }

        public static void Save(
            string path,
            IEnumerable<string> values,
            StringComparer comparer)
        {
            values = values
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Select(f => f.Trim())
                .Distinct(comparer)
                .OrderBy(f => f, StringComparer.InvariantCulture);

            Debug.WriteLine($"Saving '{path}'");

            File.WriteAllText(path, string.Join(Environment.NewLine, values));
        }

        public void Save(string path)
        {
            Save(path ?? throw new ArgumentException("", nameof(path)), this);
        }

        public static void Normalize(string path)
        {
            WordList list = LoadFile(path);

            list.Save(path);
        }
    }
}
