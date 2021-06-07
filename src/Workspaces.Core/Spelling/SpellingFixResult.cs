// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Roslynator.Spelling
{
    internal class SpellingFixResult
    {
        public SpellingFixResult(
            string oldValue,
            string newValue,
            FileLinePositionSpan lineSpan) : this(oldValue, newValue, null, null, -1, lineSpan)
        {
        }

        public SpellingFixResult(
            string oldValue,
            string newValue,
            string oldIdentifier,
            string newIdentifier,
            int valueIndex,
            FileLinePositionSpan lineSpan)
        {
            OldValue = oldValue;
            NewValue = newValue;
            OldIdentifier = oldIdentifier;
            NewIdentifier = newIdentifier;
            ValueIndex = valueIndex;
            LineSpan = lineSpan;
        }

        public string OldValue { get; }

        public string NewValue { get; }

        public string OldIdentifier { get; }

        public string NewIdentifier { get; }

        public int ValueIndex { get; }

        public FileLinePositionSpan LineSpan { get; }

        public bool Success => NewValue != null;

        public bool IsSymbol => OldIdentifier != null;
    }
}
