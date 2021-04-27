// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0266_CannotImplicitlyConvertTypeExplicitConversionExists
    {
        public static void Foo()
        {
            int i = Foo<int>();
            char ch = Foo<char>();
            DateTime dt = Foo<DateTime>();

            bool f = Foo<bool>();
        }

        public static IEnumerable<string> FooEnumerableOfT(string item)
        {
            yield return item;

            yield return FooEnumerableOfT(item);
        }

        private static T? Foo<T>() where T : struct
        {
            return null;
        }
    }
}
