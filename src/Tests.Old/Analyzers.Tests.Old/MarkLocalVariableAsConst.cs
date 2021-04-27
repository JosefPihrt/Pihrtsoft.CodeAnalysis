// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable CA1806, CS0219, RCS1008, RCS1078, RCS1081, RCS1124, RCS1176

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class MarkLocalAsConst
    {
        public static void Foo()
        {
            string s = "";
            string x = s;

            string s2 = s + "";
            string x2 = s2;

            var s3 = "";
            string x3 = s3;

            string s4 = "", s5 = "";
            string x4 = s4;
            string x5 = s5;

            bool f = false;
            bool f2 = f;

            var options = StringSplitOptions.None;
            StringSplitOptions options2 = options;
        }

        //n

        public static void Foo2()
        {
            string s6 = "", s7 = string.Empty;
            string x6 = s6;
            string x7 = s7;

            string s8 = string.Empty;
            string x8 = s8;

            string s9 = "";
            string x9 = s9;
            s9 = null;

            int i = 0;
            if (int.TryParse("", out i))
            {
            }

            string tuple1 = null;
            string tuple2 = null;

            (tuple1, tuple2) = default((string, string));

            var a = new int[] { 0 };

            int i1 = 0;
            int i2 = 0;
            (a[i1++], a[i2--]) = default((int, int));

            int i3 = 0;
            int i4 = 0;
            (a[++i3], a[--i4]) = default((int, int));

            int i5 = 0;
            a[i5++] = 0;

            int i6 = 0;
            a[i6--] = 0;

            int i7 = 0;
            a[++i7] = 0;

            int i8 = 0;
            a[--i8] = 0;

            int i9 = 0;
            i = (a[i9++])++;

            int i10 = 0;
            i = (a[i10--])--;

            int i11 = 0;
            i = ++(a[++i11]);

            int i12 = 0;
            i = --(a[--i12]);

            int i13 = 0;
            int.TryParse("", out a[i13++]);

            int i14 = 0;
            int.TryParse("", out a[i14--]);

            int i15 = 0;
            int.TryParse("", out a[++i15]);

            int i16 = 0;
            int.TryParse("", out a[--i16]);
        }

        private static class AddressOfExpression
        {
            public static unsafe void Foo()
            {
                char ch = '\0';

                char* pCh = &ch;
            }
        }
    }
}
