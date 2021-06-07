// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using static Roslynator.Logger;

namespace Roslynator.Spelling
{
    [Obsolete]
    internal class SpellingFixHelpers
    {
        private static readonly Regex _lowercasedSeparatedWithUnderscoresRegex = new Regex(@"\A_*\p{Ll}+(_+\p{Ll}+)+\z");

        public SpellingFix ChooseFix(
            SpellingDiagnostic diagnostic,
            List<SpellingFix> fixes,
            bool interactive)
        {
            fixes = fixes
                .Distinct(SpellingFixComparer.InvariantCulture)
                .Where(f =>
                {
                    return f.Kind == SpellingFixKind.Predefined
                        || diagnostic.IsApplicableFix(f.Value);
                })
                .Select(fix =>
                {
                    if (TextUtility.GetTextCasing(fix.Value) != TextCasing.Undefined)
                        return fix.WithValue(TextUtility.SetTextCasing(fix.Value, diagnostic.Casing));

                    return fix;
                })
                .OrderBy(f => f.Kind)
                .Take(9)
                .ToList();

            if (fixes.Count > 0)
            {
                for (int i = 0; i < fixes.Count; i++)
                    WriteSuggestion(diagnostic, fixes[i], i, interactive);

                if (TryReadSuggestion(out int index)
                    && index < fixes.Count)
                {
                    return fixes[index];
                }
            }

            return default;
        }

        public void AddPossibleFixes(
            SpellingDiagnostic diagnostic,
            SpellingData spellingData,
            ref List<SpellingFix> fixes)
        {
            Debug.WriteLine($"find possible fix for '{diagnostic.Value}'");

            string value = diagnostic.Value;

            ImmutableArray<string> matches = SpellingFixProvider.SwapLetters(
                diagnostic.ValueLower,
                spellingData);

            foreach (string match in matches)
                fixes.Add(new SpellingFix(match, SpellingFixKind.None));

            if (fixes.Count == 0
                && (diagnostic.Casing == TextCasing.Lower
                    || diagnostic.Casing == TextCasing.FirstUpper))
            {
                foreach (int splitIndex in GetSplitIndexes(diagnostic, spellingData))
                {
                    //TODO: foofooBar > fooBar
                    //if (value.Length - splitIndex >= splitIndex
                    //    && string.Compare(value, 0, value, splitIndex, splitIndex, StringComparison.Ordinal) == 0)
                    //{
                    //    fixes.Add(new SpellingFix(value.Remove(splitIndex, splitIndex), SpellingFixKind.Split));
                    //}

                    bool? canInsertUnderscore = null;

                    if (!diagnostic.IsSymbol
                        || !(canInsertUnderscore ??= _lowercasedSeparatedWithUnderscoresRegex.IsMatch(value)))
                    {
                        // foobar > fooBar
                        // Tvalue > TValue
                        fixes.Add(new SpellingFix(
                            TextUtility.ReplaceRange(value, char.ToUpperInvariant(value[splitIndex]).ToString(), splitIndex, 1),
                            SpellingFixKind.None));
                    }

                    if (diagnostic.IsSymbol)
                    {
                        // foobar > foo_bar
                        if (canInsertUnderscore == true)
                            fixes.Add(new SpellingFix(value.Insert(splitIndex, "_"), SpellingFixKind.None));
                    }
                    else if (splitIndex > 1)
                    {
                        // foobar > foo bar
                        fixes.Add(new SpellingFix(value.Insert(splitIndex, " "), SpellingFixKind.None));
                    }
                }
            }

            //TODO: 
            //if (matches.Length == 0)
            //{
            //    matches = SpellingFixProvider.FuzzyMatches(
            //        diagnostic.ValueLower,
            //        SpellingData,
            //        cancellationToken);

            //    foreach (string match in matches)
            //        fixes.Add(new SpellingFix(match, SpellingFixKind.Fuzzy));
            //}
        }

