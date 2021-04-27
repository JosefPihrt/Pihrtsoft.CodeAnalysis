// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

#pragma warning disable CS0168, CS8321, RCS1002, RCS1004, RCS1118, RCS1176, RCS1177, RCS1208, RCS1213

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class RemoveRedundantStatement
    {
        private static void Foo()
        {
            bool f = false;

            do
            {
                Foo();
                continue;

            } while (f);

            while (f)
            {
                Foo();
                continue;
            }

            var items = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                Foo();
                continue;
            }

            foreach (string item in items)
            {
                Foo();
                continue;
            }

            do
            {
                Foo();

                if (f)
                {
                    continue;
                }
                else
                {
                    continue;
                }

            } while (f);

            while (f)
            {
                Foo();

                if (f)
                {
                    continue;
                }
                else
                {
                    continue;
                }
            }

            for (int i = 0; i < items.Count; i++)
            {
                Foo();

                if (f)
                {
                    continue;
                }
                else
                {
                    continue;
                }
            }

            foreach (string item in items)
            {
                Foo();

                if (f)
                {
                    continue;
                }
                else
                {
                    continue;
                }
            }

            return;

            void LocalFunction()
            {
                return;

                void LocalFunction2()
                {
                    return;
                }
            }
        }

        private static void Foo2()
        {
            bool f = false;

            if (f)
            {
                return;
            }
            else
            {
                return;
            }

            void LocalFunction()
            {
                if (f)
                {
                    return;
                }
                else
                {
                    return;
                }

                void LocalFunction2()
                {
                    if (f)
                    {
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private static IEnumerable<object> Foo3()
        {
            yield return null;

            IEnumerable<object> LocalFunction()
            {
                yield return null;
                yield break;
            }

            yield break;

            IEnumerable<object> LocalFunction2()
            {
                yield return null;
                yield break;

                IEnumerable<object> LocalFunction3()
                {
                    yield return null;
                    yield break;
                }
            }
        }

        private static IEnumerable<object> Foo4()
        {
            bool f = false;

            yield return null;

            IEnumerable<object> LocalFunction()
            {
                yield return null;

                if (f)
                {
                    yield break;
                }
                else
                {
                    yield break;
                }
            }

            if (f)
            {
            }
            else
            {
                yield break;
            }

            IEnumerable<object> LocalFunction2()
            {
                yield return null;

                if (f)
                {
                    yield break;
                }
                else
                {
                    yield break;
                }

                IEnumerable<object> LocalFunction3()
                {
                    yield return null;

                    if (f)
                    {
                        yield break;
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
        }

        private static void FooNested()
        {
            bool f = false;

            if (f)
            {
                if (f)
                {
                }
                else
                {
                    try
                    {
                        try
                        {

                        }
                        catch
                        {
                            return;
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static class TryCatch
        {
            private static void Foo()
            {
                bool f = false;

                do
                {
                    Foo();

                    try
                    {
                        Foo();
                        continue;
                    }
                    catch
                    {
                        Foo();
                        continue;
                    }

                } while (f);

                while (f)
                {
                    Foo();

                    try
                    {
                        Foo();
                        continue;
                    }
                    catch
                    {
                        Foo();
                        continue;
                    }
                }

                var items = new List<string>();

                for (int i = 0; i < items.Count; i++)
                {
                    Foo();

                    try
                    {
                        Foo();
                        continue;
                    }
                    catch
                    {
                        Foo();
                        continue;
                    }
                }

                foreach (string item in items)
                {
                    Foo();

                    try
                    {
                        Foo();
                        continue;
                    }
                    catch
                    {
                        Foo();
                        continue;
                    }
                }

                try
                {
                    Foo();
                    return;
                }
                catch
                {
                    Foo();
                    return;
                }

                void LocalFunction()
                {
                    try
                    {
                        Foo();
                        return;
                    }
                    catch
                    {
                        Foo();
                        return;
                    }

                    void LocalFunction2()
                    {
                        try
                        {
                            Foo();
                            return;
                        }
                        catch
                        {
                            Foo();
                            return;
                        }
                    }
                }
            }

            private static IEnumerable<object> Foo3()
            {
                yield return null;

                IEnumerable<object> LocalFunction()
                {
                    yield return null;

                    try
                    {
                        Foo();
                        yield break;
                    }
                    catch
                    {
                        Foo();
                        yield break;
                    }
                }

                try
                {
                    Foo();
                    yield break;
                }
                catch
                {
                    Foo();
                    yield break;
                }

                IEnumerable<object> LocalFunction2()
                {
                    yield return null;

                    try
                    {
                        Foo();
                        yield break;
                    }
                    catch
                    {
                        Foo();
                        yield break;
                    }

                    IEnumerable<object> LocalFunction3()
                    {
                        yield return null;

                        try
                        {
                            Foo();
                            yield break;
                        }
                        catch
                        {
                            Foo();
                            yield break;
                        }
                    }
                }
            }
        }

        //n

        private static string Foo5()
        {
            return;

            string LocalFunction()
            {
                return;

                string LocalFunction2()
                {
                    return;
                }
            }
        }

        private static string Foo6()
        {
            bool f = false;

            if (f)
            {
            }
            else
            {
                return;
            }

            string LocalFunction()
            {
                if (f)
                {
                }
                else
                {
                    return;
                }

                string LocalFunction2()
                {
                    if (f)
                    {
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private static IEnumerable<object> Foo7()
        {
            yield break;

            IEnumerable<object> LocalFunction()
            {
                yield break;
            }
        }

        private static IEnumerable<object> Foo8()
        {
            IEnumerable<object> LocalFunction()
            {
                yield break;
            }

            yield break;
        }
    }
}
