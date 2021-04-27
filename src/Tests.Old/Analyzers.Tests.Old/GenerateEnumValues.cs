// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class GenerateEnumValues
    {
        [Flags]
        private enum Foo2 : sbyte
        {
            None = 0,
            A = 1,
            B,
            C = 16,
            D,
            E = 8,
            F
        }
    }
}
