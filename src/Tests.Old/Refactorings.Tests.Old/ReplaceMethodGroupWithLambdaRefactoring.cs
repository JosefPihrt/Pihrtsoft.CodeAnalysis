// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ReplaceMethodGroupWithLambdaRefactoring
    {
        public class C
        {
            public void VM()
            {
                Action func1 = VM;
                Action<string> func2 = VM;
                Action<string, string> func3 = VM;

                func1 = VM;
                func2 = VM;
                func3 = VM;
            }

            public string M()
            {
                Func<string> func1 = M;
                Func<string, string> func2 = M;
                Func<string, string, string> func3 = M;

                func1 = M;
                func2 = M;
                func3 = M;

                return null;
            }

            public void M2(
                Func<string> func1,
                Func<string, string> func2,
                Func<string, string, string> func3)
            {
                M2(
                    M,
                    M,
                    M);
            }

            public void M3(
                Action func1,
                Action<string> func2,
                Action<string, string> func3)
            {
                M3(
                    VM,
                    VM,
                    VM);
            }

            public void VM(string s) { }

            public void VM(string s1, string s2) { }

            public string M(string s) => null;

            public string M(string s1, string s2) => null;
        }
    }
}
