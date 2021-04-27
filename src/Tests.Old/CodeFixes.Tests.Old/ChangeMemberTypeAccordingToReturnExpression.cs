// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS0168, CS1998, CS4014

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ChangeMemberTypeAccordingToReturnExpression
    {
        private class Void
        {
            public void Field = DateTime.Now;

            public void FooMethod()
            {
                return DateTime.Now;

                void FooLocalFunction()
                {
                    return DateTime.Now;
                }
            }

            public void FooProperty
            {
                get { return DateTime.Now; }
            }

            public void this[int index]
            {
                get { return DateTime.Now; }
            }
        }

        private class VoidExpressionBody
        {
            public void FooMethod() => DateTime.Now;

            public void FooProperty => DateTime.Now;

            public void this[int index] => DateTime.Now;
        }

        private class NonVoid
        {
            public string Field = DateTime.Now;

            public string FooMethod()
            {
                return DateTime.Now;

                string FooLocalFunction()
                {
                    return DateTime.Now;
                }
            }

            public string FooProperty
            {
                get { return DateTime.Now; }
            }

            public string this[int index]
            {
                get { return DateTime.Now; }
            }
        }

        private class NonVoidExpressionBody
        {
            public string FooMethod() => DateTime.Now;

            public string FooProperty => DateTime.Now;

            public string this[int index] => DateTime.Now;
        }

        private class NonVoidExplicitConversion
        {
            public int Field = default(long);

            public int FooMethod()
            {
                return default(long);

                int FooLocalFunction()
                {
                    return default(long);
                }
            }

            public int FooProperty
            {
                get { return default(long); }
            }

            public int this[int index]
            {
                get { return default(long); }
            }
        }

        private class NonVoidExplicitConversionExpressionBody
        {
            public int FooMethod() => default(long);

            public int FooProperty => default(long);

            public int this[int index] => default(long);
        }

        private class AsyncMethod
        {
            private async void VoidAwaitAsync()
            {
                return await Task.FromResult(false);
            }

            private async void VoidAsync()
            {
                return Task.FromResult(false);
            }

            private async Task TaskAwaitAsync()
            {
                return await Task.FromResult(false);
            }

            private async Task TaskAsync()
            {
                return Task.FromResult(false);
            }

            private async Task<bool> TaskOfTAwaitAsync()
            {
                return await Task.FromResult(new object());
            }

            private async Task<bool> TaskOfTAsync()
            {
                return Task.FromResult(new object());
            }
        }

        private class AsyncMethodExpressionBody
        {
            private async void VoidAwaitAsync() => await Task.FromResult(false);

            private async void VoidAsync() => Task.FromResult(false);

            private async Task TaskAwaitAsync() => await Task.FromResult(false);

            private async Task TaskAsync() => Task.FromResult(false);

            private async Task<bool> TaskOfTAwaitAsync() => await Task.FromResult(new object());

            private async Task<bool> TaskOfTAsync() => Task.FromResult(new object());
        }

        private class YieldReturn
        {
            private static IEnumerable<string> Foo()
            {
                yield return default(long);
            }

            private static IEnumerable<int> Foo2()
            {
                yield return default(long);
            }
        }

        //IOrderedEnumerable<T>

        public void GetValues()
        {
            return Enumerable.Range(0, 0).OrderByDescending(f => f);
        }

        public string GetValues2()
        {
            return Enumerable.Range(0, 0).OrderByDescending(f => f);
        }

        //n

        private class AsyncMethodReturnTask
        {
            private async Task<bool> TaskOfTTaskAsync()
            {
                return Task.Run(null);
            }

            private async Task<bool> TaskOfTTaskAsync() => Task.Run(null);
        }

        private interface IFoo
        {
            string Foo();
        }

        private class FooExplicitImplementation : IFoo
        {
            public string Foo()
            {
                return new object();
            }
        }

        private class FooToString
        {
            public override string ToString()
            {
                return new object();
            }
        }

        private class FooTuple
        {
            private static void Foo()
            {
                return (0, new { Value = "x" });
            }

            private static string Foo2()
            {
                return (0, new { Value = "x" });
            }
        }
    }
}
