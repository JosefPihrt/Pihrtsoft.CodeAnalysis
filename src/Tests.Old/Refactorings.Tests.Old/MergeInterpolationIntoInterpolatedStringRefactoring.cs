// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class MergeInterpolationIntoInterpolatedStringRefactoring
    {
        public static void Foo()
        {
            string s = null;

            s = $"a{"b"}c";
            s = $"a{"{}"}c";

            s = $@"a{@"b"}c";

            s = $"a{@"b"}c";

            s = $@"a{"b"}c";
        }
    }
}
