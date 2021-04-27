// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class ImplementIEquatableOfTRefactoring
    {
        private class Foo<T>
        {
        }

        private class Foo2<T> :
        {
        }

        private class Foo3<T> : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        private struct FooStruct<T>
        {
        }

        private struct FooStruct2<T> :
        {
        }

        private class FooStruct3<T> : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}
