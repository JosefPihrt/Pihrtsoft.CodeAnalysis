// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0713_StaticClassCannotDeriveFromType
    {
        private static class Foo2 : Foo
        {
        }

        private static class Foo3
            : Foo
        {
        }

        private static class Foo<T> : Foo where T : Foo
        {
        }

        private static class Foo2<T> : Foo
            where T : Foo
        {
        }

        private class Foo
        {
        }
    }
}
