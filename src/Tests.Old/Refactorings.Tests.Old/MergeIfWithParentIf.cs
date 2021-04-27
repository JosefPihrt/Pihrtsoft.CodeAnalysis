// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1118

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class MergeIfWithParentIf
    {
        public static void Foo()
        {
            bool x = false;
            bool y = false;

            if (x)
            {
                if (y)
                {
                    Foo();
                }
            }

            if (x)
            {
                if (y)
                    Foo();
            }

            if (x)
                if (y)
                {
                    Foo();
                }

            if (x)
                if (y)
                    Foo();

            if (x || x)
            {
                if (y)
                {
                    Foo();
                }
            }

            if (x)
            {
                if (y || y)
                {
                    Foo();
                }
            }
        }
    }
}
