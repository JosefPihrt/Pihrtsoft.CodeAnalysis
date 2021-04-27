// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

#pragma warning disable CS0162, RCS1118, RCS1171

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class UseInsteadOfIfRefactoring
    {
        private static bool IfElse()
        {
            bool condition = false;
            bool x = false;
            bool y = false;
            bool z = false;

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
                return true;
            }
            else
            {
                return false;
            }

            if (condition)
            {
                return true;
            }
            else
            {
                return x;
            }

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
            else
            {
                return false;
            }

            if (condition)
            {
                return false;
            }
            else
            {
                return x;
            }

            if (x)
            {
                return true;
            }
            else
            {
                return y;
            }

            if (condition)
            {
                return x;
            }
            else
            {
                return true;
            }

            if (condition)
            {
                return GetValue();
            }
            else
            {
                return false;
            }

            if (x)
            {
                return y;
            }
            else
            {
                return z;
            }
        }

        private static bool IfReturn()
        {
            bool condition = false;
            bool x = false;

            if (condition)
            {
                return true;
            }

            return false;

            if (condition)
            {
                return true;
            }

            return x;

            if (condition)
            {
                return false;
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
            else
            {
                return x;
            }

            if (condition)
            {
                return true;
            }

            return x;

            if (condition)
            {
                return x;
            }

            return true;

            if (condition)
            {
                return GetValue();
            }

            return false;

            if (condition)
            {
                return x;
            }

            return !x;
        }

        public static IEnumerable<bool> IfElseToYieldReturnWithConditionalExpression()
        {
            bool condition = false;
            bool x = false;
            bool y = false;

            if (condition)
            {
                yield return true;
            }
            else
            {
                yield return false;
            }

            if (condition)
            {
                yield return x;
            }
            else
            {
                yield return y;
            }
        }

        public static bool IfElseToAssignmentWithConditionalExpression()
        {
            bool condition = false;

            bool x = false;

            if (condition)
            {
                x = true;
            }
            else
            {
                x = false;
            }

            bool y;

            if (condition)
                y = true;
            else
                y = false;

            x = false;

            if (condition)
            {
                x = true;
            }
            else
            {
                x = false;
            }

            return x;
        }

        private static string IfElseToReturnWithCoalesceExpression()
        {
            string x = null;
            string y = null;

            if (x != null)
            {
                return x;
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
                return x;
            }
        }

        private static bool GetValue()
        {
            return false;
        }
    }
}
