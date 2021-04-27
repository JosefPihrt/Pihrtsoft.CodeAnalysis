// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class CheckExpressionForNullRefactoring
    {
        public static void Bar()
        {
            Foo x = GetFoo();

            x = GetFoo();

            if (true)
                x = GetFoo();

            //n

            x = new Foo();

            int i = GetInt();

            i = GetInt();
        }

        private static Foo GetFoo()
        {
            return null;
        }

        private static int GetInt()
        {
            return 0;
        }

        private class Foo
        {
        }
    }
}
