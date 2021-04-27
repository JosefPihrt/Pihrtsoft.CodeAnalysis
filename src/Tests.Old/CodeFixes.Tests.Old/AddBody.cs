// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class AddBody
    {
        private partial class Foo
        {
            partial void PartialMethod();
        }

        private partial class Foo
        {
            partial void PartialMethod();

            public void Method();

            public Foo(object parameter);

            ~Foo();

            public string this[int index] { get; set; }

            public static explicit operator Foo(string value);

            public static Foo operator !(Foo value);

            public void MethodWithLocalFunction()
            {
                void Local(string value);
            }
        }
    }
}
