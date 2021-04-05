// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Roslynator.Spelling
{
    public class SpellingData
    {
        private WordCharMap _charIndexMap;
        private WordCharMap _reversedCharIndexMap;
        private ImmutableDictionary<string, ImmutableHashSet<string>> _charMap;

        public static SpellingData Empty { get; } = new SpellingData(WordList.Default, WordList.CaseSensitive, FixList.Empty);

        public SpellingData(
            WordList words,
            WordList caseSensitiveWords,
            FixList fixes)
            : this(words, caseSensitiveWords, fixes, ImmutableHashSet.Create<string>(StringComparer.InvariantCulture))
        {
        }

        private SpellingData(
            WordList words,
            WordList caseSensitiveWords,
            FixList fixes,
            ImmutableHashSet<string> ignoredValues)
        {
            Words = words;
            CaseSensitiveWords = caseSensitiveWords;
            Fixes = fixes ?? FixList.Empty;
            IgnoredValues = ignoredValues.ToImmutableHashSet(StringComparer.InvariantCulture);
        }

        public WordList Words { get; }

        public WordList CaseSensitiveWords { get; }

        public ImmutableHashSet<string> IgnoredValues { get; }

        public FixList Fixes { get; }

        public WordCharMap CharIndexMap
        {
            get
            {
                if (_charIndexMap == null)
                    Interlocked.CompareExchange(ref _charIndexMap, WordCharMap.CreateCharIndexMap(Words), null);

                return _charIndexMap;
            }
        }

        public WordCharMap ReversedCharIndexMap
        {
            get
            {
                if (_reversedCharIndexMap == null)
                    Interlocked.CompareExchange(ref _reversedCharIndexMap, WordCharMap.CreateCharIndexMap(Words, reverse: true), null);

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
                    return Words.Values
                        .Select(s =>
                        {
                            char[] arr = s.ToCharArray();

                            Array.Sort(arr, (x, y) => x.CompareTo(y));

                            return (value: s, value2: new string(arr));
                        })
                        .GroupBy(f => f.value, Words.Comparer)
                        .ToImmutableDictionary(f => f.Key, f => f.Select(f => f.value2).ToImmutableHashSet(Words.Comparer));
                }
            }
        }

        public bool Contains(string value)
        {
            return IgnoredValues.Contains(value)
                || CaseSensitiveWords.Contains(value)
                || Words.Contains(value);
        }

        public WordSequenceMatch GetSequenceMatch(string value, int startIndex, int length, Match match)
        {
            WordSequenceMatch sequenceMatch = default;

            if (CaseSensitiveWords.Sequences.TryGetValue(match.Value, out ImmutableArray<WordSequence> sequences))
            {
                sequenceMatch = GetSequenceMatch(value, startIndex, length, match, sequences, Words.Comparer);

                if (sequenceMatch.IsDefault
                    && Words.Sequences.TryGetValue(match.Value, out sequences))
                {
                    sequenceMatch = GetSequenceMatch(value, startIndex, length, match, sequences, Words.Comparer);
                }
            }

            return sequenceMatch;
        }

        private WordSequenceMatch GetSequenceMatch(
            string value,
            int startIndex,
            int length,
            Match match,
            ImmutableArray<WordSequence> sequences,
            StringComparer comparer)
        {
            List<WordSequence> sequenceList = sequences.ToList();
            int endIndex = startIndex + length - 1;
            int sequenceIndex = 1;
            //int sequenceEndIndex = -1;
            int i = match.Index + match.Length - 1;

            WordSequenceMatch sequenceMatch = default;

            while (true)
            {
                int j = i;

                while (i < endIndex
                    && char.IsWhiteSpace(value[i + 1]))
                {
                    i++;
                }

                if (j == i)
                    return default;

                j = i;

                while (i < endIndex
                    && char.IsLetter(value[i + 1]))
                {
                    i++;
                }

                if (j == i)
                    return default;

                string word = value.Substring(j + 1, i - j);

                for (int k = sequenceList.Count - 1; k >= 0; k--)
                {
                    WordSequence sequence = sequenceList[k];

                    if (comparer.Equals(word, sequence.Words[sequenceIndex]))
                    {
                        if (sequence.Words.Length - 1 == sequenceIndex)
                        {
                            if (i == endIndex
                                || !char.IsLetter(value[i + 1]))
                            {
                                if (sequenceMatch.EndIndex < i)
                                    sequenceMatch = new WordSequenceMatch(sequence, match.Index, i - match.Index + 1);
                            }

                            sequenceList.RemoveAt(k);
                        }
                    }
                    else
                    {
                        sequenceList.RemoveAt(k);
                    }
                }

                if (sequenceList.Count == 0)
                    break;

                sequenceIndex++;
            }

            return sequenceMatch;
        }

        public SpellingData AddWords(IEnumerable<string> values)
        {
            WordList newList = Words.AddValues(values);

            return new SpellingData(newList, CaseSensitiveWords, Fixes, IgnoredValues);
        }

        public SpellingData AddWord(string value)
        {
            return new SpellingData(Words.AddValue(value), CaseSensitiveWords, Fixes, IgnoredValues);
        }

        public SpellingData AddFix(string error, SpellingFix fix)
        {
            FixList fixList = Fixes.Add(error, fix);

            return new SpellingData(Words, CaseSensitiveWords, fixList, IgnoredValues);
        }

        public SpellingData AddIgnoredValue(string value)
        {
            return new SpellingData(Words, CaseSensitiveWords, Fixes, IgnoredValues.Add(value));
        }

        public SpellingData AddIgnoredValues(IEnumerable<string> values)
        {
            return new SpellingData(Words, CaseSensitiveWords, Fixes, IgnoredValues.Union(values));
        }
    }
}
