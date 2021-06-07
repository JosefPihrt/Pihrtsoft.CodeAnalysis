// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Roslynator.Spelling
{
    internal sealed class NewWordComparer : EqualityComparer<NewWord>
    {
        public static NewWordComparer Instance { get; } = new NewWordComparer();

        private NewWordComparer()
        {
        }

        public override bool Equals(NewWord x, NewWord y)
        {
            if (object.ReferenceEquals(x, y))
                return true;

            if (x == null)
                return false;

            if (y == null)
                return false;

            return StringComparer.Ordinal.Equals(x.Value, y.Value)
                && x.LineSpan.Equals(y.LineSpan);
        }

        public override int GetHashCode(NewWord obj)
        {
            return obj?.LineSpan.GetHashCode() ?? 0;
        }
    }
}
