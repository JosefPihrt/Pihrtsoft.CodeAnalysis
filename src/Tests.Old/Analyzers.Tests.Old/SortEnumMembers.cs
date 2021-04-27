// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class SortEnumMembers
    {
        private enum FooEnum
        {
            D = 4,
            A = 1,
            B,
            C = 3
        }

        private enum FooEnum2
        {
            D = 4,
            A = 1,
            B,
            C = x
        }

        private enum SortedEnum
        {
            A,
            B,
            C,
            D
        }

        private enum SortedEnum2
        {
            A = 1,
            B = 2,
            C,
            D = 3
        }
    }
}
