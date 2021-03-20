// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Roslynator.Spelling
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct SpellingFix : IEquatable<SpellingFix>
    {
        public SpellingFix(string value, SpellingFixKind kind)
        {
            Value = value;
            Kind = kind;
        }

        public string Value { get; }

        public SpellingFixKind Kind { get; }

        public bool IsDefault => Value == null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Kind}  {Value}";

        public SpellingFix WithValue(string value) => new SpellingFix(value, Kind);

        public override bool Equals(object obj) => obj is SpellingFix fix && Equals(fix);

        public bool Equals(SpellingFix other) => Value == other.Value;

        public override int GetHashCode() => EqualityComparer<string>.Default.GetHashCode(Value);

        public static bool operator ==(SpellingFix left, SpellingFix right) => left.Equals(right);

        public static bool operator !=(SpellingFix left, SpellingFix right) => !(left == right);
    }
}
