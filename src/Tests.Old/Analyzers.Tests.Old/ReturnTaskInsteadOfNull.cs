// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

#pragma warning disable CS0162, CS0168, RCS1007, RCS1016, RCS1021, RCS1048, RCS1163

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal class ReturnTaskInsteadOfNull
    {
        private Task<string> FooAsync()
        {
            Func<string, Task<string>> func1 = f => null;

            Func<string, Task<string>> func2 = f =>
            {
                if (true)
                    return null;

                return null;
            };

            Func<string, Task<string>> func3 = (f) => null;

            Func<string, Task<string>> func4 = (f) =>
            {
                if (true)
                    return null;

                return null;
            };

            Func<string, Task<string>> func5 = delegate (string f)
            {
                if (true)
                    return null;

                return null;
            };

            return null;

            Task<string> FooLocalAsync(string value)
            {
                if (true)
                    return null;

                return null;
            }
        }

        private Task<string> PropertyAsync
        {
            get
            {
                if (true)
                    return null;

                return null;
            }
        }

        private Task<string> Property2Async
        {
            get => null;
        }

        private Task<string> Property3Async => null;

        private Task<string> this[int index]
        {
            get
            {
                if (true)
                    return null;

                return null;
            }
        }

        private Task<string> this[int index1, int index2]
        {
            get => null;
        }

        private Task<string> this[int index, int index2, int index3] => null;

        private class FooDefault
        {
            private Task<string> FooAsync()
            {
                Func<string, Task<string>> func1 = f => default(Task<string>);

                Func<string, Task<string>> func2 = f =>
                {
                    if (true)
                        return default(Task<string>);

                    return default(Task<string>);
                };

                Func<string, Task<string>> func3 = (f) => default(Task<string>);

                Func<string, Task<string>> func4 = (f) =>
                {
                    if (true)
                        return default(Task<string>);

                    return default(Task<string>);
                };

                Func<string, Task<string>> func5 = delegate (string f)
                {
                    if (true)
                        return default(Task<string>);

                    return default(Task<string>);
                };

                return default(Task<string>);

                Task<string> FooLocalAsync()
                {
                    if (true)
                        return default(Task<string>);

                    return default(Task<string>);
                }
            }

            private Task<string> PropertyAsync
            {
                get
                {
                    if (true)
                        return default(Task<string>);

                    return default(Task<string>);
                }
            }

            private Task<string> Property2Async
            {
                get => default(Task<string>);
            }

            private Task<string> Property3Async => default(Task<string>);

            private Task<string> this[int index]
            {
                get
                {
                    if (true)
                        return default(Task<string>);

                    return default(Task<string>);
                }
            }

            private Task<string> this[int index1, int index2]
            {
                get => default(Task<string>);
            }

            private Task<string> this[int index, int index2, int index3] => default(Task<string>);
        }
    }
}
