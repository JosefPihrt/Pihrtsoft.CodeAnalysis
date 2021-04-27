// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable RCS1024, RCS1079, CS0535

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class RemoveRedundantBaseInterface
    {
        private static class Yes
        {
            private class Foo2 : ICollection<string>, IEnumerable<string>
            {
                public int Count => throw new NotImplementedException();
                public bool IsReadOnly => throw new NotImplementedException();
                public void Add(string item) => throw new NotImplementedException();
                public void Clear() => throw new NotImplementedException();
                public bool Contains(string item) => throw new NotImplementedException();
                public void CopyTo(string[] array, int arrayIndex) => throw new NotImplementedException();
                public IEnumerator<string> GetEnumerator() => throw new NotImplementedException();
                public bool Remove(string item) => throw new NotImplementedException();
                IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
            }

            private struct Foo3 : IEnumerable<string>, ICollection<string>
            {
                public int Count => throw new NotImplementedException();
                public bool IsReadOnly => throw new NotImplementedException();
                public void Add(string item) => throw new NotImplementedException();
                public void Clear() => throw new NotImplementedException();
                public bool Contains(string item) => throw new NotImplementedException();
                public void CopyTo(string[] array, int arrayIndex) => throw new NotImplementedException();
                public IEnumerator<string> GetEnumerator() => throw new NotImplementedException();
                public bool Remove(string item) => throw new NotImplementedException();
                IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
            }

            private class Foo4 : List<string>, ICollection<string>, IEnumerable<string>
            {
            }

            private class Foo5 : ICollection<string>, IList<string>, IEnumerable<string>
            {
                public int Count => throw new NotImplementedException();
                public bool IsReadOnly => throw new NotImplementedException();
                public void Add(string item) => throw new NotImplementedException();
                public void Clear() => throw new NotImplementedException();
                public bool Contains(string item) => throw new NotImplementedException();
                public void CopyTo(string[] array, int arrayIndex) => throw new NotImplementedException();
                public IEnumerator<string> GetEnumerator() => throw new NotImplementedException();
                public bool Remove(string item) => throw new NotImplementedException();
                IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

                public string this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
                public int IndexOf(string item) => throw new NotImplementedException();
                public void Insert(int index, string item) => throw new NotImplementedException();
                public void RemoveAt(int index) => throw new NotImplementedException();
            }

            private interface FooInterface : IEnumerable<string>, ICollection<string>
            {
            }

            private class FooStruct : ICollection<string>, IEnumerable<string>
            {
                public int Count => throw new NotImplementedException();
                public bool IsReadOnly => throw new NotImplementedException();
                public void Add(string item) => throw new NotImplementedException();
                public void Clear() => throw new NotImplementedException();
                public bool Contains(string item) => throw new NotImplementedException();
                public void CopyTo(string[] array, int arrayIndex) => throw new NotImplementedException();
                public IEnumerator<string> GetEnumerator() => throw new NotImplementedException();
                public bool Remove(string item) => throw new NotImplementedException();
                IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
            }
        }

        private static class No
        {
            private class Foo1 : List<string>
            {
            }

            private class Foo2 : ICollection<string>
            {
                public int Count => throw new NotImplementedException();
                public bool IsReadOnly => throw new NotImplementedException();
                public void Add(string item) => throw new NotImplementedException();
                public void Clear() => throw new NotImplementedException();
                public bool Contains(string item) => throw new NotImplementedException();
                public void CopyTo(string[] array, int arrayIndex) => throw new NotImplementedException();
                public IEnumerator<string> GetEnumerator() => throw new NotImplementedException();
                public bool Remove(string item) => throw new NotImplementedException();
                IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
            }

            private class Foo3 : ICollection<string>, List<string>, IEnumerable<string>
            {
            }

            private class Foo4 : List<object>, IEnumerable<object>
            {
                IEnumerator<object> IEnumerable<object>.GetEnumerator() => throw new NotImplementedException();

                IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
            }

            private class Foo5 : Foo, IFoo
            {
                object IFoo.Property => throw new NotImplementedException();
            }

            private class Foo6 : Foo, IFoo
            {
                event EventHandler IFoo.Event
                {
                    add { throw new NotImplementedException(); }
                    remove { throw new NotImplementedException(); }
                }
            }

            private class Foo7 : Foo, IFoo
            {
                void IFoo.Method() => throw new NotImplementedException();
            }

            private class Foo : IFoo
            {
                public object Property => throw new NotImplementedException();
                public event EventHandler Event;
                public void Method() => throw new NotImplementedException();
            }

            private interface IFoo
            {
                object Property { get; }
                event EventHandler Event;
                void Method();
            }
        }
    }
}
