// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class AddTypeArgument
    {
        private class Foo : Base
        {
            private void Bar()
            {
                List list1 = null;
                System.Collections.Generic.List list2 = null;
            }
        }

        private class Base<T>
        {

        }

        private class Base<T, T2>
        {
        }

        private class Base2<T, T2>
        {
        }

        //n

        public class Foo2 : Base2<>
        {
        }
    }
}
