// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class RemoveRedundantDisposeOrCloseCall
    {
        private static void Foo(StreamReader sr)
        {
            using (sr)
            {
                int x = sr.Read();
                sr.Dispose();
            }

            using (var sw = new StreamWriter(null))
            {
                sw.Write("");
                sw.Dispose();
            }

            //n

            var sr2 = new StreamReader(null);

            using (sr)
            {
                int x = sr.Read();
                sr2.Dispose();
            }

            var sw2 = new StreamReader(null);

            using (var sw = new StreamWriter(null))
            {
                sw.Write("");
                sw2.Dispose();
            }
        }
    }
}
