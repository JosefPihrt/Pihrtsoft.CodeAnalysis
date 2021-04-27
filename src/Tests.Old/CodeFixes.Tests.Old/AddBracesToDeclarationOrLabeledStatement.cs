// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class AddBracesToDeclarationOrLabeledStatement
    {
        private static void Foo()
        {
            bool f = false;

            if (f)
                string s = "";

            if (f)
                Label: Foo();

        }
    }
}
