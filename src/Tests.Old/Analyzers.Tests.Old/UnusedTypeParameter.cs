// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable CS0168, CS0219, CS8321, RCS1100, RCS1016, RCS1079, RCS1140, RCS1163, RCS1176

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class UnusedTypeParameter
    {
        private abstract class Foo : IFoo
        {
            public void Bar<T>()
            {
                void LocalFunction<T2>()
                {
                }
            }

            public void Bar<T1, T2>(T2 value)
            {
            }

            /// <summary>
            /// ...
            /// </summary>
            /// <typeparam name="T1"></typeparam>
            /// <typeparam name="T2"></typeparam>
            /// <param name="value"></param>
            /// <returns></returns>
            public void Bar<T1, T2>()
            {
            }

            // n

            public virtual void Virtual<T>()
            {
            }

            public abstract void Abstract<T>();

            public void InterfaceMethod<T>()
            {
            }

            public void Bar<T>(T x)
            {
                x = default(T);
            }

            public void Bar2<T>(object x)
            {
                var t = (T)x;
            }

            public void Bar3<T>() where T : Exception
            {
                try
                {

                }
                catch (T ex)
                {
                }
            }

            public void Bar4<T>()
            {
                Local(out T x);

                void Local<T2>(out T2 value)
                {
                    value = default(T2);
                }
            }

            public void Bar5<T>()
            {
                object x = null;

                if (x is T y)
                {
                }
            }

            public void Bar6<T>()
            {
                T x;
            }

            public void Bar7<T>()
            {
                (T x, object y) tuple;
            }

            public void Bar8<T>()
            {
                Bar8<T>();
            }

            public void Bar9<T>()
            {
                Type x = typeof(T);
            }
        }

        private abstract class Foo2
        {
            public void Bar<T>()
            {
                throw new NotImplementedException();
            }

            public void Bar<T>(T parameter)
            {
                throw new NotImplementedException();
            }

            public T2 Bar<T1, T2>(T2 value)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// ...
            /// </summary>
            /// <typeparam name="T1"></typeparam>
            /// <typeparam name="T2"></typeparam>
            /// <typeparam name="T3"></typeparam>
            /// <param name="value"></param>
            /// <returns></returns>
            public void Bar<T1, T2, T3>()
            {
                throw new NotImplementedException();
            }
        }

        private partial class FooPartial : Foo
        {
            public override void Virtual<T>()
            {
            }

            public override void Abstract<T>()
            {
            }

            partial void BarPartial<T>();
        }

        private partial class FooPartial : Foo
        {
        }

        private interface IFoo
        {
            void InterfaceMethod<T>();
        }

        internal interface IFoo<T>
        {
        }

        internal class FooConstraint<T1, T2> where T1 : IFoo<T2>
        {
            internal void Bar<T3, T4>() where T3 : IFoo<T4>
            {
                var t1 = default(T1);
                var t3 = default(T3);

                void LocalFunction<T5, T6>() where T5 : IFoo<T6>
                {
                    var t5 = default(T5);
                }
            }
        }
    }
}
