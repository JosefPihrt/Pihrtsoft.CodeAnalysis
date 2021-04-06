// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Roslynator.Spelling
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SpellingCapture
    {
        public SpellingCapture(string value, int index, string containingValue = null)
        {
            Value = value;
            Index = index;
            ContainingValue = containingValue;
        }

        public string Value { get; }

        public int Index { get; }

        public int Length => Value.Length;

        public string ContainingValue { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Index}  {Length}  {Value}";
    }
}
