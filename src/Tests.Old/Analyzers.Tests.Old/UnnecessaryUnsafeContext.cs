// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable CS8321, RCS1016, RCS1074, RCS1106, RCS1118, RCS1163, RCS1176, RCS1213

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class UnnecessaryUnsafeContext
    {
        public unsafe class Foo
        {
            public static unsafe void Method()
            {
                unsafe
                {
                    char* pCh = null;
                }

                unsafe
                {
                    unsafe
                    {
                        char* pCh = null;
                    }
                }

                unsafe void LocalFunction()
                {
                    unsafe
                    {
                        char* pCh = null;
                    }

                    unsafe void LocalFunction2()
                    {
                        unsafe
                        {
                            unsafe
                            {
                                char* pCh = null;
                            }
                        }
                    }
                }
            }

            public unsafe Foo()
            {
                unsafe
                {
                    char* pCh = null;
                }
            }

            unsafe ~Foo()
            {
                unsafe
                {
                    char* pCh = null;
                }
            }

            public static unsafe implicit operator Foo(string value)
            {
                unsafe
                {
                    char* pCh = null;
                }

                return null;
            }

            public static unsafe Foo operator !(Foo value)
            {
                unsafe
                {
                    char* pCh = null;
                }

                return value;
            }

            public unsafe delegate void FooDelegate();

            public unsafe event EventHandler Event;

            public unsafe event EventHandler Event2
            {
                add
                {
                    unsafe
                    {
                        char* pCh = null;
                    }
                }

                remove { }
            }

            public static readonly unsafe Action Field = () =>
            {
                unsafe
                {
                    char* pCh = null;
                }
            };

            public unsafe int this[int index]
            {
                get
                {
                    unsafe
                    {
                        char* pCh = null;
                    }

                    return index;
                }
            }

            public unsafe string Property
            {
                get
                {
                    unsafe
                    {
                        char* pCh = null;
                    }

                    return null;
                }
            }

            public static unsafe class UnsafeClass
            {
                public unsafe class UnsafeClass2
                {
                }
            }

            public static unsafe class UnsafeStruct
            {
                public unsafe class UnsafeStruct2
                {
                }
            }

            public static unsafe class UnsafeInterface
            {
                public unsafe class UnsafeInterface2
                {
                }
            }
        }

        public unsafe interface IFoo
        {
            unsafe void Method();

            unsafe event EventHandler Event;

            unsafe int this[int index] { get; set; }

            unsafe string Property { get; }
        }

        private unsafe struct Unsafe
        {
            public static void Foo()
            {
                // pointer type
                char* pCh = null;

                // fixed statement
                fixed (char* pStart = "")
                {
                }

                // pointer indirection expression
                char ch = *pCh;

                // addressof expression
                pCh = &ch;

                // stackalloc array creation
                char* pStart2 = stackalloc char[100];

                var x = default(Unsafe);
                var px = &x;

                // pointer member access expression
                px->Value = 25;
            }

            public int Value { get; set; }
        }
    }
}
