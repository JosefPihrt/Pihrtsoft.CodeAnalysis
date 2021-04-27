// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

#pragma warning disable RCS1120, RCS1121

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class UseElementAccessInsteadOfEnumerableMethodRefactoring
    {
        public void SomeMethod()
        {
            var items = new List<int>() { 0, 1, 2 };

            var first = items.First();
            var last = items.Last();
            var element = items.ElementAt(1);

            var a = new int[] { 0 };
            first = a.First();
            first = a.Last();
            element = a.ElementAt(1);

            var ia = ImmutableArray.Create(1);
            first = ia.First();
            last = ia.Last();
            element = ia.ElementAt(1);


            string s = "abc";
            char ch1 = s.First();
            char ch2 = s.Last();

            // n

            var dic = new Dictionary<string, string>();
            KeyValuePair<string, string> kvp1 = dic.First();
            KeyValuePair<string, string> kvp2 = dic.Last();
            KeyValuePair<string, string> kvp3 = dic.ElementAt(1);
        }
    }
}
