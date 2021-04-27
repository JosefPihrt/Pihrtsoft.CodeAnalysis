// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class ReplaceInterpolatedStringWithConcatenationRefactoring
    {
        private static void Foo(string s, int i)
        {
            s = $"\"a{1}{i}b\"";

            s = $@"""a{1}{i}b""";

            s = $"{1}{i}";
            s = $"{i}{1}";

            //n

            s = "a";
            s = $"{s}";
            s = $"a{s,1}";
            s = $"a{s:f}";
            s = $"a{s,1:f}";
        }
    }
}
