// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class SplitDeclarationAndInitializationRefactoring
    {
        private static void Foo()
        {
            string s1 = GetValue();

            var s2 = GetValue();

            string s3 = GetValue2();

            //n

            var s4 = GetValue2();

            var x = new { Property = "" };
        }

        private static string GetValue()
        {
            throw new NotImplementedException();
        }
    }
}
