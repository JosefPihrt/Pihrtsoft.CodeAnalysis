// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Roslynator.Spelling
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct WordSequence
    {
        public WordSequence(ImmutableArray<string> words)
        {
            if (words.IsDefault
                || words.Length <= 1)
            {
                throw new ArgumentException("", nameof(words));
            }

            Words = words;
        }

        public string First => Words[0];

        public int Count => Words.Length;

        public ImmutableArray<string> Words { get; }

        public bool IsDefault => Words.IsDefault;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => (IsDefault) ? "Uninitialized" : $"Count = {Count}  {ToString()}";

        public override string ToString()
        {
            return (Words.IsDefault) ? "" : string.Join(" ", Words);
        }
    }
}
