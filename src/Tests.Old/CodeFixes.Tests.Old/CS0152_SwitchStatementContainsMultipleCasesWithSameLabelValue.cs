// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0152_SwitchStatementContainsMultipleCasesWithSameLabelValue
    {
        private class Foo
        {
            public static void Bar()
            {
                RegexOptions options = RegexOptions.None;

                switch (options)
                {
                    case RegexOptions.Compiled:
                        break;
                    case RegexOptions.CultureInvariant:
                        break;
                    case RegexOptions.ECMAScript:
                    case RegexOptions.ECMAScript:
                        break;
                    case RegexOptions.ExplicitCapture:
                        break;
                    case RegexOptions.IgnoreCase:
                    case RegexOptions.ExplicitCapture:
                        break;
                    case RegexOptions.IgnorePatternWhitespace:
                        break;
                    case RegexOptions.Multiline:
                    case RegexOptions.Singleline:
                        break;
                    case RegexOptions.None:
                        break;
                    case RegexOptions.RightToLeft:
                    case RegexOptions.None:
                    case RegexOptions.None:
                        break;
                    case RegexOptions.Singleline:
                    case RegexOptions.Multiline:
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
