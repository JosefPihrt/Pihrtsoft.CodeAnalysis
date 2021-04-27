// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

#pragma warning disable RCS1001, RCS1003, RCS1004, RCS1007, RCS1027, RCS1118, RCS1126, RCS1176, RCS1177, RCS1185

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class RemoveBraces
    {
        private static void Foo()
        {
            bool f = false;
            var items = new List<object>();

            if (f)
            {
                Foo();
            }

            if (f) { Foo(); }
            Foo();

            foreach (object item in items)
            {
                Foo();
            }

            foreach ((string, string) item in Tuple.Values)
            {
                Foo();
            }

            for (int i = 0; i < items.Count; i++)
            {
                Foo();
            }

            using ((IDisposable)null)
            {
                Foo();
            }

            while (f)
            {
                Foo();
            }

            do
            {
                Foo();
            }
            while (f);

            lock (null)
            {
                Foo();
            }

            unsafe
            {
                fixed (char* p = "")
                {
                    Foo();
                }
            }

            //n

            if (f)
            {
                Foo();
            }
            else
            {
                Foo();
            }

            if (f)
            {
                Foo();
            }
            else if (f)
            {
                Foo();
            }

            if (f)
            {
                Foo();
            }
            else if (f)
            {
                Foo();
            }
            else
            {
                Foo();
            }
        }
    }
}
