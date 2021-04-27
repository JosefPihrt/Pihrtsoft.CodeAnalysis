// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class ChangeTypeOfLocalVariable
    {
        private static void FooVoid()
        {
            var x = FooVoid;

            Action<object> x2 = FooVoid;
        }

        private static void FooVoid(int parameter)
        {
        }

        private static void FooVoid2(object parameter)
        {
            var x = FooVoid2;

            Action x2 = FooVoid2;
        }

        private static void FooVoid3(object parameter, Dictionary<object, DateTime> dictionary)
        {
            var x = FooVoid3;

            Action x2 = FooVoid3;
        }

        private static string Foo()
        {
            var x = Foo;

            Func<int> x2 = Foo;

            return null;
        }

        private static string Foo2(object parameter)
        {
            var x = Foo2;

            Func<object> x2 = Foo2;

            return null;
        }

        private static string Foo3(object parameter, Dictionary<object, DateTime> dictionary)
        {
            var x = Foo3;

            Func<object> x2 = Foo3;

            return null;
        }
    }
}
