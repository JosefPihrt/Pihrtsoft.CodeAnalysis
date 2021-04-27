// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class AddBaseConstructorsRefactoring
    {
        internal class Foo<T> : FooBase<T>
        {
        }

        internal class FooBase<T>
        {
            public FooBase(IEnumerable<T> values = null, int x = 1, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
            {
            }
        }

        internal class FooDictionary<TKey> : Dictionary<TKey, IEnumerable<DateTime>>
        {
        }

        internal class FooException : Exception
        {
        }
    }
}
