// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class GenerateCombinedEnumMember
    {
        [Flags]
        private enum Foo : sbyte
        {
            None = 0,
            Alpha = 1,
            Beta = 2,
            Gamma = 4,
            Delta = 8,
        }
    }
}
