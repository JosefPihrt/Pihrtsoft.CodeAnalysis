// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Roslynator.Spelling
{
    internal abstract class SpellingDiagnosticComparer :
        IComparer<SpellingDiagnostic>,
        IEqualityComparer<SpellingDiagnostic>,
        IComparer,
        IEqualityComparer
    {
        public static SpellingDiagnosticComparer FilePathThenSpanStart { get; } = new FilePathThenSpanStartComparer();

        public abstract int Compare(SpellingDiagnostic x, SpellingDiagnostic y);

        public abstract bool Equals(SpellingDiagnostic x, SpellingDiagnostic y);

        public abstract int GetHashCode(SpellingDiagnostic obj);

        public int Compare(object x, object y)
        {
            if (x == y)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            if (x is SpellingDiagnostic a
                && y is SpellingDiagnostic b)
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

            if (x is SpellingDiagnostic a
                && y is SpellingDiagnostic b)
            {
                return Equals(a, b);
            }

            throw new ArgumentException("", nameof(x));
        }

        public int GetHashCode(object obj)
        {
            if (obj == null)
                return 0;

            if (obj is SpellingDiagnostic diagnostic)
                return GetHashCode(diagnostic);

            throw new ArgumentException("", nameof(obj));
        }

        private class FilePathThenSpanStartComparer : SpellingDiagnosticComparer
        {
            public override int Compare(SpellingDiagnostic x, SpellingDiagnostic y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                if (x == null)
                    return -1;

                if (y == null)
                    return 1;

                int result = StringComparer.OrdinalIgnoreCase.Compare(
                    x.FilePath,
                    y.FilePath);

                if (result != 0)
                    return result;

                return x.Span.Start.CompareTo(y.Span.Start);
            }

            public override bool Equals(SpellingDiagnostic x, SpellingDiagnostic y)
            {
                if (object.ReferenceEquals(x, y))
                    return true;

                if (x == null)
                    return false;

                if (y == null)
                    return false;

                return StringComparer.OrdinalIgnoreCase.Equals(
                    x.FilePath,
                    y.FilePath)
                    && x.Span.Start == y.Span.Start;
            }

            public override int GetHashCode(SpellingDiagnostic obj)
            {
                if (obj == null)
                    throw new ArgumentNullException(nameof(obj));

                return Hash.Combine(
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.FilePath),
                    obj.Span.Start);
            }
        }
    }
}
