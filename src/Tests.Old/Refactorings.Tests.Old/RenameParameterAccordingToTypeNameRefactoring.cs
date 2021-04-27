// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable CS0219, RCS1163, RCS1164

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class RenameParameterAccordingToTypeNameRefactoring
    {
        private partial class Foo
        {
            public Foo()
            {
            }

            public Foo(Foo foo, Foo value)
            {
                Foo foo2 = null;
            }

            public void Method<foo3>(Foo foo, Foo value)
            {
                Foo foo2 = null;
            }

            public void MethodWithLocalFunction<foo3>(Foo foo)
            {
                Foo foo2 = null;

                void foo5(Foo foo4, Foo value)
                {
                }
            }

            public void MethodWithSimpleLambda<foo3>(Foo foo)
            {
                Foo foo2 = null;

                Func<Foo, Foo> action = f => f;
            }

            public void MethodWithParenthesizedLambda<foo3>(Foo foo)
            {
                Foo foo2 = null;

                Func<Foo, Foo> action = (f) => f;
            }

            public void MethodWithAnonymousMethod<foo3>(Foo foo)
            {
                Foo foo2 = null;

                Func<Foo, Foo> action = delegate (Foo f) { return f; };
            }

            public string this[Foo foo, Foo value]
            {
                get
                {
                    Foo foo2 = null;
                    return null;
                }
            }

            public static explicit operator string(Foo value)
            {
                Foo foo2 = null;
                return null;
            }

            public static Foo operator !(Foo value)
            {
                Foo foo2 = null;
                return new Foo();
            }

            public delegate void DelegateName<foo2>(Foo foo, Foo value);

            public void Method(Foo foo2)
            {
                Foo foo = null;
            }

            partial void PartialMethod(Foo foo, Foo value)
            {
                Foo foo2 = null;
            }
        }

        private partial class Foo
        {
            partial void PartialMethod(Foo foo, Foo value);
        }
    }
}
