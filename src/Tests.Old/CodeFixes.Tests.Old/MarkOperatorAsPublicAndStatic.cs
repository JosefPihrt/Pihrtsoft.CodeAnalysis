// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class MarkOperatorAsPublicAndStatic
    {
        private class Foo
        {
            implicit operator string(Foo foo)
            {
                return "";
            }

            public implicit operator int(Foo foo)
            {
                return 0;
            }

            static implicit operator long(Foo foo)
            {
                return 0;
            }

            internal implicit operator uint(Foo foo)
            {
                return 0;
            }

            internal static implicit operator ulong(Foo foo)
            {
                return 0;
            }
        }
    }
}
