// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class FormatConstraintClauses
    {
        private class Foo
        {
            public static void Bar<T1>() where
                T1 : class
            {
            }

            public static void Bar<T1, T2, T3>() where T1 : class where T2 : class where T3 : class
            {
            }
        }

        // n

        private class Foo2<T> //x
            where T : class
        {
        }

        private class Foo2<T, T2>
            where T : class //x
            where T2 : class
        {
        }
    }
}
