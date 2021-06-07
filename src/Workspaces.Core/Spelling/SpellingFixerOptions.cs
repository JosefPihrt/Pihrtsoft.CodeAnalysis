// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.Spelling
{
    internal class SpellingFixerOptions
    {
        public static SpellingFixerOptions Default { get; } = new SpellingFixerOptions();

        public SpellingFixerOptions(
            SpellingScopeFilter scopeFilter = SpellingScopeFilter.All,
            VisibilityFilter symbolVisibility = VisibilityFilter.All,
            SplitMode splitMode = SplitMode.CaseAndHyphen,
            int minWordLength = 3,
            int codeContext = 1,
            bool includeGeneratedCode = false,
            bool autoFix = true,
            bool interactive = false,
            bool dryRun = false)
        {
            if (codeContext < 0)
                throw new ArgumentOutOfRangeException(nameof(codeContext), codeContext, "");

            ScopeFilter = scopeFilter;
            SymbolVisibility = symbolVisibility;
            SplitMode = splitMode;
            MinWordLength = minWordLength;
            CodeContext = codeContext;
            IncludeGeneratedCode = includeGeneratedCode;
            AutoFix = autoFix;
            Interactive = interactive;
            DryRun = dryRun;
        }

        public SpellingScopeFilter ScopeFilter { get; }

        public VisibilityFilter SymbolVisibility { get; }

        public SplitMode SplitMode { get; }

        public int MinWordLength { get; }

        public int CodeContext { get; }

        public bool IncludeGeneratedCode { get; }

        public bool AutoFix { get; }

        public bool Interactive { get; }

        public bool DryRun { get; }
    }
}
