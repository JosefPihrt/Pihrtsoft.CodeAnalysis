// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class RemoveReturnKeywordOrReturnExpression
    {
        public static void Foo()
        {
            if (true)
                return true;

            return false;
        }

        public static void Foo2()
        {
            if (true)
                return Foo();

            return Foo();
        }

        public static async Task FooAsync()
        {
            if (true)
                return Foo();

            return Foo();
        }
    }
}
