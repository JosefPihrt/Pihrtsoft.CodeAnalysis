// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Roslynator.Spelling
{
    public class SpellingData
    {
        public static SpellingData Empty { get; } = new SpellingData(
            WordList.Default,
            FixList.Empty,
            WordList.Default_CurrentCulture);

        public SpellingData(
            WordList list,
            FixList fixList,
            WordList ignoreList = null)
        {
            List = list;
            FixList = fixList ?? FixList.Empty;
            IgnoreList = ignoreList ?? WordList.Default_CurrentCulture;
        }

        public WordList List { get; }

        public WordList IgnoreList { get; }

        public FixList FixList { get; }

        public SpellingData AddWords(IEnumerable<string> values)
        {
            WordList newList = List.AddValues(values);

            return new SpellingData(newList, FixList, IgnoreList);
        }

        public SpellingData AddWord(string value)
        {
            return new SpellingData(List.AddValue(value), FixList, IgnoreList);
        }

        public SpellingData AddFix(string error, SpellingFix spellingFix)
        {
            FixList fixList = FixList.Add(error, spellingFix);

            return new SpellingData(List, fixList, IgnoreList);
        }

        public SpellingData AddIgnoredValue(string value)
        {
            return new SpellingData(List, FixList, IgnoreList.AddValue(value));
        }

        public SpellingData AddIgnoredValues(IEnumerable<string> values)
        {
            return new SpellingData(List, FixList, IgnoreList.AddValues(values));
        }
    }
}
