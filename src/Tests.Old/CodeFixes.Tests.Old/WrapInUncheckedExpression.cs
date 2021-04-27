// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class WrapInUncheckedExpression
    {
        private const sbyte Bar = unchecked((sbyte)0xee);

        private static void Foo()
        {
            byte x = (byte)Bar;
        }
    }
}
