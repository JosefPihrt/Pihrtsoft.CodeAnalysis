// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS1031_TypeExpected
    {
        private static void Foo()
        {
            string s = null;

            s = default();

            DateTime dt = DateTime.MinValue;

            dt = default();
        }
    }
}
