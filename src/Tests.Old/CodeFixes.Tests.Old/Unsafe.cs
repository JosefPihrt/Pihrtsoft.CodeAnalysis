// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal unsafe struct Unsafe
    {
        public static void Foo()
        {
            // pointer type
            char* pCh = null;

            // fixed statement
            fixed (char* pStart = "")
            {
            }

            // pointer indirection expression
            char ch = *pCh;

            // addressof expression
            pCh = &ch;

            // stackalloc array creation
            char* pStart2 = stackalloc char[100];

            var x = default(Unsafe);
            var px = &x;

            // pointer member access expression
            px->Value = 25;
        }

        public int Value { get; set; }
    }
}
