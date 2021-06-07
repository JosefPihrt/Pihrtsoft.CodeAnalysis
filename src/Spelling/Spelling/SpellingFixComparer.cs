// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Roslynator.Spelling
{
    public abstract class SpellingFixComparer :
        IComparer<SpellingFix>,
        IEqualityComparer<SpellingFix>,
        IComparer,
        IEqualityComparer
    {
        public static SpellingFixComparer InvariantCulture { get; } = new InvariantCultureSpellingFixComparer();

        public static SpellingFixComparer InvariantCultureIgnoreCase { get; } = new InvariantCultureIgnoreCaseSpellingFixComparer();

        public abstract int Compare(SpellingFix x, SpellingFix y);

        public abstract bool Equals(SpellingFix x, SpellingFix y);

        public abstract int GetHashCode(SpellingFix obj);

        public int Compare(object x, object y)
        {
            if (x == y)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            if (x is SpellingFix a
                && y is SpellingFix b)
            {
                return Compare(a, b);
            }

            throw new ArgumentException("", nameof(x));
        }

        new public bool Equals(object x, object y)
        {
            if (x == y)
                return true;

            if (x == null)
                return false;

            if (y == null)
                return false;

            if (x is SpellingFix a
                && y is SpellingFix b)
            {
                return Equals(a, b);
            }

            throw new ArgumentException("", nameof(x));
        }

        public int GetHashCode(object obj)
        {
            if (obj == null)
                return 0;

            if (obj is SpellingFix type)
                return GetHashCode(type);

            throw new ArgumentException("", nameof(obj));
        }

        private class InvariantCultureSpellingFixComparer : SpellingFixComparer
        {
            public override int Compare(SpellingFix x, SpellingFix y)
            {
                return StringComparer.CurrentCulture.Compare(x.Value, y.Value);
            }

            public override bool Equals(SpellingFix x, SpellingFix y)
            {
                return StringComparer.CurrentCulture.Equals(x.Value, y.Value);
            }

            public override int GetHashCode(SpellingFix obj)
            {
                return StringComparer.CurrentCulture.GetHashCode(obj.Value);
            }
        }

        private class InvariantCultureIgnoreCaseSpellingFixComparer : SpellingFixComparer
        {
            public override int Compare(SpellingFix x, SpellingFix y)
            {
                return WordList.DefaultComparer.Compare(x.Value, y.Value);
            }

            public override bool Equals(SpellingFix x, SpellingFix y)
            {
                return WordList.DefaultComparer.Equals(x.Value, y.Value);
            }

            public override int GetHashCode(SpellingFix obj)
            {
                return WordList.DefaultComparer.GetHashCode(obj.Value);
            }
        }
    }
}
