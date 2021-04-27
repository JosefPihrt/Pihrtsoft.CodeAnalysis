// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class ExtractDeclarationFromUsingStatement
    {
        private static void Bar()
        {
            using (var x1 = new object())
            {
                Bar();
            }

            using (var x2 = new object())
            {
                Bar();
                Bar();
            }

            using (var x3 = new object())
                Bar();
        }
    }
}
