// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class AddCastExpressionRefactoring
    {
        private class Foo
        {
            public void SomeMethod()
            {
                object value = GetObject();

                ProcessString(value);
                ProcessValue(value);

                int i = 0;

                short s = i * i;
            }

            private object GetObject()
            {
                return string.Empty;
            }

            private void ProcessString(string value)
            {
            }

            private void ProcessValue(string value)
            {
            }

            private void ProcessValue(int index)
            {
            }

            public class Base
            {
                public Base(long x)
                {
                }

                public Base(int x)
                {
                }
            }

            public class Derived : Base
            {
                public Derived(object x) : base(x)
                {
                }
            }
        }

        private class AddCastExpressionToAssignmentExpressionRefactoring
        {
            public string SomeMethod()
            {
                string s = null;

                object value = GetObject();

                s = value;

                return s;
            }

            private object GetObject()
            {
                return string.Empty;
            }
        }

        public class AddCastExpressionToReturnStatementRefactoring
        {
            public string GetString()
            {
                object value = GetObject();

                return value;
            }

            private object GetObject()
            {
                return string.Empty;
            }

            private async Task<int> Method4Async()
            {
                bool f = false;
                if (f)
                {
                    return await Task.FromResult(new object());
                }
                else
                {
                    return new object();
                }
            }
        }

        private class AddCastExpressionToVariableDeclarationRefactoring
        {
            public void GetString()
            {
                string value = GetObject();
            }

            private object GetObject()
            {
                string s = "";

                return s;
            }
        }
    }
}
