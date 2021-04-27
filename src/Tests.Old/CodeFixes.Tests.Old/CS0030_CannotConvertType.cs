// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0030_CannotConvertType
    {
        private class Foo
        {
            public void Method()
            {
                var items = new List<Bar>();

                foreach (Foo item in items)
                {
                }
            }
        }

        private class Bar
        {
        }
    }
}
