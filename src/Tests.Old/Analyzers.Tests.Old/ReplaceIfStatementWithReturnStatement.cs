// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable CS0162, RCS1002, RCS1004, RCS1007, RCS1098, RCS1111, RCS1118, RCS1126, RCS1128, RCS1176

using System.Collections;
using System.Collections.Generic;

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class ReplaceIfStatementWithReturnStatement
    {
        private static bool Foo(bool condition)
        {
            if (condition)
            {
                return true;
            }
            else
            {
                return false;
            }

            if (condition)
                return true;
            else
                return false;

            if (condition)
            {
                return true;
            }

            return false;

            if (condition)
                return true;

            return false;

            if (condition)
            {
                return false;
            }
            else
            {
                return true;
            }

            if (condition)
            {
                return false;
            }

            return true;

            if (condition)
            {
                return false;
            }

            return true;

            if (condition)
                return false;

            return true;

            switch ("")
            {
                default:
                    if (condition)
                    {
                        return true;
                    }

                    return false;
            }
        }

        private static IEnumerable<bool> IfElseToYieldReturn(bool condition)
        {
            if (condition)
            {
                yield return true;
            }
            else
            {
                yield return false;
            }

            if (condition)
                yield return true;
            else
                yield return false;

            if (condition)
            {
                yield return false;
            }
            else
            {
                yield return true;
            }

            if (condition)
                yield return false;
            else
                yield return true;
        }

        private static Base IfElseToReturn(Derived x)
        {
            if (x != null)
            {
                return x;
            }
            else
            {
                return null;
            }

            if (x != null)
                return x;
            else
                return null;

            if (x == null)
            {
                return null;
            }
            else
            {
                return x;
            }

            if (x == null)
                return null;
            else
                return x;
        }

        private static Base IfReturnToReturn(Derived x)
        {
            if (x != null)
            {
                return x;
            }

            return null;

            if (x != null)
                return x;

            return null;

            if (x == null)
            {
                return null;
            }

            return x;

            if (x == null)
                return null;

            return x;
        }

        private static IEnumerable<Base> IfElseToYieldReturn2(Derived x)
        {
            if (x != null)
            {
                yield return x;
            }
            else
            {
                yield return null;
            }

            if (x != null)
                yield return x;
            else
                yield return null;

            if (x == null)
            {
                yield return null;
            }
            else
            {
                yield return x;
            }

            if (x == null)
                yield return null;
            else
                yield return x;
        }

        private static IEnumerable IfElseToYieldReturn3(Derived x)
        {
            if (x != null)
            {
                yield return x;
            }
            else
            {
                yield return null;
            }

            if (x != null)
                yield return x;
            else
                yield return null;

            if (x == null)
            {
                yield return null;
            }
            else
            {
                yield return x;
            }

            if (x == null)
                yield return null;
            else
                yield return x;
        }

        private class Base
        {
        }

        private class Derived : Base
        {
        }

        //n

        private static bool Foo2(bool condition)
        {
            if (condition)
            {
                return true;
            }
            else
            {
                return true;
            }

            if (condition)
            {
                return false;
            }
            else
            {
                return false;
            }

            if (condition)
            {
                return true;
            }

            return true;

            if (condition)
            {
                return false;
            }

            return false;

            if (condition)
            {
                return false;
            }

            if (condition)
            {
                return false;
            }

            return true;

            if (condition)
                return false;

            if (condition)
                return false;

            return true;

            if (condition)
            {
                return true;
            }

            if (condition)
            {
                return true;
            }

            return false;

            if (condition)
                return true;

            if (condition)
                return true;

            return false;
        }

        private static IEnumerable<bool> IfElseToYieldReturn2(bool condition)
        {
            if (condition)
            {
                yield return true;
            }

            yield return false;

            if (condition)
                yield return true;

            yield return false;

            if (condition)
            {
                yield return false;
            }

            yield return true;

            if (condition)
            {
                yield return false;
            }

            yield return true;

            if (condition)
                yield return false;

            yield return true;
        }
    }
}
