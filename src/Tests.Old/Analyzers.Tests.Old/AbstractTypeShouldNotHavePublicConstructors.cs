// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class AbstractTypeShouldNotHavePublicConstructors
    {
        public abstract class Foo
        {
            public Foo()
            {
            }

            protected internal Foo(object parameter)
            {
            }

            internal Foo(object parameter, object parameter2)
            {
            }

            protected Foo(object parameter1, object parameter2, object parameter3)
            {
            }
        }

        public class Foo2
        {
            public Foo2()
            {
            }

            internal Foo2(object parameter)
            {
            }

            protected Foo2(object parameter1, object parameter2)
            {
            }
        }
    }
}
