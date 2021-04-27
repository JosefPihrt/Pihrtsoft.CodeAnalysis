// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using static System.Math;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class InlineUsingStaticRefactoring
    {
        private static void Foo()
        {
            var max = Max(1, 2);
            var min = Min(1, 2);
        }
    }
}
