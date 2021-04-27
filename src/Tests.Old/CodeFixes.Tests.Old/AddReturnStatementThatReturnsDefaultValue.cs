// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS0168, CS0219

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class AddReturnStatementThatReturnsDefaultValue
    {
        private class Foo
        {
            public string MethodName()
            {
                string s = null;
            }

            public static int? MethodNullable()
            {
                string s = null;
            }

            public static DateTime MethodStruct()
            {
                string s = null;
            }

            public static string MethodWithLocalFunction()
            {
                string LocalFunction()
                {
                    string s = null;
                }
            }

            public static int MethodInt32()
            {
                string s = null;
            }

            public string PropertyName
            {
                get
                {
                    string s = null;
                }
            }

            public string this[int index]
            {
                get
                {
                    string s = null;
                }
            }

            public void Bar()
            {
                var items = Enumerable.Empty<string>();

                items = items.Select(f =>
                {
                    bool condition = false;

                    if (condition)
                    {
                        return f;
                    }
                });

                items = items.Select<string, string>(delegate (string f)
                {
                    bool condition = false;

                    if (condition)
                    {
                        return f;
                    }
                });
            }

            public static explicit operator Foo(string value)
            {
                string s = null;
            }

            public static Foo operator !(Foo value)
            {
                string s = null;
            }
        }

        //n

        private class Foo
        {
            public string MethodName()
            {
            }

            public static int? MethodNullable()
            {
            }

            public static DateTime MethodStruct()
            {
            }

            public static int MethodInt32()
            {
            }

            public string PropertyName
            {
                get { }
            }

            public string this[int index]
            {
                get { }
            }

            public static explicit operator Foo(string value)
            {
            }

            public static Foo operator !(Foo value)
            {
            }
        }

        public static void MethodVoid()
        {
        }

        public static IEnumerable MethodEnumerable()
        {
        }

        public static IEnumerable<object> MethodEnumerableOfT()
        {
        }

        public static xxx MethodErrorType()
        {
        }

        public static int MethodExpressionBody() => ;

        private partial class FooPartial
        {
            partial object Method()
            {
            }

            partial object Method();
        }
    }
}
