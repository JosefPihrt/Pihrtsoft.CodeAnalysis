// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1118

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class SplitIfElseRefactoring
    {
        internal static bool Foo()
        {
            bool condition1 = false;
            bool condition2 = false;

            if (condition1)
            {
                return Bar1();
            }
            else if (condition2)
            {
                Foo();
                return Bar2();
            }
            else
            {
                Foo();
                Foo();
                return false;
            }
        }

        internal static bool Foo2()
        {
            bool condition1 = false;
            bool condition2 = false;
            bool condition3 = false;

            if (condition1)
                return Bar1();
            else if (condition2)
                return Bar2();
            else if (condition3)
                return false;

            return false;
        }

        private static bool Bar1()
        {
            return false;
        }

        private static bool Bar2()
        {
            return false;
        }

        //n

        internal static bool Foo3()
        {
            bool condition = false;

            if (condition)
            {
                return false;
            }
            else
            {
                Foo3();
            }

            if (condition)
            {
                Foo3();
            }
            else
            {
                return false;
            }

            return false; 
        }
    }
}
