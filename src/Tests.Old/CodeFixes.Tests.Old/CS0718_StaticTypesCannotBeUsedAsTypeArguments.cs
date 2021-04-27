// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static partial class CS0718_StaticTypesCannotBeUsedAsTypeArguments
    {
        private class Foo
        {
            private class Bar<T>
            {
                private void MethodName()
                {
                    var x1 = new Bar<Foo>();
                    var x2 = new Bar<Foo<T>>();
                    var x3 = new Bar<Foo2<object>>();
                    var x4 = new Bar<FooPartial>();

                    // n

                    var x5 = new Bar<System.Convert>();
                }
            }
        }

        private class Foo<T>
        {
        }

        private class Foo2<T>
        {
        }

        private partial class FooPartial
        {
        }
    }
}
