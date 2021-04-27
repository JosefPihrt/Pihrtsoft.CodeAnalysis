// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable CS0219, RCS1003, RCS1004, RCS1007, RCS1118, RCS1126, RCS1176

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class ReplaceIfStatementWithAssignment
    {
        public static void Foo(bool condition, bool f)
        {
            if (condition)
            {
                f = true;
            }
            else
            {
                f = false;
            }

            if (condition)
                f = true;
            else
            {
                f = false;
            }

            if (condition)
            {
                f = true;
            }
            else
                f = false;

            if (condition)
                f = true;
            else
                f = false;

            if (condition)
            {
                f = false;
            }
            else
            {
                f = true;
            }

            if (condition)
                f = false;
            else
            {
                f = true;
            }

            if (condition)
            {
                f = false;
            }
            else
                f = true;

            if (condition)
                f = false;
            else
                f = true;

            Base x = null;
            Derived y = null;

            if (y != null)
            {
                x = y;
            }
            else
            {
                x = null;
            }

            if (y != null)
                x = y;
            else
                x = null;

            if (y == null)
            {
                x = null;
            }
            else
            {
                x = y;
            }

            if (y == null)
                x = null;
            else
                x = y;

            // n

            if (condition)
            {
            }
            else if (condition)
            {
                f = true;
            }
            else
            {
                f = false;
            }

            if (condition)
            {
                f = true;
            }
            else
            {
                f = true;
            }

            if (condition)
            {
                f = false;
            }
            else
            {
                f = false;
            }
        }

        private class Base
        {
        }

        private class Derived : Base
        {
        }
    }
}
