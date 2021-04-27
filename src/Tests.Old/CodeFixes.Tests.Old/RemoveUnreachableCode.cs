// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable CS0219

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class RemoveUnreachableCode
    {
        private static object Foo()
        {
            return null;
            Foo();
        }

        private static object Foo2()
        {
            return null;
            Foo2();
            Foo2();
        }

        private static void WhenTrue()
        {
            bool value = true;
            const bool constTrue = true;

            if (constTrue)
            {
                WhenTrue();
            }
            else
            {
                WhenFalse();
            }

            if (constTrue)
                WhenTrue();
            else
                WhenFalse();

            if (constTrue)
            {
                WhenTrue();
            }
            else if (value)
            {
                WhenFalse();
            }

            if (constTrue)
                WhenTrue();
            else if (value)
                WhenFalse();

            if (value)
            {
                Foo();
            }
            else if (constTrue)
            {
                WhenTrue();
            }
            else if (value)
            {
                WhenFalse();
            }

            if (value)
                Foo();
            else if (constTrue)
                WhenTrue();
            else if (value)
                WhenFalse();
        }

        private static void WhenFalse()
        {
            bool value = true;
            const bool constFalse = false;

            if (constFalse)
            {
                WhenFalse();
            }

            if (constFalse)
                WhenFalse();

            if (constFalse)
            {
                WhenFalse();
            }
            else
            {
                WhenTrue();
            }

            if (constFalse)
                WhenFalse();
            else
                WhenTrue();

            if (constFalse)
            {
                WhenFalse();
            }
            else if (value)
            {
                Foo();
            }

            if (constFalse)
                WhenFalse();
            else if (value)
                Foo();
        }
    }
}
