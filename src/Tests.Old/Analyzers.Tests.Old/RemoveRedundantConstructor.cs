// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class RemoveRedundantConstructor
    {
        private class Base
        {
        }

        private class Foo : Base
        {
            public Foo()
                : base()
            {
            }

            static Foo()
            {
            }
        }

        private class Foo2
        {
            public Foo2()
            {
            }

            public Foo2(object parameter)
            {
            }
        }

        private class Foo3
        {
            protected Foo3()
            {
            }
        }
    }
}
