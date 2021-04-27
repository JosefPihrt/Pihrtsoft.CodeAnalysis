// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class AddBracesToSwitchSectionWithMultipleStatements
    {
        public static void Foo()
        {
            var options = RegexOptions.None;

            switch (options)
            {
                case RegexOptions.CultureInvariant:
                    Foo();
                    break;
                case RegexOptions.ECMAScript:
                case RegexOptions.ExplicitCapture:
                    Foo();
                    Foo();
                    break;
                case RegexOptions.IgnoreCase:
                    break;
                case RegexOptions.IgnorePatternWhitespace:
                case RegexOptions.Multiline:
                case RegexOptions.None:
                case RegexOptions.RightToLeft:
                case RegexOptions.Singleline:
                    break;
            }
        }
    }
}
