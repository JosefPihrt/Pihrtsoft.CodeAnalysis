// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class ReorderTypeParameterConstraints
    {
        private class Foo<T1, T2, T3, T4, T5>
            where T5 : class //T5
            where T4 : class //T4
            where T3 : class //T3
            where T2 : class //T2
            where T1 : class //T1
        {
        }

        private class Foo<T1, T2, T3, T4, T5>
            where T4 : class
            where T3 : class
            where T2 : class
            where T1 : class
        {
        }

        private class Foo<T1, T2, T3, T4, T5>
            where T5 : class
            where T4 : class
            where T3 : class
            where T2 : class
        {
        }

        private class Foo<T1, T2, T3, T4, T5>
            where T4 : class
            where T2 : class
            where T3 : class
        {
        }

        // n

        private class Foo<T1, T2, T3, T4, T5>
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
        }
    }
}
