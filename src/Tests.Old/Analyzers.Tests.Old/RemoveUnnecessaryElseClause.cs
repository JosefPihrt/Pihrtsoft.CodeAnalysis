// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

#pragma warning disable CS0162, RCS1001, RCS1004

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class RemoveUnnecessaryElseClause
    {
        private static object Foo()
        {
            bool f = false;

            if (f)
            {
                return WhenTrue();
            }
            else
            {
                return WhenFalse();
            }

            if (f)
                return WhenTrue();
            else
                return WhenFalse();

            if (f)
            {
                return WhenTrue();
            }
            else
            {
            }

            if (f)
                if (f)
                {
                    return WhenTrue();
                }
                else
                {
                    return WhenFalse();
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
