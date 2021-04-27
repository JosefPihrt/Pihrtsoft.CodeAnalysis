// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class ReplaceEqualsExpressionWithStringEqualsRefactoring
    {
        public static void Foo()
        {
            string s = null;
            string s2 = null;

            if (s == s2)
            {
            }

            if (s != s2)
            {
            }
        }
    }
}
