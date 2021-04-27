// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable CS0168, RCS1002, RCS1006, RCS1016, RCS1048, RCS1049, RCS1090, RCS1111, RCS1118, RCS1163, RCS1176, RCS1177, RCS1187

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class ReduceIfNesting
    {
        private static readonly bool _condition;
        private static readonly bool _condition1;
        private static readonly bool _condition2;
        private static readonly bool _condition3;

        private static void Foo()
        {
            if (_condition)
            {
                Foo();

                if (!_condition)
                {
                    Foo2();

                    if (_condition)
                    {
                        Foo3();
                    }
                }
            }

            void FooLocal()
            {
                if (_condition)
                {
                    Foo();

                    if (!_condition)
                    {
                        Foo2();

                        if (_condition)
                        {
                            Foo3();
                        }
                    }
                }
            }
        }

        private static IEnumerable FooYield()
        {
            if (_condition)
            {
                yield return "1";

                if (!_condition)
                {
                    yield return "2";

                    if (_condition)
                    {
                        yield return "3";
                    }
                }
            }

            IEnumerable FooYieldLocal()
            {
                if (_condition)
                {
                    yield return "1";

                    if (!_condition)
                    {
                        yield return "2";

                        if (_condition)
                        {
                            yield return "3";
                        }
                    }
                }
            }
        }

        private static IEnumerable<object> FooYield2()
        {
            if (_condition)
            {
                yield return "1";

                if (!_condition)
                {
                    yield return "2";

                    if (_condition)
                    {
                        yield return "3";
                    }
                }
            }

            IEnumerable<object> FooYieldLocal()
            {
                if (_condition)
                {
                    yield return "1";

                    if (!_condition)
                    {
                        yield return "2";

                        if (_condition)
                        {
                            yield return "3";
                        }
                    }
                }
            }
        }

        private static async Task FooAsync()
        {
            if (_condition)
            {
                await FooAsync();

                if (!_condition)
                {
                    await Foo2Async();

                    if (!_condition)
                    {
                        await Foo3Async();
                    }
                }
            }

            async Task FooLocalAsync()
            {
                if (_condition)
                {
                    await FooAsync();

                    if (!_condition)
                    {
                        await Foo2Async();

                        if (!_condition)
                        {
                            await Foo3Async();
                        }
                    }
                }
            }
        }

        private static void FooSimpleLambda()
        {
            Foo(f =>
            {
                if (_condition)
                {
                    Foo();

                    if (!_condition)
                    {
                        Foo2();

                        if (_condition)
                        {
                            Foo3();
                        }
                    }
                }
            });
        }

        private static void FooParenthesizedLambda()
        {
            Foo(() =>
            {
                if (_condition)
                {
                    Foo();

                    if (!_condition)
                    {
                        Foo2();

                        if (_condition)
                        {
                            Foo3();
                        }
                    }
                }
            });
        }

        private static void FooAnonymousMethod()
        {
            Foo(delegate()
            {
                if (_condition)
                {
                    Foo();

                    if (!_condition)
                    {
                        Foo2();

                        if (_condition)
                        {
                            Foo3();
                        }
                    }
                }
            });
        }

        // n

        private static void FooFor()
        {
            var items = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                if (_condition1)
                {
                    Foo1();

                    if (_condition2)
                    {
                        Foo2();

                        if (_condition3)
                        {
                            Foo3();

                            for (int j = 0; j < items.Count; j++)
                            {
                                if (_condition)
                                {
                                    Foo();
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void FooForEach()
        {
            var items = new List<string>();

            foreach (string item in items)
            {
                if (_condition1)
                {
                    Foo1();

                    if (_condition2)
                    {
                        Foo2();

                        if (_condition3)
                        {
                            Foo3();

                            foreach (string item2 in items)
                            {
                                if (_condition)
                                {
                                    Foo();
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void FooDo()
        {
            do
            {
                if (_condition1)
                {
                    Foo1();

                    if (_condition2)
                    {
                        Foo2();

                        if (_condition3)
                        {
                            Foo3();

                            do
                            {
                                if (_condition)
                                {
                                    Foo();
                                }

                            } while (_condition);
                        }
                    }
                }

            } while (_condition);
        }

        private static void FooWhile()
        {
            while (_condition)
            {
                if (_condition1)
                {
                    Foo1();

                    if (_condition2)
                    {
                        Foo2();

                        if (_condition3)
                        {
                            Foo3();

                            while (_condition)
                            {
                                if (_condition)
                                {
                                    Foo();
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void FooSwitchSection()
        {
            switch (0)
            {
                case 0:
                    if (_condition1)
                    {
                        Foo1();

                        if (_condition2)
                        {
                            Foo2();

                            if (_condition3)
                            {
                                Foo3();

                                switch (0)
                                {
                                    case 0:
                                        if (_condition)
                                        {
                                            Foo();
                                        }

                                        break;
                                }
                            }
                        }
                    }

                    break;
            }

            switch (0)
            {
                case 0:
                    {
                        if (_condition1)
                        {
                            Foo1();

                            if (_condition2)
                            {
                                Foo2();

                                if (_condition3)
                                {
                                    Foo3();

                                    switch (0)
                                    {
                                        case 0:
                                            {
                                                if (_condition)
                                                {
                                                    Foo();
                                                }

                                                break;
                                            }
                                    }
                                }
                            }
                        }

                        break;
                    }
            }
        }

        private static void FooElseIf()
        {
            if (_condition)
            {
            }
            else if (_condition)
            {
                if (_condition1)
                {
                    Foo1();

                    if (_condition2)
                    {
                        Foo2();

                        if (_condition3)
                        {
                            Foo3();
                        }
                    }
                }
            }

            void Local()
            {
            }
        }

        private static void FooElse()
        {
            if (_condition)
            {
            }
            else
            {
                if (_condition1)
                {
                    Foo1();

                    if (_condition2)
                    {
                        Foo2();

                        if (_condition3)
                        {
                            Foo3();
                        }
                    }
                }
            }

            void Local()
            {
            }
        }

        private static void Foo(Action action)
        {
        }

        private static void Foo(Action<object> action)
        {
        }

        private static void Foo1()
        {
        }

        private static void Foo2()
        {
        }

        private static void Foo3()
        {
        }

        private static async Task Foo2Async()
        {
            await Foo2Async();
        }

        private static async Task Foo3Async()
        {
            await Foo3Async();
        }
    }
}
