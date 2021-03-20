// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading;

namespace Roslynator.RegularExpressions
{
    public class SplitItemCollection : ReadOnlyCollection<SplitItem>
    {
        internal SplitItemCollection(IList<SplitItem> list)
            : base(list)
        {
        }

        public static SplitItemCollection Create(
            Regex regex,
            string input,
            int count = 0,
            CancellationToken cancellationToken = default)
        {
            List<SplitItem> items = GetItems(regex, input, count, cancellationToken);

            return new SplitItemCollection(items);
        }

        private static List<SplitItem> GetItems(
            Regex regex,
            string input,
            int maxCount,
            CancellationToken cancellationToken)
        {
            var splits = new List<SplitItem>();

            if (maxCount == 1)
            {
                splits.Add(new SplitItem(input));
                return splits;
            }

            Match firstMatch = regex.Match(input);

            if (!firstMatch.Success)
            {
                splits.Add(new SplitItem(input));
                return splits;
            }

            int prevIndex = 0;
            int count = 0;
            int splitNumber = 1;

            foreach (Match match in EnumerateMatches(regex, firstMatch, maxCount, cancellationToken))
            {
                splits.Add(new SplitItem(input.Substring(prevIndex, match.Index - prevIndex), prevIndex, splitNumber));
                count++;
                splitNumber++;
                prevIndex = match.Index + match.Length;
            }

            splits.Add(new SplitItem(input.Substring(prevIndex, input.Length - prevIndex), prevIndex, splitNumber));
            return splits;
        }

        private static IEnumerable<Match> EnumerateMatches(
            Regex regex,
            Match match,
            int count,
            CancellationToken cancellationToken)
        {
            count--;

            if (regex.RightToLeft)
            {
                var matches = new List<Match>();

                while (match.Success)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    matches.Add(match);

                    count--;

                    if (count == 0)
                        break;

                    match = match.NextMatch();
                }

                for (int i = (matches.Count - 1); i >= 0; i--)
                    yield return matches[i];
            }
            else
            {
                while (match.Success)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    yield return match;

                    count--;

                    if (count == 0)
                        break;

                    match = match.NextMatch();
                }
            }
        }
    }
}
