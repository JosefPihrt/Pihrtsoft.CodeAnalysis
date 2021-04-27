// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0660_TypeDefinesEqualityOperatorButDoesNotOverrideObjectEquals
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
