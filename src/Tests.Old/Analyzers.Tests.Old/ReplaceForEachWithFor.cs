// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class ReplaceForEachWithFor
    {
        private static void Foo()
        {
            var items = new List<object>();

            foreach (object item in items)
            {
            }
        }
    }
}
