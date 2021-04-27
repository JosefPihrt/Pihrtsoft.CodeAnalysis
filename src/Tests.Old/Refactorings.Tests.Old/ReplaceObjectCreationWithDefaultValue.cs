// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable CS0219, RCS1118

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class ReplaceObjectCreationWithDefaultValue
    {
        private static void Foo()
        {
            var x1 = new object();

            var x2 = new object[1,2];

            var x3 = new[] { default(object) };

            string s = new string(' ', 1);
            char ch = new char();
            bool b = new bool();
            int i = new int();
            int? ni = new int?();
            StringSplitOptions sso = new StringSplitOptions();
            DateTime dt = new DateTime();
        }

        private static void Foo2()
        {
            object x1 = new object();

            object[,] x2 = new object[1,2];

            object[] x3 = new[] { default(object) };
        }
    }
}
