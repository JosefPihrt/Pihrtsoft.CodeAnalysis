// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

#pragma warning disable RCS1002, RCS1004, RCS1007, RCS1033, RCS1049, RCS1073, RCS1098, RCS1103, RCS1118, RCS1126, RCS1176, CS0162

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class UseCoalesceExpressionInsteadOfIf
    {
        private static void IfElseToAssignment(Derived x, Derived2 y, Base z)
        {
            if (x != null)
            {
                z = x;
            }
            else
            {
                z = y;
            }

            if (x != null)
                z = x;
            else
                z = y;

            if (x == null)
            {
                z = y;
            }
            else
            {
                z = x;
            }

            if (x == null)
                z = y;
            else
                z = x;
        }

        private static void IfElseToAssignmentNullable(int? x, int y, int z)
        {
            if (x != null)
            {
                z = x.Value;
            }
            else
            {
                z = y;
            }

            if (x.HasValue)
            {
                z = x.Value;
            }
            else
            {
                z = y;
            }

            if (x.HasValue == true)
            {
                z = x.Value;
            }
            else
            {
                z = y;
            }

            if (x.HasValue != false)
            {
                z = x.Value;
            }
            else
            {
                z = y;
            }

            if (x == null)
            {
                z = y;
            }
            else
            {
                z = x.Value;
            }

            if (!x.HasValue)
            {
                z = y;
            }
            else
            {
                z = x.Value;
            }

            if (x.HasValue == false)
            {
                z = y;
            }
            else
            {
                z = x.Value;
            }

            if (x.HasValue != true)
            {
                z = y;
            }
            else
            {
                z = x.Value;
            }
        }

        private static Base IfElseToReturn(Derived x, Derived2 y)
        {
            if (x != null)
            {
                return x;
            }
            else
            {
                return y;
            }

            if (null != x)
            {
                return x;
            }
            else
            {
                return y;
            }

            if (x != null)
                return x;
            else
                return y;

            if (x == null)
            {
                return y;
            }
            else
            {
                return x;
            }

            if (null == x)
            {
                return y;
            }
            else
            {
                return x;
            }

            if (x == null)
                return y;
            else
                return x;
        }

        private static int IfElseToReturnNullable(int? x, int y)
        {
            if (x != null)
            {
                return x.Value;
            }
            else
            {
                return y;
            }

            if (x.HasValue)
            {
                return x.Value;
            }
            else
            {
                return y;
            }

            if (x.HasValue == true)
            {
                return x.Value;
            }
            else
            {
                return y;
            }

            if (x.HasValue != false)
            {
                return x.Value;
            }
            else
            {
                return y;
            }

            if (x == null)
            {
                return y;
            }
            else
            {
                return x.Value;
            }

            if (!x.HasValue)
            {
                return y;
            }
            else
            {
                return x.Value;
            }

            if (x.HasValue == false)
            {
                return y;
            }
            else
            {
                return x.Value;
            }

            if (x.HasValue != true)
            {
                return y;
            }
            else
            {
                return x.Value;
            }
        }

        private static Base IfReturnToReturn(Derived x, Derived2 y)
        {
            if (x != null)
            {
                return x;
            }

            return y;

            if (x != null)
                return x;

            return y;

            if (x == null)
            {
                return y;
            }

            return x;

            if (x == null)
                return y;

            return x;
        }

        private static int IfReturnToReturnNullable(int? x, int y)
        {
            if (x != null)
            {
                return x.Value;
            }

            return y;

            if (x.HasValue)
            {
                return x.Value;
            }

            return y;

            if (x.HasValue == true)
            {
                return x.Value;
            }

            return y;

            if (x.HasValue != false)
            {
                return x.Value;
            }

            return y;

            if (x == null)
            {
                return y;
            }

            return x.Value;

            if (!x.HasValue)
            {
                return y;
            }

            return x.Value;

            if (x.HasValue == false)
            {
                return y;
            }

            return x.Value;

            if (x.HasValue != true)
            {
                return y;
            }

            return x.Value;
        }

        private static IEnumerable<Base> IfElseToYieldReturn(Derived x, Derived2 y)
        {
            if (x != null)
            {
                yield return x;
            }
            else
            {
                yield return y;
            }

            if (x != null)
                yield return x;
            else
                yield return y;

            if (x == null)
            {
                yield return y;
            }
            else
            {
                yield return x;
            }

            if (x == null)
                yield return y;
            else
                yield return x;
        }

        private static IEnumerable<int> IfElseToYieldReturnNullable(int? x, int y)
        {
            if (x != null)
            {
                yield return x.Value;
            }
            else
            {
                yield return y;
            }

            if (x.HasValue)
            {
                yield return x.Value;
            }
            else
            {
                yield return y;
            }

            if (x.HasValue == true)
            {
                yield return x.Value;
            }
            else
            {
                yield return y;
            }

            if (x.HasValue != false)
            {
                yield return x.Value;
            }
            else
            {
                yield return y;
            }

            if (x == null)
            {
                yield return y;
            }
            else
            {
                yield return x.Value;
            }

            if (!x.HasValue)
            {
                yield return y;
            }
            else
            {
                yield return x.Value;
            }

            if (x.HasValue == false)
            {
                yield return y;
            }
            else
            {
                yield return x.Value;
            }

            if (x.HasValue != true)
            {
                yield return y;
            }
            else
            {
                yield return x.Value;
            }
        }

        private static IEnumerable IfElseToYieldReturn2(Derived x, Derived2 y)
        {
            if (x != null)
            {
                yield return x;
            }
            else
            {
                yield return y;
            }

            if (x != null)
                yield return x;
            else
                yield return y;

            if (x == null)
            {
                yield return y;
            }
            else
            {
                yield return x;
            }

            if (x == null)
                yield return y;
            else
                yield return x;
        }

        private class Base
        {
        }

        private class Derived : Base
        {
        }

        private class Derived2 : Base
        {
        }
    }
}
