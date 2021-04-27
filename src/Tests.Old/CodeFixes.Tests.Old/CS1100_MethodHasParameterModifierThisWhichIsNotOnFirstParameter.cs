// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS1100_MethodHasParameterModifierThisWhichIsNotOnFirstParameter
    {
        private static void Foo(object parameter, this object parameter2)
        {
        }
    }
}
