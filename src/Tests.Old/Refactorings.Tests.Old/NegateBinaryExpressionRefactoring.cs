// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class NegateBinaryExpressionRefactoring
    {
        public void SomeMethod()
        {
            bool a = false;
            bool b = false;
            bool c = false;
            bool d = false;

            bool x = false;
            bool y = false;
            bool z = false;

            if (x && y && z) { }

            if (x & y & z) { }

            if (x || y && z) { }

            if (x && y || z) { }

            if (a && b || c && d) { }

            if (a || b && c && d) { }

            bool f = false;
            int i = 0;
            int? ni = null;
            int? ni2 = null;
            string s = null;

            if (i > 1) { /*true*/ } else { /*false*/ }

            if (ni > 1) { /*true*/ } else { /*false*/ }

            if (s?.Length > 1) { /*true*/ } else { /*false*/ }

            if (s?[0] > 1) { /*true*/ } else { /*false*/ }

            if (s?.IndexOf(s) > 1) { /*true*/ } else { /*false*/ }

            if (s?.ToString()?.IndexOf(s) > 1) { /*true*/ } else { /*false*/ }

            if (ni > ni2) { /*true*/ } else { /*false*/ }

            if (1 > i) { /*true*/ } else { /*false*/ }

            if (1 > ni) { /*true*/ } else { /*false*/ }

            if (1 > s?.Length) { /*true*/ } else { /*false*/ }

            if (1 > s?[0]) { /*true*/ } else { /*false*/ }

            if (1 > s?.IndexOf(s)) { /*true*/ } else { /*false*/ }

            if (1 > s?.ToString()?.IndexOf(s)) { /*true*/ } else { /*false*/ }

            if (ni2 > ni) { /*true*/ } else { /*false*/ }
        }
    }
}
