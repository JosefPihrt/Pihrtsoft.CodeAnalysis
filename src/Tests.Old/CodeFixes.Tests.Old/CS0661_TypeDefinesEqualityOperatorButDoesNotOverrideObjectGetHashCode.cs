// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0661_TypeDefinesEqualityOperatorButDoesNotOverrideObjectGetHashCode
    {
        private class Foo
        {
            public static bool operator ==(Foo left, Foo right)
            {
                return false;
            }

            public static bool operator !=(Foo left, Foo right)
            {
                return !(left == right);
            }
        }
    }
}
