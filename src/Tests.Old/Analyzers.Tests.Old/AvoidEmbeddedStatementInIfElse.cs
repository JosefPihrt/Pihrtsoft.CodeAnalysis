// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;

#pragma warning disable RCS1004, RCS1007, RCS1073, RCS1118, RCS1176

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class AvoidEmbeddedStatementInIfElse
    {
        public static void Foo()
        {
            bool f = false;

            if (f)
                Foo();
            else
                Foo();

            if (f)
                Foo();
            else if (f)
                Foo();

            if (f)
                Foo();
            else if (f)
                Foo();
            else
                Foo();

            //n

            if (f)
                Foo();

            if (f)
            {
            }
            else if (f)
            {
            }

            using (var sr = new StringReader(""))
            using (var sr2 = new StringReader(""))
            {
            }
        }
    }
}
