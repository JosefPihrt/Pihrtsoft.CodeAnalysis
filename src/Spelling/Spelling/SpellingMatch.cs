// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Roslynator.Spelling
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct SpellingMatch
    {
        public SpellingMatch(string value, int index, string parent = null, int parentIndex = 0)
        {
            Value = value;
            Index = index;
            Parent = parent;
            ParentIndex = parentIndex;
        }

        public string Value { get; }

        public int Index { get; }

        public string Parent { get; }

        public int ParentIndex { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Index}  {Value}";
    }
}
