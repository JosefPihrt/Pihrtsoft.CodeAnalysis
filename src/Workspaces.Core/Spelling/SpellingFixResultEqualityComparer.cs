// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Roslynator.Spelling
{
    internal abstract class SpellingFixResultEqualityComparer :
        IEqualityComparer<SpellingFixResult>,
        IEqualityComparer
    {
        public static SpellingFixResultEqualityComparer OldValueAndNewValue { get; } = new ValueAndFixedValueComparer();

        public abstract bool Equals(SpellingFixResult x, SpellingFixResult y);

        public abstract int GetHashCode(SpellingFixResult obj);

        new public bool Equals(object x, object y)
        {
            if (x == y)
                return true;

            if (x == null)
                return false;

            if (y == null)
                return false;

            if (x is SpellingFixResult a
                && y is SpellingFixResult b)
            {
                return Equals(a, b);
            }

            throw new ArgumentException("", nameof(x));
        }

        public int GetHashCode(object obj)
        {
            if (obj == null)
                return 0;

            if (obj is SpellingFixResult result)
                return GetHashCode(result);

            throw new ArgumentException("", nameof(obj));
        }

        private class ValueAndFixedValueComparer : SpellingFixResultEqualityComparer
        {
            public override bool Equals(SpellingFixResult x, SpellingFixResult y)
            {
                if (object.ReferenceEquals(x, y))
                    return true;

                if (x == null)
                    return false;

                if (y == null)
                    return false;

                return StringComparer.CurrentCulture.Equals(x.OldValue, y.OldValue)
                    && StringComparer.CurrentCulture.Equals(x.NewValue, y.NewValue);
            }

            public override int GetHashCode(SpellingFixResult obj)
            {
                if (obj == null)
                    throw new ArgumentNullException(nameof(obj));

                return Hash.Combine(
                    StringComparer.CurrentCulture.GetHashCode(obj.OldValue),
                    StringComparer.CurrentCulture.GetHashCode(obj.NewValue));
            }
        }
    }
}
