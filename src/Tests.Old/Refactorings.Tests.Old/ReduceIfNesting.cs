// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

#pragma warning disable CS0168

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class ReduceIfNesting
    {
        private static readonly bool _condition;
        private static readonly bool _condition1;
        private static readonly bool _condition2;
        private static readonly bool _condition3;

        private static void Foo()
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

            switch (0)
            {
                case 0:
                    if (_condition)
                    {
                        Foo();
                    }

                    break;
            }

            switch (0)
            {
                case 0:
                    {
                        if (_condition)
                        {
                            Foo();
                        }
                    }

                    break;
            }

            switch (0)
            {
                case 0:
                    {
                        if (_condition)
                        {
                            Foo();
                        }

                        return;
                    }
            }

            switch (0)
            {
                case 0:
                    {
                        if (_condition)
                        {
                            Foo();
                        }

                        throw;
                    }
            }
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
    }
}
