// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Roslynator.Spelling
{
    public class Spellchecker
    {
        private static readonly Regex _wordRegex = new Regex(
            @"
\b
\p{L}{2,}
(-\p{L}{2,})*
\p{L}*
(
    (?='s\b)
|
    ('(d|ll|m|re|t|ve)\b)
|
    ('(?!\p{L})\b)
|
    \b
)",
            RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex _urlRegex = new Regex(
            @"\bhttps?://[^\s]+(?=\s|\z)", RegexOptions.IgnoreCase);

        private readonly Regex _splitRegex;

        public SpellingData Data { get; }

        public SpellcheckerOptions Options { get; }

        public Spellchecker(
            SpellingData data,
            SpellcheckerOptions options)
        {
            Data = data;
            Options = options;
            _splitRegex = SplitUtility.GetSplitRegex(Options.SplitMode);
        }

        public ImmutableArray<SpellingMatch> AnalyzeText(string value)
        {
            int prevEnd = 0;

            Match match = _urlRegex.Match(value, prevEnd);

            ImmutableArray<SpellingMatch>.Builder builder = null;

            while (match.Success)
            {
                AnalyzeText(value, prevEnd, match.Index - prevEnd, ref builder);

                prevEnd = match.Index + match.Length;

                match = match.NextMatch();
            }

            AnalyzeText(value, prevEnd, value.Length - prevEnd, ref builder);

            return builder?.ToImmutableArray() ?? ImmutableArray<SpellingMatch>.Empty;
        }

        private void AnalyzeText(
            string value,
            int startIndex,
            int length,
            ref ImmutableArray<SpellingMatch>.Builder builder)
        {
            int sequenceEndIndex = -1;

            for (
                Match match = _wordRegex.Match(value, startIndex, length);
                match.Success;
                match = match.NextMatch())
            {
                if (sequenceEndIndex >= 0)
                {
                    if (match.Index <= sequenceEndIndex)
                    {
                        continue;
                    }
                    else
                    {
                        sequenceEndIndex = -1;
                    }
                }

                WordSequenceMatch sequenceMatch = Data.GetSequenceMatch(value, startIndex, length, match);

                if (!sequenceMatch.IsDefault)
                {
                    sequenceEndIndex = sequenceMatch.EndIndex;
                    continue;
                }

                if (match.Length >= Options.MinWordLength)
                {
                    if (_splitRegex == null)
                    {
                        AnalyzeValue(match.Value, match.Index, null, 0, ref builder);
                    }
                    else
                    {
                        AnalyzeSplit(_splitRegex, match.Value, match.Index, 0, ref builder);
                    }
                }
            }
        }

        internal ImmutableArray<SpellingMatch> AnalyzeIdentifier(
            string value,
            int prefixLength = 0)
        {
            if (value.Length < Options.MinWordLength)
                return ImmutableArray<SpellingMatch>.Empty;

            if (prefixLength > 0
                && Data.Contains(value))
            {
                return ImmutableArray<SpellingMatch>.Empty;
            }

            ImmutableArray<SpellingMatch>.Builder builder = null;

            AnalyzeSplit(SplitUtility.SplitIdentifierRegex, value, 0, prefixLength, ref builder);

            return builder?.ToImmutableArray() ?? ImmutableArray<SpellingMatch>.Empty;
        }

        private void AnalyzeSplit(
            Regex regex,
            string input,
            int offset,
            int prefixLength,
            ref ImmutableArray<SpellingMatch>.Builder builder)
        {
            Match match = regex.Match(input, prefixLength);

            if (!match.Success)
            {
                if (prefixLength > 0)
                {
                    AnalyzeValue(input.Substring(prefixLength), offset + prefixLength, input, offset, ref builder);
                }
                else
                {
                    AnalyzeValue(input, offset, null, 0, ref builder);
                }
            }
            else if (!Data.Contains(input))
            {
                int prevIndex = prefixLength;

                do
                {
                    AnalyzeValue(input.Substring(prevIndex, match.Index - prevIndex), prevIndex + offset, input, offset, ref builder);

                    prevIndex = match.Index + match.Length;

                    match = match.NextMatch();

                } while (match.Success);

                AnalyzeValue(input.Substring(prevIndex), prevIndex + offset, input, offset, ref builder);
            }
        }

        private void AnalyzeValue(
            string value,
            int valueIndex,
            string containingValue,
            int containingValueIndex,
            ref ImmutableArray<SpellingMatch>.Builder builder)
        {
            if (IsMatch(value))
            {
                var spellingMatch = new SpellingMatch(value, valueIndex, containingValue, containingValueIndex);

                (builder ??= ImmutableArray.CreateBuilder<SpellingMatch>()).Add(spellingMatch);
            }
        }

        private bool IsMatch(string value)
        {
            Debug.Assert(value.All(f => char.IsLetter(f) || f == '\''), value);

            if (value.Length < Options.MinWordLength)
                return false;

            if (IsAllowedNonsensicalWord(value))
                return false;

            if (Data.Contains(value))
                return false;

            return true;
        }

        private static bool IsAllowedNonsensicalWord(string value)
        {
            if (value.Length < 3)
                return false;

            switch (value)
            {
                case "xyz":
                case "Xyz":
                case "XYZ":
                case "asdfgh":
                case "Asdfgh":
                case "ASDFGH":
                case "qwerty":
                case "Qwerty":
                case "QWERTY":
                case "qwertz":
                case "Qwertz":
                case "QWERTZ":
                    return true;
            }

            if (IsAbcSequence())
                return true;

            if (IsAaaSequence())
                return true;

            if (IsAaaBbbCccSequence())
                return true;

            return false;

            bool IsAbcSequence()
            {
                int num = 0;

                if (value[0] == 'a')
                {
                    if (value[1] == 'b')
                    {
                        num = 'c';
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (value[0] == 'A')
                {
                    if (value[1] == 'B')
                    {
                        num = 'C';
                    }
                    else if (value[1] == 'b')
                    {
                        num = 'c';
                    }
                    else
                    {
                        return false;
                    }
                }

                for (int i = 2; i < value.Length; i++)
                {
                    if (value[i] != num)
                        return false;

                    num++;
                }

                return true;
            }

            bool IsAaaSequence()
            {
                char ch = value[0];
                int i = 1;

                if (ch >= 65
                    && ch <= 90
                    && value[1] == ch + 32)
                {
                    ch = (char)(ch + 32);
                    i++;
                }

                while (i < value.Length)
                {
                    if (value[i] != ch)
                        return false;

                    i++;
                }

                return true;
            }

            // aabbcc
            bool IsAaaBbbCccSequence()
            {
                char ch = value[0];
                int i = 1;

                while (i < value.Length
                    && value[i] == ch)
                {
                    i++;
                }

                if (i > 1
                    && (ch == 'a' || ch == 'A')
                    && value.Length >= 6
                    && value.Length % i == 0)
                {
                    int length = i;
                    int count = value.Length / i;

                    for (int j = 0; j < count - 1; j++)
                    {
                        var ch2 = (char)(ch + j + 1);

                        int start = i + (j * length);
                        int end = start + length;

                        for (int k = i + (j * length); k < end; k++)
                        {
                            if (ch2 != value[k])
                                return false;
                        }
                    }

                    return true;
                }

                return false;
            }
        }
    }
}
