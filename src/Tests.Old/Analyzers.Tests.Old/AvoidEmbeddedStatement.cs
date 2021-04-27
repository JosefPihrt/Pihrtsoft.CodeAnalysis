// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;

#pragma warning disable RCS1002

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class AvoidEmbeddedStatement
    {
        public static void Foo()
        {
            bool condition = false;

            var items = new List<string>();

            if (condition)
                Foo();

            foreach (string item in items)
                Foo();

            foreach ((string, string) item in Tuple.Values)
                Foo();

            //n

            if (condition)
            {
            }
            else if (condition)
            {
            }

            using (var sr = new StringReader(""))
            using (var sr2 = new StringReader(""))
            {
            }
        }
    }
}