        private void WriteSuggestion(
            SpellingDiagnostic diagnostic,
            SpellingFix fix,
            int index,
            bool interactive)
        {
            string value = diagnostic.Value;
            string containingValue = diagnostic.Parent;

            if (index == 0)
            {
                Write("    Replace  '");

                if (containingValue != null)
                {
                    Write(containingValue.Remove(diagnostic.Index));
                    Write(value);
                    Write(containingValue.Substring(diagnostic.EndIndex, containingValue.Length - diagnostic.EndIndex));
                }
                else
                {
                    Write(value, ConsoleColor.Cyan);
                }

                WriteLine("'");
            }

            Write("    ");

            if (interactive)
            {
                Write($"({index + 1}) ");
            }
            else
            {
                Write("   ");
            }

            Write("with '");

            if (containingValue != null)
            {
                Write(containingValue.Remove(diagnostic.Index));
                Write(fix.Value, ConsoleColor.Cyan);
                Write(containingValue.Substring(diagnostic.EndIndex, containingValue.Length - diagnostic.EndIndex));
            }
            else
            {
                Write(fix.Value, ConsoleColor.Cyan);
            }

            Write("'");

            if (interactive)
                Write($" ({index + 1})");

            WriteLine();
        }

        private static bool TryReadSuggestion(out int index)
        {
            Write("    Enter number of a suggestion: ");

            string text = Console.ReadLine()?.Trim();

            if (text?.Length == 1)
            {
                int num = text[0];

                if (num >= 97
                    && num <= 122)
                {
                    index = num - 97;
                    return true;
                }
            }

            if (int.TryParse(
                text,
                NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                CultureInfo.CurrentCulture,
                out index)
                && index > 0)
            {
                index--;
                return true;
            }

            index = -1;
            return false;
        }

        private static ImmutableArray<int> GetSplitIndexes(
            SpellingDiagnostic diagnostic,
            SpellingData spellingData,
            CancellationToken cancellationToken = default)
        {
            string value = diagnostic.Value;
            int length = value.Length;

            ImmutableArray<int>.Builder splitIndexes = null;

            if (length >= 4)
            {
                char ch = value[0];

                // Tvalue > TValue
                // Ienumerable > IEnumerable
                if ((ch == 'I' || ch == 'T')
                    && diagnostic.Casing == TextCasing.FirstUpper
                    && spellingData.Words.Contains(value.Substring(1)))
                {
                    (splitIndexes ??= ImmutableArray.CreateBuilder<int>()).Add(1);
                }
            }

            if (length < 6)
                return splitIndexes?.ToImmutableArray() ?? ImmutableArray<int>.Empty;

            value = diagnostic.ValueLower;

            WordCharMap map = spellingData.CharIndexMap;

            ImmutableHashSet<string> values = ImmutableHashSet<string>.Empty;

            for (int i = 0; i < length - 3; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!map.TryGetValue(value, i, out ImmutableHashSet<string> values2))
                    break;

                values = (i == 0) ? values2 : values.Intersect(values2);

                if (values.Count == 0)
                    break;

                if (i < 2)
                    continue;

                foreach (string value2 in values)
                {
                    if (value2.Length != i + 1)
                        continue;

                    ImmutableHashSet<string> values3 = ImmutableHashSet<string>.Empty;

                    for (int j = i + 1; j < length; j++)
                    {
                        if (!map.TryGetValue(value[j], j - i - 1, out ImmutableHashSet<string> values4))
                            break;

                        values3 = (j == i + 1) ? values4 : values3.Intersect(values4);

                        if (values3.Count == 0)
                            break;
                    }

                    foreach (string value3 in values3)
                    {
                        if (value3.Length != length - i - 1)
                            continue;

                        (splitIndexes ??= ImmutableArray.CreateBuilder<int>()).Add(i + 1);
                        break;
                    }

                    break;
                }
            }

            return splitIndexes?.ToImmutableArray() ?? ImmutableArray<int>.Empty;
        }
    }
}
