// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.Spelling
{
    public readonly struct SpellingMatch
    {
        public SpellingMatch(string value, int index)
        {
            Value = value;
            Index = index;
        }

        public string Value { get; }

        public int Index { get; }
    }
}
