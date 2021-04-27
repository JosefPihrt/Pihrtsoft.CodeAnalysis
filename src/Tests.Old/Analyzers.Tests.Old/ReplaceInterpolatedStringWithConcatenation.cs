// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1118, RCS1176, RCS1198, RCS1214

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class ReplaceInterpolatedStringWithConcatenation
    {
        public static void Bar()
        {
            string s = null;
            int i = 0;

            string s1 = null;
            string s2 = null;
            string s3 = null;

            s = $"{s1}{s2}";
            s = $"{s1}{s2}{s3}";

            // n

            s = $"{s1}";

            s = $"{i}";
            s = $"{s1,1}";
            s = $"{s1:f} ";
            s = $" {s1}";
            s = $"{s1} ";
            s = $"{s1} {s2}";
            s = $"{s1}{s2} ";
        }
    }
}
