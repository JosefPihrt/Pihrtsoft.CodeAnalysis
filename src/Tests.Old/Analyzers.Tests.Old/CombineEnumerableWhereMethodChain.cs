// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

#pragma warning disable RCS1176

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class CombineEnumerableWhereMethodChain
    {
        private static void Foo()
        {
            var items = new List<string>();
            ImmutableArray<string> ia = ImmutableArray.Create<string>();

            IEnumerable<string> q = items.Where(f => true).Where(f => false);

            q = items.Where((f, index) => true).Where((f, index) => false);

            q = ia.Where((f, index) => true).Where((f, index) => false);

            q = ia.Where((f, index) => true).Where((f, index) => false);

            q = items
                .Where((f, index) => true)
                .Where((f, index) => false);

            q = items
                //a
                .Where(f => true) //b
                                  //c
                .Where(f => false); //d

            //n

            q = items.Where((f, index) => true).Where(f => false);

            q = ia.Where(f => true).Where(f => false);
        }
    }
}
