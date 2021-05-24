// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Roslynator.Spelling
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class WordList
    {
        public static StringComparison DefaultComparison { get; } = StringComparison.InvariantCultureIgnoreCase;

        public static StringComparer DefaultComparer { get; } = StringComparerUtility.FromComparison(DefaultComparison);

        public static WordList Default { get; } = new WordList(null, DefaultComparison);

        public static WordList CaseSensitive { get; } = new WordList(
            null,
            StringComparison.InvariantCulture);

        public WordList(IEnumerable<string> values, StringComparison? comparison = null)
            : this(values, ImmutableArray<WordSequence>.Empty, comparison)
        {
        }

        public WordList(
            IEnumerable<string> values,
            IEnumerable<WordSequence> sequences,
            StringComparison? comparison = null)
        {
            Comparer = StringComparerUtility.FromComparison(comparison ?? DefaultComparison);
            Comparison = comparison ?? DefaultComparison;

            Values = values?.ToImmutableHashSet(Comparer) ?? ImmutableHashSet<string>.Empty;

            Sequences = sequences?
                .GroupBy(f => f.First, Comparer)
                .ToImmutableDictionary(f => f.Key, f => f.ToImmutableArray())
                ?? ImmutableDictionary<string, ImmutableArray<WordSequence>>.Empty;
        }

        public ImmutableHashSet<string> Values { get; }

        public ImmutableDictionary<string, ImmutableArray<WordSequence>> Sequences { get; }

        public StringComparison Comparison { get; }

        public StringComparer Comparer { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"Words = {Values.Count}  Sequences = {Sequences.Sum(f => f.Value.Length)}";

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
            StringComparer comparer,
            bool merge = false)
        {
            if (merge
                && File.Exists(path))
            {
                WordListLoaderResult result = WordListLoader.LoadFile(path);

                values = values.Concat(result.List.Values).Concat(result.CaseSensitiveList.Values);
            }

            values = values
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Select(f => f.Trim())
                .Distinct(comparer)
                .OrderBy(f => f, comparer);

            Debug.WriteLine($"Saving '{path}'");

            File.WriteAllText(path, string.Join(Environment.NewLine, values));
        }

        public void Save(string path)
        {
            Save(path ?? throw new ArgumentException("", nameof(path)), this);
        }

        public static void Normalize(string path)
        {
            List<string> values = File.ReadLines(path)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Select(f => f.Trim())
                .Distinct(StringComparer.InvariantCulture)
                .OrderBy(f => f, StringComparer.InvariantCulture)
                .ToList();

            string content = string.Join(Environment.NewLine, values);

            if (!string.IsNullOrEmpty(content))
                content += Environment.NewLine;

            File.WriteAllText(path, content, Encoding.UTF8);
        }
    }
}
