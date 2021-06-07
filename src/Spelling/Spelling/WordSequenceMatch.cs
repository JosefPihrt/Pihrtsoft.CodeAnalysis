// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Roslynator.Spelling
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct WordSequenceMatch
    {
        public WordSequenceMatch(WordSequence sequence, int index, int length)
        {
            Sequence = sequence;
            Index = index;
            Length = length;
        }

        public WordSequence Sequence { get; }

        public int Index { get; }

        public int Length { get; }

        internal int EndIndex => Index + Length;

        public bool IsDefault => Sequence.IsDefault;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => (IsDefault) ? "Uninitialized" : Sequence.ToString();
    }
}
