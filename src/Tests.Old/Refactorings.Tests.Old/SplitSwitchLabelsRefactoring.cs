// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class SplitSwitchLabelsRefactoring
    {
        public static void Foo(RegexOptions options)
        {
            switch (options)
            {
                case RegexOptions.CultureInvariant:
                case RegexOptions.ECMAScript:
                    break;
                case RegexOptions.ExplicitCapture:
                case RegexOptions.IgnoreCase:
                case RegexOptions.IgnorePatternWhitespace:
                    break;
                case RegexOptions.Multiline:
                case RegexOptions.None:
                case RegexOptions.RightToLeft:
                case RegexOptions.Singleline:
                    break;
            }
        }
    }
}
