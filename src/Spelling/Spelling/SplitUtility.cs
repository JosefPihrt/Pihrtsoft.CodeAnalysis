// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace Roslynator.Spelling
{
    public static class SplitUtility
    {
        private const string _splitCasePattern = @"
    (?<=
        \p{Lu}
    )
    (?=
        \p{Lu}\p{Ll}
    )
|
    (?<=
        \p{Ll}
    )
    (?=
        \p{Lu}
    )
";

        private static readonly Regex _splitCaseRegex = new Regex(
            _splitCasePattern,
            RegexOptions.IgnorePatternWhitespace);

        public static readonly Regex SplitIdentifierRegex = new Regex(
            @"\P{L}+|" + _splitCasePattern,
            RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex _splitHyphenRegex = new Regex("-");

        private static readonly Regex _splitCaseAndHyphenRegex = new Regex(
            "-|" + _splitCasePattern,
            RegexOptions.IgnorePatternWhitespace);

        public static Regex GetSplitRegex(SplitMode splitMode)
        {
            return splitMode switch
            {
                SplitMode.None => null,
                SplitMode.Case => _splitCaseRegex,
                SplitMode.Hyphen => _splitHyphenRegex,
                SplitMode.CaseAndHyphen => _splitCaseAndHyphenRegex,
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
