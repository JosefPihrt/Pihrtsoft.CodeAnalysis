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

        public static StringComparer DefaultComparer { get; } = StringComparer.CurrentCultureIgnoreCase;

        public static WordList Default { get; } = new WordList(null, DefaultComparer, null);

        public static WordList Default_CurrentCulture { get; } = new WordList(
            null,
            StringComparer.CurrentCulture,
            null);

        public WordList(string path, StringComparer comparer, IEnumerable<string> values)
        {
            Path = path;
            Comparer = comparer ?? DefaultComparer;
            Values = values?.ToImmutableHashSet(comparer) ?? ImmutableHashSet<string>.Empty;
        }

        public string Path { get; }

        public StringComparer Comparer { get; }

        public ImmutableHashSet<string> Values { get; }

        public int Count => Values.Count;

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

        //TODO: del
        //public WordCharMap CharMap
        //{
        //    get
        //    {
        //        if (_charMap == null)
        //            Interlocked.CompareExchange(ref _charMap, WordCharMap.CreateCharMap(this), null);

        //        return _charMap;
        //    }
        //}

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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"Count = {Values.Count}  {Path}";

        public static WordList LoadFiles(IEnumerable<string> filePaths)
        {
            WordList wordList = Default;

            foreach (string filePath in filePaths)
                wordList = wordList.AddValues(LoadFile(filePath));

            return wordList;
        }

        public static WordList LoadFile(string path, StringComparer comparer = null)
        {
            IEnumerable<string> values = ReadWords(path);

            return new WordList(path, comparer, values);
        }

        public static WordList LoadText(string text, StringComparer comparer = null)
        {
            IEnumerable<string> values = ReadLines();

            values = ReadWords(values);

            return new WordList(null, comparer, values);

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
            return new WordList(Path, Comparer, Values.Add(value));
        }

        public WordList AddValues(IEnumerable<string> values)
        {
            values = Values.Concat(values).Distinct(Comparer);

            return new WordList(Path, Comparer, values);
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
            return new WordList(Path, Comparer, values);
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

        public void Save(string path = null)
        {
            Save(path ?? Path, this);
        }

        public static void Normalize(string filePath)
        {
            WordList list = LoadFile(filePath);

            list.Save();
        }

        //public WordList SaveAndLoad()
        //{
        //    Save();

        //    return LoadFile(Path, Comparer);
        //}
    }
}
