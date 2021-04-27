// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class RemoveMemberDeclaration
    {
        private struct FooStruct
        {
            public FooStruct()
            {
            }

            ~FooStruct()
            {
            }
        }

        private interface IFoo
        {
            private readonly string _field;

            public static IFoo operator !(IFoo value)
            {
                return null;
            }

            public static explicit operator IFoo(object value)
            {
                return null;
            }

            public static explicit operator object(IFoo value)
            {
                return null;
            }

            ~IFoo()
            {
            }

            private class ClassName
            {
            }

            private struct StructName
            {
            }

            private interface IInterfaceName
            {
            }
        }
    }
}
