// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0501_MemberMustDeclareBodyBecauseItIsNotMarkedAbstractExternOrPartial
    {
        private abstract class Foo
        {
            public void Method();

            public Foo(object parameter);

            ~Foo();

            public string this[int index] { get; set; }

            public static explicit operator Foo(string value);

            public static Foo operator !(Foo value);
        }
    }
}
