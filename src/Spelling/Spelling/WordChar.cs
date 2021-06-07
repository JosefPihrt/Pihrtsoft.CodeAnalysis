// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Roslynator.Spelling
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct WordChar : IEquatable<WordChar>
    {
        public WordChar(char value, int index)
        {
            Value = value;
            Index = index;
        }

        public char Value { get; }

        public int Index { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Value}  {(int)Value}  {Index}";

        public static WordChar Create(string value, int index)
        {
            return new WordChar(value[index], index);
        }

        public override bool Equals(object obj)
        {
            return obj is WordChar wordChar
                && Equals(wordChar);
        }

        public bool Equals(WordChar other)
        {
            return Value == other.Value
                && Index == other.Index;
        }

        public override int GetHashCode() => Hash.Combine(Value, Hash.Combine(Index));

        public static bool operator ==(WordChar left, WordChar right) => left.Equals(right);

        public static bool operator !=(WordChar left, WordChar right) => !(left == right);
    }
}
