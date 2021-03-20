// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Roslynator.RegularExpressions;

namespace Roslynator.Spelling
{
    //TODO: decode html entity?
    //TODO: parse email address
    public class SpellingParser
    {
        private static readonly Regex _wordInCommentRegex = new Regex(
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

        // NaN, IDs, GACed, JSONify, AND'd
        private static readonly Regex _specialWordRegex = new Regex(
            @"
\A
(?:
    (?:
        (?<g>\p{Lu}\p{Ll}\p{Lu})
    )
    |
    (?:
        (?<g>\p{Lu}{2,})
        (?:s|ed|ify|'d)
    )
)
\z
",
            RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex _urlRegex = new Regex(
            @"\bhttps?://[^\s]+(?=\s|\z)", RegexOptions.IgnoreCase);

        public SpellingData SpellingData { get; }

        public SpellingParserOptions Options { get; }

        public CancellationToken CancellationToken { get; }

        public SpellingParser(
            SpellingData spellingData,
            SpellingParserOptions options,
            CancellationToken cancellationToken)
        {
            SpellingData = spellingData;
            Options = options;
            CancellationToken = cancellationToken;
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
            Regex splitRegex = SplitUtility.GetSplitRegex(Options.SplitMode);

            for (
                Match match = _wordInCommentRegex.Match(value, startIndex, length);
                match.Success;
                match = match.NextMatch())
            {
                if (match.Length >= Options.MinWordLength)
                {
                    if (splitRegex == null)
                    {
                        TryAddMatch(match.Value, match.Index, ref builder);
                    }
                    else
                    {
                        Match match2 = _specialWordRegex.Match(match.Value);

                        if (match2.Success)
                        {
                            Group group = match2.Groups["g"];

                            TryAddMatch(group.Value, group.Index, ref builder);
                        }
                        else
                        {
                            foreach (SplitItem splitItem in SplitItemCollection.Create(splitRegex, match.Value))
                            {
                                TryAddMatch(splitItem.Value, match.Index + splitItem.Index, ref builder);
                            }
                        }
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

            if (prefixLength > 0)
            {
                if (SpellingData.IgnoreList.Contains(value))
                    return ImmutableArray<SpellingMatch>.Empty;

                if (SpellingData.List.Contains(value))
                    return ImmutableArray<SpellingMatch>.Empty;
            }

            string value2 = (prefixLength > 0) ? value.Substring(prefixLength) : value;

            Match match = _specialWordRegex.Match(value2);

            ImmutableArray<SpellingMatch>.Builder builder = null;

            if (match.Success)
            {
                Group group = match.Groups["g"];

                TryAddMatch(
                    group.Value,
                    prefixLength,
                    ref builder);
            }
            else
            {
                SplitItemCollection splitItems = SplitItemCollection.Create(SplitUtility.SplitIdentifierRegex, value2);

                if (splitItems.Count > 1)
                {
                    if (SpellingData.IgnoreList.Contains(value2))
                        return ImmutableArray<SpellingMatch>.Empty;

                    if (SpellingData.List.Contains(value2))
                        return ImmutableArray<SpellingMatch>.Empty;
                }

                foreach (SplitItem splitItem in splitItems)
                {
                    Debug.Assert(splitItem.Value.All(f => char.IsLetter(f)), splitItem.Value);

                    TryAddMatch(
                        splitItem.Value,
                        splitItem.Index + prefixLength,
                        ref builder);
                }
            }

            return builder?.ToImmutableArray() ?? ImmutableArray<SpellingMatch>.Empty;
        }

        private void TryAddMatch(string value, int index, ref ImmutableArray<SpellingMatch>.Builder builder)
        {
            if (IsMatch(value))
            {
                (builder ??= ImmutableArray.CreateBuilder<SpellingMatch>()).Add(new SpellingMatch(value, index));
            }
        }

        private bool IsMatch(string value)
        {
            Debug.Assert(value.All(f => char.IsLetter(f) || f == '\''), value);

            if (value.Length < Options.MinWordLength)
                return false;

            if (IsAllowedNonsensicalWord(value))
                return false;

            if (SpellingData.IgnoreList.Contains(value))
                return false;

            if (SpellingData.List.Contains(value))
                return false;

            return true;
        }

        public static bool IsAllowedNonsensicalWord(string value)
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
