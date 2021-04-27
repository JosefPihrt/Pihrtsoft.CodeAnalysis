// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class CallToMethodRefactoring
    {
        private class Foo
        {
            public void Bar()
            {
                object value = GetObject();

                ProcessString(value);

                ProcessValue(value);

                RegexOptions options = RegexOptions.None;
                Debug.Assert(false, options);
                Debug.Assert(false, RegexOptions.Singleline | RegexOptions.Multiline);
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

            private void ProcessArray(string[] items)
            {
                IEnumerable<string> enumerable = items;

                ProcessArray(enumerable);
            }

            private void ProcessList(List<string> items)
            {
                IEnumerable<string> enumerable = items;

                ProcessList(enumerable);
            }
        }

        private class AddCastExpressionToAssignmentExpressionRefactoring
        {
            public void Foo()
            {
                string s = null;

                s = GetObject();
            }

            private object GetObject() => null;
        }

        public class AddCastExpressionToReturnStatementRefactoring
        {
            public string GetString()
            {
                object value = GetObject();

                return value;
            }

            private object GetObject() => null;
        }

        private class AddCastExpressionToVariableDeclarationRefactoring
        {
            public void GetString()
            {
                string value = GetObject();
            }

            private object GetObject() => null;
        }
    }
}
