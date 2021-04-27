// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ExtractExpressionFromConditionRefactoring
    {
        public static void MethodName()
        {
            bool f = false;
            bool f2 = false;
            bool f3 = false;
            bool f4 = false;

            if (f && f2)
            {
                Do();
            }

            if (f && f2 && f3 && f4)
            {
                MethodName();
                MethodName();
            }

            if (f && f2 && f3 && f4)
                MethodName();

            if (f || f2 || f3 || f4)
            {
                MethodName();
                MethodName();
            }

            if (f || f2 || f3 || f4)
                MethodName();

            while (f && f2 && f3 && f4)
            {
                MethodName();
                MethodName();
            }

            while (f && f2 && f3 && f4)
                MethodName();

            while (f || f2 || f3 || f4)
            {
                MethodName();
                MethodName();
            }

            while (f || f2 || f3 || f4)
                MethodName();
        }

        private static void Do()
        {
        }
    }
}
