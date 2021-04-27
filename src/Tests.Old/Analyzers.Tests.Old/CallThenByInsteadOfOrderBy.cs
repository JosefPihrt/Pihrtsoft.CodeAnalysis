// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

#pragma warning disable RCS1021, RCS1176, RCS1196

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class CallThenByInsteadOfOrderBy
    {
        private static void Foo()
        {
            IOrderedEnumerable<string> x = null;

            var items = new List<string>();

            x = items.OrderBy(f => f).OrderBy(f => f);
            x = items.OrderByDescending(f => f).OrderByDescending(f => f);

            x = Enumerable.OrderBy(items, f => f).OrderBy(f => f);
            x = Enumerable.OrderByDescending(items, f => f).OrderByDescending(f => f);
        }
    }
}
