// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal sealed class DiagnosticDeepEqualityComparer : IEqualityComparer<Diagnostic>
    {
        public static DiagnosticDeepEqualityComparer Instance { get; } = new DiagnosticDeepEqualityComparer();

        internal static bool Equals(ImmutableArray<Diagnostic> first, ImmutableArray<Diagnostic> second)
        {
            return first.Length == second.Length
                && first.Intersect(second, Instance).Count() == first.Length;
        }

        private DiagnosticDeepEqualityComparer()
        {
        }

        public bool Equals(Diagnostic x, Diagnostic y)
        {
            if (object.ReferenceEquals(x, y))
                return true;

            if (x == null)
                return false;

            if (y == null)
                return false;

            if (!x.Descriptor.Equals(y.Descriptor))
                return false;

            if (!x.Location.GetLineSpan().Equals(y.Location.GetLineSpan()))
                return false;

            if (x.Severity != y.Severity)
                return false;

            if (x.WarningLevel != y.WarningLevel)
                return false;

            return true;
        }

        public int GetHashCode(Diagnostic obj)
        {
            if (obj == null)
                return 0;

            return Hash.Combine(
                obj.Descriptor,
                Hash.Combine(
                    obj.Location.GetLineSpan().GetHashCode(),
                    Hash.Combine((int)obj.Severity, obj.WarningLevel)));
        }
    }
}
