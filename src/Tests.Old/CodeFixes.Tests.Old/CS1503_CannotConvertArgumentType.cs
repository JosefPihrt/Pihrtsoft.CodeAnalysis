// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

#pragma warning disable RCS1213

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS1503_CannotConvertArgumentType
    {
        // Add argument list
        private static void Foo(StringBuilder sb)
        {
            Foo(GetStringBuilder);

            Foo(CS1503_CannotConvertArgumentType.GetStringBuilder);
        }


        // Replace 'null' with default vaule
        private static void Foo(int value, DateTime dateTime)
        {
            Foo(null, null);
        }

        private static StringBuilder GetStringBuilder()
        {
            return new StringBuilder();
        }
    }
}
