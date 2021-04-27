// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1118, RCS1023

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class RemoveRedundantBooleanLiteral
    {
        public static void Foo()
        {
            bool f = false;

            if (f && true) { }

            if (f || false) { }

            if (f == true) { }

            if (f != false) { }

            if (true && f) { }

            if (false || f) { }

            if (true == f) { }

            if (false != f) { }

            if (f
#if DEBUG
                && true) { }
#endif

            for (int i = 0; true; i++)
            {
            }
        }
    }
}
