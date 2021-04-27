// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class InitializeLocalVariableWithDefaultValue
    {
        private static void Foo()
        {
            string s, s2;

            s = s.ToString();

            s2 = s2.ToString();

            int i;

            s = i.ToString();

            StringSplitOptions options;

            s = options.ToString();
        }
    }
}
