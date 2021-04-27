// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1023, RCS1036, RCS1078, RCS1146, RCS1156

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class UseStringIsNullOrEmptyMethod
    {
        private static string V { get; }

        private static void Foo(string s, string s2)
        {
            if (s == null || s.Length == 0) { }

            if (s == null || s == string.Empty) { }

            if (s == null || s == "") { }

            if (V == null || V.Length == 0) { }

            if ((s == null) || (s.Length == 0)) { }

            if ((s == null) || (s == string.Empty)) { }

            if ((s == null) || (s == "")) { }


            if (s is null || s.Length == 0) { }

            if (s is null || s == string.Empty) { }

            if (s is null || s == "") { }

            if (V is null || V.Length == 0) { }

            if ((s is null) || (s.Length == 0)) { }

            if ((s is null) || (s == string.Empty)) { }

            if ((s is null) || (s == "")) { }


            if (s != null && s.Length != 0) { }

            if (s != null && s.Length > 0) { }

            if (s != null && s != string.Empty) { }

            if (s != null && s != "") { }

            if (V != null && V.Length != 0) { }

            if (V != null && V.Length > 0) { }

            if ((s != null) && (s.Length != 0)) { }

            if ((s != null) && (s.Length > 0)) { }

            if ((s != null) && (s != string.Empty)) { }

            if ((s != null) && (s != "")) { }


            if (!(s is null) && s.Length > 0) { }

            if (!(s is null) && s != string.Empty) { }

            if (!(s is null) && s != "") { }

            if (!(V is null) && V.Length != 0) { }

            if (!(V is null) && V.Length > 0) { }

            if ((!(s is null)) && (s.Length != 0)) { }

            if ((!(s is null)) && (s.Length > 0)) { }

            if ((!(s is null)) && (s != string.Empty)) { }

            if ((!(s is null)) && (s != "")) { }

            //n

            if (s2 == null || s.Length == 0) { }

            if (s != null || s.Length == 0) { }

            if (s == s2 || s.Length == 0) { }

            if (s == null && s.Length == 0) { }

            if (s == null || s2.Length == 0) { }

            if (s == null || s.Length != 0) { }

            if (s == null || s.Length == 1) { }

            if (s == null || s2 == string.Empty) { }

            if (s2 == null || s == string.Empty) { }

            if (s == null || s2 == "") { }

            if (s2 == null || s == "") { }

            if (s == null || s == "x") { }

            if (s != null && s2 != string.Empty) { }

            if (s2 != null && s != string.Empty) { }

            if (s != null && s2 != "") { }

            if (s2 != null && s != "") { }

            if (s != null && s != "x") { }
        }
    }
}
