// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ReplaceForEachWithForRefactoring
    {
        public void SomeMethod()
        {
            string s = null;
            foreach (var ch in s)
            {
                char x = ch; 
            }

            var arr = new string[] { null };
            foreach (var item in arr)
            {
                string x = item;
            }

            var ia = ImmutableArray.Create<string>("");
            foreach (var item in ia)
            {
                string x = item;
            }

            var items = new List<string>();
            foreach (string item in items)
            {
                string x = item;
            }

            MatchCollection matches = null;
            foreach (Match item in matches)
            {
                Match x = item;
            }

            //n

            Foo foo = null;
            foreach (var item in foo)
            {
                string x = item;
            }
        }

        private class Foo : IEnumerable<string>
        {
            public int this[int index]
            {
                get { return 0; }
            }

            public IEnumerator<string> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}
