// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class InsertInterpolationIntoStringLiteralRefactoring
    {
        private class Foo
        {
            public static void Bar(string s)
            {
                s = "x\tx\u1234x\U00001234x\x9x\x09x\x009x{}xFoox\u0046oo";
                s = @"x\tx\u1234x\U00001234x\x9x\x09x\x009x{}x""xFoox\u0046oo";

                s = $"x\tx\u1234x\U00001234x\x9x\x09x\x009x{{}}x{s}xFoox\u0046oo";
                s = $@"x\tx\u1234x\U00001234x\x9x\x09x\x009x{{}}x""x{s}xFoox\u0046oo";
            }
        }

        //n

        [DebuggerDisplay("", Name = "")]
        private class Foo2
        {
            public static string Bar(string parameter = "")
            {
                const string s = "";

                switch (parameter)
                {
                    case "":
                        break;
                }

                return s;
            }
        }
    }
}
