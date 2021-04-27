// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;

#pragma warning disable RCS1032, RCS1118

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class UseStringComparison
    {
        private static void Foo()
        {
            string a = null;
            string b = null;

            if (a.ToLower() == b.ToLower()) { }
            if (a.ToUpper() == b.ToUpper()) { }
            if (a.ToLowerInvariant() == b.ToLowerInvariant()) { }
            if (a.ToUpperInvariant() == b.ToUpperInvariant()) { }
            if (a.ToLower() == "a") { }
            if ("A" == b.ToUpper()) { }
            if (a.ToLowerInvariant() == "a") { }
            if ("A" == b.ToUpperInvariant()) { }

            if (a.ToLower() != b.ToLower()) { }
            if (a.ToUpper() != b.ToUpper()) { }
            if (a.ToLowerInvariant() != b.ToLowerInvariant()) { }
            if (a.ToUpperInvariant() != b.ToUpperInvariant()) { }
            if (a.ToLower() != "a") { }
            if ("A" != b.ToUpper()) { }
            if (a.ToLowerInvariant() != "a") { }
            if ("A" != b.ToUpperInvariant()) { }

            if (string.Equals(a.ToLower(), b.ToLower())) { }
            if (string.Equals(a.ToUpper(), b.ToUpper())) { }
            if (string.Equals(a.ToLowerInvariant(), b.ToLowerInvariant())) { }
            if (string.Equals(a.ToUpperInvariant(), b.ToUpperInvariant())) { }
            if (string.Equals(a.ToLower(), "a")) { }
            if (string.Equals("A", b.ToUpper())) { }
            if (string.Equals(a.ToLowerInvariant(), "a")) { }
            if (string.Equals("A", b.ToUpperInvariant())) { }

            if (a.ToLower().Equals(b.ToLower())) { }
            if (a.ToUpper().Equals(b.ToUpper())) { }
            if (a.ToLowerInvariant().Equals(b.ToLowerInvariant())) { }
            if (a.ToUpperInvariant().Equals(b.ToUpperInvariant())) { }
            if (a.ToLower().Equals("a")) { }
            if (a.ToUpper().Equals("A")) { }
            if (a.ToLowerInvariant().Equals("a")) { }
            if (a.ToUpperInvariant().Equals("A")) { }

            if (a.ToLower().StartsWith(b.ToLower())) { }
            if (a.ToUpper().StartsWith(b.ToUpper())) { }
            if (a.ToLowerInvariant().StartsWith(b.ToLowerInvariant())) { }
            if (a.ToUpperInvariant().StartsWith(b.ToUpperInvariant())) { }
            if (a.ToLower().StartsWith("a")) { }
            if (a.ToUpper().StartsWith("A")) { }
            if (a.ToLowerInvariant().StartsWith("a")) { }
            if (a.ToUpperInvariant().StartsWith("A")) { }

            if (a.ToLower().EndsWith(b.ToLower())) { }
            if (a.ToUpper().EndsWith(b.ToUpper())) { }
            if (a.ToLowerInvariant().EndsWith(b.ToLowerInvariant())) { }
            if (a.ToUpperInvariant().EndsWith(b.ToUpperInvariant())) { }
            if (a.ToLower().EndsWith("a")) { }
            if (a.ToUpper().EndsWith("A")) { }
            if (a.ToLowerInvariant().EndsWith("a")) { }
            if (a.ToUpperInvariant().EndsWith("A")) { }

            if (a.ToLower().IndexOf(b.ToLower()) >= 0) { }
            if (a.ToUpper().IndexOf(b.ToUpper()) >= 0) { }
            if (a.ToUpperInvariant().IndexOf(b.ToUpperInvariant()) >= 0) { }
            if (a.ToLowerInvariant().IndexOf(b.ToLowerInvariant()) >= 0) { }
            if (a.ToLower().IndexOf("a") >= 0) { }
            if (a.ToUpper().IndexOf("A") >= 0) { }
            if (a.ToUpperInvariant().IndexOf("a") >= 0) { }
            if (a.ToLowerInvariant().IndexOf("A") >= 0) { }

            if (a.ToLower().LastIndexOf(b.ToLower()) >= 0) { }
            if (a.ToUpper().LastIndexOf(b.ToUpper()) >= 0) { }
            if (a.ToLowerInvariant().LastIndexOf(b.ToLowerInvariant()) >= 0) { }
            if (a.ToUpperInvariant().LastIndexOf(b.ToUpperInvariant()) >= 0) { }
            if (a.ToLower().LastIndexOf("a") >= 0) { }
            if (a.ToUpper().LastIndexOf("A") >= 0) { }
            if (a.ToLowerInvariant().LastIndexOf("a") >= 0) { }
            if (a.ToUpperInvariant().LastIndexOf("A") >= 0) { }

            if (a.ToLower().Contains(b.ToLower())) { }
            if (a.ToUpper().Contains(b.ToUpper())) { }
            if (a.ToLowerInvariant().Contains(b.ToLowerInvariant())) { }
            if (a.ToUpperInvariant().Contains(b.ToUpperInvariant())) { }
            if (a.ToLower().Contains("a")) { }
            if (a.ToUpper().Contains("A")) { }
            if (a.ToLowerInvariant().Contains("a")) { }
            if (a.ToUpperInvariant().Contains("A")) { }

            if ((a.ToLower()) == (b.ToLower())) { }
            if ((a.ToLower()) == ("a")) { }
            if (("a") == (b.ToLower())) { }

            if (string.Equals((a.ToLower()), ("a"))) { }
            if (string.Equals(("A"), (b.ToUpper()))) { }

            if (a.ToLower().Equals((b.ToLower()))) { }
            if (a.ToLower().Equals(("a"))) { }
            if (a.ToLower().StartsWith((b.ToLower()))) { }
            if (a.ToLower().StartsWith(("a"))) { }
            if (a.ToLower().EndsWith((b.ToLower()))) { }
            if (a.ToLower().EndsWith(("a"))) { }
            if (a.ToLower().IndexOf((b.ToLower())) >= 0) { }
            if (a.ToLower().IndexOf(("a")) >= 0) { }
            if (a.ToLower().LastIndexOf((b.ToLower())) >= 0) { }
            if (a.ToLower().LastIndexOf(("a")) >= 0) { }
            if (a.ToLower().Contains((b.ToLower()))) { }
            if (a.ToLower().Contains(("a"))) { }
        }
    }
}
