// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class AddPartialModifier
    {
        private class Foo
        {
        }

        private partial class Foo
        {
        }

        private struct FooStruct
        {
        }

        private partial struct FooStruct
        {
        }

        private interface IFoo
        {
        }

        private partial interface IFoo
        {
        }

        private class Foo2
        {
            partial void Bar();
        }

        private struct Struct2
        {
            partial void Bar();
        }
    }
}
