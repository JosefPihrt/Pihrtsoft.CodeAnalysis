// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class ReplaceHexadecimalLiteralWithDecimalLiteralRefactoring
    {
        public static void Foo()
        {
            int i = 0x010;
            uint u = 0x010u;
            long l = 0x010l;
            ulong ul = 0x010ul;
        }
    }
}
