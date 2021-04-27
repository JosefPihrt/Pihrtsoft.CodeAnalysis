// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class ReplaceNullLiteralExpressionWithDefaultExpression
    {
        private static void Foo()
        {
            DateTime dt = null;

            dt = null;

            int i = null;

            i = null;
        }
    }
}
