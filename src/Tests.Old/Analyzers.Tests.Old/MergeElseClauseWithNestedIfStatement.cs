// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1002, RCS1004, RCS1118, RCS1176

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class MergeElseClauseWithNestedIfStatement
    {
        private static void Foo()
        {
            bool f = false;

            if (f)
            {
                Foo();
            }
            else
            {
                if (f)
                {
                    Foo();
                }
            }

            if (f)
            {
                Foo();
            }
            else
            {
                if (f)
                {
                    Foo();
                }
                else
                {
                    Foo();
                }
            }

            if (f)
            {
                Foo();
            }
            else
            {
                if (f)
                {
                    Foo();
                }
                else if (f)
                {
                    Foo();
                }
            }

            // n

            if (f)
            {
                Foo();
            }
            else
            {
                if (f)
                {
                    Foo();
                }

                Foo();
            }

            if (f)
            {
                Foo();
            }
            else
            {
                if (f)
                {
                    Foo();
                }
                else
                {
                    Foo();
                }

                Foo();
            }

            if (f)
            {
                Foo();
            }
            else
            {
                if (f)
                {
                    Foo();
                }
                else if (f)
                {
                    Foo();
                }

                Foo();
            }
        }
    }
}
