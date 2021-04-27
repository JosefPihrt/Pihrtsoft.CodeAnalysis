// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ReplaceIfWithSwitchRefactoring
    {
        public static void Foo()
        {
            string s = "";

            var sb = new StringBuilder();

            using (var sw = new StringReader(s))
            {
                var ch = sw.Read();

                if (ch == 10 || ch == 13)
                {
                    return;
                }
                else
                {
                    sb.Append(ch);
                }
            }

            int i = 0;

            if ((((i) == (1)) || ((i) == (2))))
            {
                for (; ; )
                    break;

                return;
            }
            else
            {
                for (; ; )
                    break;
            }

            RegexOptions options = RegexOptions.None;

            if (options == RegexOptions.CultureInvariant)
                Foo();
            else if (options == RegexOptions.ECMAScript || options == RegexOptions.ExplicitCapture)
            {
                Foo();
                Foo();
                return;
            }
            else if (options == RegexOptions.IgnoreCase || options == RegexOptions.IgnorePatternWhitespace || options == RegexOptions.Multiline)
            {
                Foo();
                Foo();
                Foo();
            }
            else
            {
                Foo();
                Foo();
                Foo();
                Foo();
            }
        }

        //n

        public static void Foo2()
        {
            int i = 0;

            for (; ; )
            {
                if (i == 0)
                {
                    break;
                }
                else
                {
                    return;
                }
            }

            for (; ; )
            {
                if (i == 0)
                {
                    return;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
