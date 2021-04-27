// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#pragma warning disable RCS1054, RCS1118, RCS1176, RCS1177, RCS1185

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class InlineLocalVariable
    {
        private static void FooLocalDeclaration()
        {
            // ...
            string i = "i";

            string s = i;

            Expression<Func<object, bool>> e = f => false;
            LambdaExpression l = e;

            string[] arr1 = { "" };
            string[] arr2 = arr1;
        }

        private static void LocalDeclaration2()
        {
            string i = "i";
            string s = i;
        }

        private static void FooExpressionStatement()
        {
            string s = "";
            LambdaExpression l = null;
            string[] arr2 = null;

            // ...
            const string i = "i";

            s = i;

            Expression<Func<object, bool>> e = f => false;
            l = e;

            string[] arr1 = { "" };
            arr2 = arr1;

            string s2 = "";
#if DEBUG
            s = s2;
#endif
        }

        private static void FooExpressionStatement2()
        {
            string s = null;

            string i = "i";
            s = i;
        }

        private static void FooForEach()
        {
            IEnumerable<int> items = Enumerable.Range(0, 10);
            foreach (int item in items)
            {
            }
        }

        private static bool FooSwitch()
        {
            string i = "i";

            switch (i)
            {
                case "i":
                    return true;
                case "i2":
                    return false;
            }

            return false;
        }

        private static string FooReturn()
        {
            string i = "i";

            return i;
        }

        public static LambdaExpression FooReturn2()
        {
            Expression<Func<object, bool>> e = f => false;
            return e;
        }

        public static string[] FooReturn3()
        {
            string[] arr1 = { "" };
            return arr1;
        }

        // n

        private static void FooExpressionStatement3()
        {
            string s = null;

            string i = "i";
            i = i;
        }

        private static void Foo()
        {
            string s = "";
            string x = s;

            string x2 = s;
        }

        private static void Foo_()
        {
            IEnumerable<int> items = Enumerable.Range(0, 10);
            foreach (int item in items)
            {
            }

            items = null;
        }

        private static void Foo__()
        {
            string x = null;

            string s2 = "";
            x = s2;
            x = s2;
        }
    }
}
