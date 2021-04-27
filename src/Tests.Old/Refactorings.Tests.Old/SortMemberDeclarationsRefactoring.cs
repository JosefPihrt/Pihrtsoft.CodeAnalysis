// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class SortMemberDeclarationsRefactoring
    {
        private class Foo2
        {
            public object GetValue() => null;

            public string this[int index] => null;

            public string Name { get; }

            public event EventHandler Changed;

        }

        private class Foo
        {
            private class FooClass
            {
            }

            private struct FooStruct
            {
            }

            private interface FooInterface
            {
            }

            private enum FooEnum
            {
                None
            }

            public static Foo operator +(Foo left, Foo right)
            {
                return null;
            }

            public static explicit operator Foo(string value)
            {
                return null;
            }

            public void FooMethod()
            {
            }

            public string this[int index]
            {
                get { return null; }
            }

            public object Value { get; }

            public string Name { get; }

            public string ValueText { get; }

            public int Index { get; }

            public event EventHandler FooChanged;

            public event EventHandler FooChanged2
            {
                add { }
                remove { }
            }

            public delegate void FooDelegate();

            ~Foo()
            {
            }

            public Foo()
            {
            }
        }
    }
}
