// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Roslynator.Spelling
{
    public sealed class WordCharMap
    {
        private WordCharMap(WordList list, ImmutableDictionary<WordChar, ImmutableHashSet<string>> map)
        {
            List = list;
            Map = map;
        }

        public WordList List { get; }

        private ImmutableDictionary<WordChar, ImmutableHashSet<string>> Map { get; }

        public ImmutableHashSet<string> this[string value, int index]
        {
            get { return Map[WordChar.Create(value, index)]; }
        }

        public ImmutableHashSet<string> this[char ch, int index]
        {
            get { return Map[new WordChar(ch, index)]; }
        }

        public bool TryGetValue(WordChar wordChar, out ImmutableHashSet<string> value)
        {
            return Map.TryGetValue(wordChar, out value);
        }

        public bool TryGetValue(string word, int index, out ImmutableHashSet<string> value)
        {
            return Map.TryGetValue(WordChar.Create(word, index), out value);
        }

        public bool TryGetValue(char ch, int index, out ImmutableHashSet<string> value)
        {
            return Map.TryGetValue(new WordChar(ch, index), out value);
        }

        public static WordCharMap CreateCharIndexMap(WordList wordList, bool reverse = false)
        {
            ImmutableDictionary<WordChar, ImmutableHashSet<string>> map = wordList.Values
                .Select(s =>
                {
                    return (
                        value: s,
                        chars: ((reverse) ? s.Reverse() : s).Select((ch, i) => (ch, i)));
                })
                .SelectMany(f => f.chars.Select(g =>
                {
                    return (
                        f.value,
                        g.ch,
                        g.i,
                        key: new WordChar(g.ch, g.i));
                }))
                .GroupBy(f => f.key)
                .ToImmutableDictionary(
                    f => f.Key,
                    f => f.Select(f => f.value).ToImmutableHashSet(wordList.Comparer));

            return new WordCharMap(wordList, map);
        }
    }
}
