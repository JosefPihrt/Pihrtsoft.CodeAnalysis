// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1001, RCS1002, RCS1040, RCS1118, RCS1176

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class MergeIfStatementWithNestedIfStatement
    {
        public static void Foo()
        {
            bool condition = false;
            bool condition2 = false;

            if (condition)
            {
                if (condition2 && condition2)
                {
                    Foo();
                }
            }

            if (condition)
            {
                if (condition2)
                    Foo();
            }

            if (condition)
                if (condition2)
                {
                    Foo();
                }

            if (condition)
                if (condition2)
                    Foo();

            // n

            if (condition || condition)
            {
                if (condition2)
                {
                    Foo();
                }
            }

            if (condition)
            {
                if (condition2 || condition2)
                {
                    Foo();
                }
            }

            if (condition)
            {
                if (condition2)
                {
                }
            }
            else
            {
            }

            if (condition)
            {
                if (condition2)
                {
                }
                else
                {
                }
            }
        }
    }
}
