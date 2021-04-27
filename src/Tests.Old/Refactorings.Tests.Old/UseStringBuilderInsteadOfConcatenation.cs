// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class UseStringBuilderInsteadOfConcatenation
    {
        public static void Foo(string x)
        {
            //a
            var s = x + "x" + x + @"x" + $"{x}x" + $@"{x}x" + $"{x,1}x" + $"{x:f}x" + $"{x,1:f}x"; //b

            s = x + "x";

            s += x + "x";

            while (true)
                s = x + "x";
        }
    }
}
