// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class RemoveConstraintClause
    {
        private class Foo
        {
            private class ClassName where T : class
            {
            }

            private class ClassName2
                where T : class
            {
            }

            private class ClassName3 where T : class where T2 : class
            {
            }

            private class ClassName4 where T : class
                where T2 : class
            {
            }

            private class ClassName5
                where T : class
                where T2 : class
            {
            }

            public struct StructName where T : class
            {
            }

            public interface InterfaceName where T : class
            {
            }

            public void MethodName() where T : class
            {
                void Local() where T : class
                {

                }
            }

            public delegate void DelegateName() where T : class;
        }
    }
}
