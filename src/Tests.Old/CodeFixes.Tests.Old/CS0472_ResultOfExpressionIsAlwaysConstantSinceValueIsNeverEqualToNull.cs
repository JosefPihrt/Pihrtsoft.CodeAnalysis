// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0472_ResultOfExpressionIsAlwaysConstantSinceValueIsNeverEqualToNull
    {
        private static void Foo()
        {
            object x = null;

            int i = 0;

            if (i != null)
            {
                WhenTrue();
            }
            else
            {

            }

            if (i != null)
                WhenTrue();

            if (i == null)
            {
                WhenTrue();
            }

            if (i == null)
            {
                WhenTrue();
            }
            else
            {
                WhenFalse();
            }

            do
            {
                Foo();
            } while (i != null);

            do
            {
                Foo();
            } while (i == null);

            while (i != null)
            {
                Foo();
            }

            while (i == null)
            {
                Foo();
            }

            var list = new List<string>();

            for (int j = 0; i != null; j++)
            {
            }

            for (int j = 0; i == null; j++)
            {
            }

            x = (i != null) ? WhenTrue() : WhenFalse();

            x = (i == null) ? WhenTrue() : WhenFalse();

            if (i != null && i.ToString() == "0")
            {
            }

            if (i == null && i.ToString() == "0")
            {
                WhenTrue();
            }
            else
            {
                WhenFalse();
            }

            if (i != null || i.ToString() == "0")
            {
                WhenTrue();
            }
            else
            {
                WhenFalse();
            }

            if (i == null || i.ToString() == "0")
            {
            }
        }

        private static object WhenTrue()
        {
            return null;
        }

        private static object WhenFalse()
        {
            return null;
        }
    }
}
