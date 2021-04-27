// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class AddMemberToInterface
    {
        public interface IFoo
        {
            void BarExplicit();
            void Bar();
        }

        public interface IFoo2
        {
            void BarExplicit2();
        }

        public interface IFoo3
        {
        }

        public interface IFoo<T>
        {
            void Bar<T2>();
        }

        public interface IFoo2<T>
        {
        }

        private class Foo<T> : BaseFoo, IFoo, IFoo2, IFoo<T>, IFoo2<T>, IDisposable
        {
            public void Bar() { }

            public string FooProperty { get; }

            public string FooProperty2 => null;

            public string FooProperty3
            {
                get { return null; }
                set { value = null; }
            }

            public string this[int index]
            {
                get { return null; }
            }

            public event EventHandler<EventArgs> FooEvent;

            public event EventHandler<EventArgs> FooEvent2
            {
                add { }
                remove { }
            }

            public void Bar(string s) { }

            public void Bar<T2>() { }

            void IFoo.BarExplicit() { }

            void IFoo2.BarExplicit2() { }

            public void Dispose() { }
        }

        private class Foo : BaseFoo, IFoo, IFoo2, IFoo<string>, IFoo2<string>, IDisposable
        {
            public void Bar() { }

            public void Bar(string s) { }

            public void Bar<T2>() { }

            void IFoo.BarExplicit() { }

            void IFoo2.BarExplicit2() { }

            public void Dispose() { }
        }

        private class Foo2 : Roslynator.CSharp.Tests.IFoo
        {
            public void Bar()
            {
            }

            public void Bar(string s)
            {
            }
        }

        private class BaseFoo
        {
        }
    }
}
