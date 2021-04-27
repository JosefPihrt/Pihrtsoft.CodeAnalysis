// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

#pragma warning disable RCS1014

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class SimplifyCoalesceExpression
    {
        private class Foo
        {
            private const string StringConst = "";
            private const string NullConst = null;

            private int _intValue = 0;

            private void Bar()
            {
                string x = "";

                x = null ?? "";
                x = default(string) ?? "";

                string s = new string(' ', 1) ?? "";
                var a = new { Value = "" } ?? new { Value = "" };
                string[] arr = new string[] { "" } ?? new string[] { "" };
                string[] arr2 = new[] { "" } ?? new[] { "" };
                string s2 = $"{x}" ?? "";
                string s3 = "" ?? "";
                Foo f = this ?? this;
                Type t = typeof(string) ?? typeof(string);

                s = StringConst ?? "";
                s = NullConst ?? "";

                int i = _intValue ?? 0;

                s = x ?? null;
                s = x ?? default(string);

                s = x ?? x;

                int? nullableInt = null;
                i = nullableInt ?? 1;
            }
        }
    }
}
