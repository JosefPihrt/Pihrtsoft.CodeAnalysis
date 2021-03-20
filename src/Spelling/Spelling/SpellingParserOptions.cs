// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.Spelling
{
    public class SpellingParserOptions
    {
        public static SpellingParserOptions Default { get; } = new SpellingParserOptions();

        public SpellingParserOptions(
            SplitMode splitMode = SplitMode.None,
            int minWordLength = 3)
        {
            SplitMode = splitMode;
            MinWordLength = minWordLength;
        }

        public SplitMode SplitMode { get; }

        public int MinWordLength { get; }
    }
}
