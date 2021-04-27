// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class AddBreakStatementToSwitchSection
    {
        public static void Foo(string s)
        {
            switch (s)
            {
                case "a":
                    {
                    }
                case "b":
                    Foo(s);
                default:
                    {
                    }
            }
        }
    }
}
