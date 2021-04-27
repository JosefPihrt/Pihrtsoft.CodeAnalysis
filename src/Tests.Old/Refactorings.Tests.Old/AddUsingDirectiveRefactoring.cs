// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class AddUsingDirectiveRefactoring
    {
        public void Do()
        {
            System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.None;

            System.DateTime dt = default(System.DateTime);

            var list = new System.Collections.Generic.List<object>();
        }
    }
}

namespace A
{
    public static class Foo
    {
        public static void Bar()
        {
            A.Foo.Bar();
        }
    }

    namespace B
    {
        public static class Foo2
        {
            public static void Bar2()
            {
                A.Foo.Bar();
            }
        }
    }
}

namespace A.B
{
    public static class Foo3
    {
        public static void Bar()
        {
            A.Foo.Bar();
        }
    }
}