// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System;

namespace Roslynator.Spelling
{
    public static class SpellingFixProvider
    {
        public static ImmutableArray<string> Fuzzy(
            string value,
            SpellingData spellingData,
            CancellationToken cancellationToken = default)
        {
            int length = value.Length;

            if (length <= 3)
                return ImmutableArray<string>.Empty;

            ImmutableHashSet<string>.Builder matches = null;
            WordCharMap map = spellingData.CharIndexMap;
            WordCharMap reversedMap = spellingData.ReversedCharIndexMap;
            var intersects = new ImmutableHashSet<string>[length];

            int i = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!map.TryGetValue(value, i, out ImmutableHashSet<string> values))
                    break;

                ImmutableHashSet<string> intersect = (i == 0)
                    ? values
                    : intersects[i - 1].Intersect(values);

                if (i == length - 2)
                {
                    TryAddMatches(intersect, length, ref matches);
                }
                else if (i == length - 1)
                {
                    TryAddMatches(intersect, length + 1, ref matches);

                    break;
                }

                if (intersect.Count == 0)
                    break;

                intersects[i] = intersect;
                i++;
            }

            int j = length - 1;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!reversedMap.TryGetValue(value[j], length - j - 1, out ImmutableHashSet<string> values))
                    break;

                ImmutableHashSet<string> intersect = (j == length - 1)
                    ? values
                    : values.Intersect(intersects[j + 1]);

                if (j == 1)
                {
                    TryAddMatches(intersect, length, ref matches);
                }
                else if (j == 0)
                {
                    TryAddMatches(intersect, length + 1, ref matches);
                    break;
                }

                if (intersect.Count == 0)
                    break;

                int diff = j - i;

                if (diff <= 0)
                {
                    if (TryAddMatches(intersects[j - 1].Intersect(intersect), length + 1, ref matches))
                        break;
                }
                else if (diff == 1
                    && j > 1)
                {
                    if (TryAddMatches(intersects[j - 2].Intersect(intersect), length - 1, length, ref matches))
                        break;
                }

                intersects[j] = intersect;
                j--;
            }

            if (matches == null)
                return ImmutableArray<string>.Empty;

            return matches
                .OrderBy(f => f)
                .ToImmutableArray();
        }

        private static bool TryAddMatches(
            ImmutableHashSet<string> values,
            int requiredLength,
            ref ImmutableHashSet<string>.Builder matches)
        {
            return TryAddMatches(values, requiredLength, requiredLength, ref matches);
        }

        private static bool TryAddMatches(
            ImmutableHashSet<string> values,
            int minRequiredLength,
            int maxRequiredLength,
            ref ImmutableHashSet<string>.Builder matches)
        {
            var success = false;

            foreach (string value2 in values)
            {
                if (value2.Length < minRequiredLength
                    || value2.Length > maxRequiredLength)
                {
                    continue;
                }

                if ((matches ??= ImmutableHashSet.CreateBuilder(WordList.DefaultComparer)).Add(value2))
                    success = true;
            }

            return success;
        }

        public static ImmutableArray<string> SwapLetters(string value, SpellingData spellingData)
        {
            int length = value.Length;

            if (length < 4)
                return ImmutableArray<string>.Empty;

            ImmutableArray<string>.Builder fixes = null;

            char[] arr = value.ToCharArray();

            Array.Sort(arr, (x, y) => x.CompareTo(y));

            var key = new string(arr);

            if (!spellingData.CharMap.TryGetValue(key, out ImmutableHashSet<string> values))
                return ImmutableArray<string>.Empty;

            int maxCharDiff = (length <= 6) ? 2 : 3;

            foreach (string value2 in values)
            {
                if (length == value2.Length)
                {
                    int charDiff = 0;
                    int diffStartIndex = -1;
                    int diffEndIndex = -1;

                    for (int i = 0; i < length; i++)
                    {
                        if (value[i] != value2[i])
                        {
                            if (diffStartIndex == -1)
                            {
                                diffStartIndex = i;
                            }
                            else
                            {
                                diffEndIndex = i;
                            }

                            charDiff++;
                        }

                        if (i > 1
                            && charDiff > maxCharDiff)
                        {
                            return ImmutableArray<string>.Empty;
                        }
                    }

                    if (diffEndIndex - diffStartIndex <= 2)
                        (fixes ??= ImmutableArray.CreateBuilder<string>()).Add(value2);
                }
            }

            return fixes?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
        }
    }
}
