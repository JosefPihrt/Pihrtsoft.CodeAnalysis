// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS1526_NewExpressionRequiresParenthesesOrBracketsOrBracesAfterType
    {
        private static void Foo()
        {
            var sb = new StringBuilder;

            //n

            var sb2 = new stringbuilder;
        }
    }
}
