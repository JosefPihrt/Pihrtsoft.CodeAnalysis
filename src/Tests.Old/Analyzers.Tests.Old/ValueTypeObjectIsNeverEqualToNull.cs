// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

#pragma warning disable RCS1023

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class ValueTypeCheckedForNull
    {
        public static void Foo<T1>() where T1 : struct
        {
            int x1 = 0;
            var x2 = default(T1);
            var x3 = default(DateTime);
            var x4 = default(StructName);
            int? x5 = x1++;

            if (x1 == null) { }
            if (x1 != null) { }

            if (x2 == null) { }
            if (x2 != null) { }

            if (x3 == null) { }
            if (x3 != null) { }

            if (x4 == null) { }
            if (x4 != null) { }

            if (x5 == null) { }
            if (x5 != null) { }
        }

        private struct StructName
        {
        }
    }
}
