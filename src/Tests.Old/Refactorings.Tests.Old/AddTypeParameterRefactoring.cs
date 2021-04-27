// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable CS0168, CS0219, RCS1163

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class AddTypeParameterRefactoring
    {
        public static void Foo()
        {
        }

        private static void Foo(object T)
        {
            object T2 = null;

            if (true)
            {
                object T4 = null;
            }

            void T3()
            {
                object T4 = null;

                if (true)
                {
                    object T5 = null;
                }
            }
        }

        private struct ClassName<T> where T : IInterfaceName
        {
        }

        private struct StructName
        {
        }

        private interface IInterfaceName
        {
        }

        private delegate void Delegatename(object T);
    }
}
