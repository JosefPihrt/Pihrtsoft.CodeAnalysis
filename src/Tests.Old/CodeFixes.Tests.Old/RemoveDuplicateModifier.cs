// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class RemoveDuplicateModifier
    {
        private private class Foo
        {
            public static static implicit operator string(Foo foo)
            {
                return "";
            }

            public public static implicit operator int(Foo foo)
            {
                return 0;
            }
        }
    }
}
