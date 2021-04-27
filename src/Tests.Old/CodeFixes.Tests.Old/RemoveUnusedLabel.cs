// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class RemoveUnusedLabel
    {
        private static void Foo()
        {
            object x = null;

            Label:
            Foo();
        }
    }
}
