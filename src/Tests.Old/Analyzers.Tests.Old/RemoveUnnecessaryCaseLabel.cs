// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class RemoveUnnecessaryCaseLabel
    {
        private static bool Foo()
        {
            string s = "";

            switch (s)
            {
                case "a":
                    return true;
                case "b":
                default:
                    return false;
            }
        }
    }
}
