// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

#pragma warning disable RCS1023

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class UnconstrainedTypeParameterCheckedForNull
    {
        private static void Foo<T1, T2, T3>()
            where T1 : new()
            where T2 : class
            where T3 : T2
        {
            var x1 = default(T1);
            var x2 = default(T2);
            var x3 = default(T3);

            if (x1 == null) { }

            if (x1 != null) { }

            // n

            if (x2 == null) { }

            if (x3 == null) { }
        }
    }
}
