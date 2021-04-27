// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class ReplaceInterpolatedStringWithStringFormatRefactoring
    {
        public static void Bar()
        {
            string s = null;
            string a = null;
            string b = null;
            string c = null;
            string d = null;

            s = $"a {a} b {b,0} c {c:f} d {d,0:f} {{a}} \"";

            s = $@"a {a} b {b,0} c {c:f} d {d,0:f} {{a}} """;
        }
    }
}
