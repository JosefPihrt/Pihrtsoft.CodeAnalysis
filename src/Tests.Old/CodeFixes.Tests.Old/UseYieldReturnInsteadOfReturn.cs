// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public static class UseYieldReturnInsteadOfReturn
    {
        private static IEnumerable<string> Foo()
        {
            return "";
        }

        private static IEnumerable<string> Foo2()
        {
            string item = "";

            bool f = false;

            if (f)
                yield return item;

            if (f)
                return "";

            return Foo2();
        }

        private static IEnumerable Foo3()
        {
            string item = "";

            bool f = false;

            if (f)
                yield return item;

            if (f)
                return "";

            if (f)
                return Foo2();
        }

        private static IEnumerable<string> Foo4
        {
            get { return ""; }
        }

        private static IEnumerable<string> Foo5
        {
            get
            {
                string item = "";

                bool f = false;

                if (f)
                    yield return item;

                if (f)
                    return "";

                return Foo5;
            }
        }

        private static IEnumerable Foo6
        {
            get
            {
                string item = "";

                bool f = false;

                if (f)
                    yield return item;

                if (f)
                    return "";

                if (f)
                    return Foo6;
            }
        }
    }
}
