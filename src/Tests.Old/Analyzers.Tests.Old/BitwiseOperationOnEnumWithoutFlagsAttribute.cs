// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class BitwiseOperationOnEnumWithoutFlagsAttribute
    {
        public static void Foo()
        {
            var options = RegexOptions.None;

            RegexOptions x = options | RegexOptions.None;
            RegexOptions xx = options & RegexOptions.None;
            RegexOptions xxx = options ^ RegexOptions.None;
            RegexOptions xxxx = ~RegexOptions.None;
        }

        public static void Foo2()
        {
            var options = DateTimeKind.Unspecified;

            DateTimeKind x = options | DateTimeKind.Unspecified;
            DateTimeKind xx = options & DateTimeKind.Unspecified;
            DateTimeKind xxx = options ^ DateTimeKind.Unspecified;
            DateTimeKind xxxx = ~DateTimeKind.Unspecified;
        }
    }
}
