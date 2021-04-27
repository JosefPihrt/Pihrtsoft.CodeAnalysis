// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class RemoveEmptyDestructor
    {
        public class Foo
        {
            ~Foo()
            {
            }
        }

        public class Foo2
        {
            ~Foo2()
            {
#if DEBUG
            }
#endif
        }

        public class Foo3
        {
            ~Foo3()
            {
                object x = null;
            }
        }
    }
}
