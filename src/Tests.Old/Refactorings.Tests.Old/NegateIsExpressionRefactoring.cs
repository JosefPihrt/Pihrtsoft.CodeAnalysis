// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class NegateIsExpressionRefactoring
    {
        public static void Foo()
        {
            object x = null;

            if (x is Entity)
            {
            }

            if ((x is Entity))
            {
            }

            if (!(x is Entity))
            {
            }

            if (x is Entity entity)
            {
            }

            if ((x is Entity entity2))
            {
            }

            if (!(x is Entity entity3))
            {
            }
        }
    }
}
