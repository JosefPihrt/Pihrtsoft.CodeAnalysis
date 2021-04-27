// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#pragma warning disable RCS1028, RCS1069, RCS1070, RCS1111, RCS1118

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class MergeSwitchSectionsWithEquivalentContent
    {
        private static void Foo(object value = null)
        {
            var options = RegexOptions.None;

            switch (options)
            {
                case RegexOptions.CultureInvariant:
                    break;
                case RegexOptions.ECMAScript:
                    break;
                case RegexOptions.ExplicitCapture:
                    break;
                case RegexOptions.IgnoreCase:
                    break;
                case RegexOptions.IgnorePatternWhitespace:
                    break;
                case RegexOptions.Multiline:
                    break;
                case RegexOptions.None:
                    break;
                case RegexOptions.RightToLeft:
                    break;
                case RegexOptions.Singleline:
                    {
                        break;
                    }
                default:
                    break;
            }

            switch (options)
            {
                case RegexOptions.CultureInvariant:
                    break;
#if DEBUG
                case RegexOptions.ECMAScript:
                    break;
#endif
                case RegexOptions.ExplicitCapture:
                    break;
                case RegexOptions.IgnoreCase:
                    break;
                case RegexOptions.IgnorePatternWhitespace:
                    break;
#if DEBUG
                case RegexOptions.Multiline:
                    break;
#endif
                case RegexOptions.None:
                    break;
                case RegexOptions.RightToLeft:
                    break;
                case RegexOptions.Singleline:
                    {
                        break;
                    }
                default:
                    break;
            }

            switch (options)
            {
                case RegexOptions.CultureInvariant:
                    {
                        break;
                    }
                case RegexOptions.ECMAScript:
                    break;
                case RegexOptions.ExplicitCapture:
                    {
                        Foo();
                        break;
                    }
                case object x:
                    break;
            }

            switch (options)
            {
                case RegexOptions.CultureInvariant:
                    {
                        Foo();
                        break;
                    }
                case RegexOptions.ECMAScript:
                    break;
                case RegexOptions.ExplicitCapture:
                    {
                        break;
                    }
            }

            switch (options)
            {
                case RegexOptions.CultureInvariant:
                    {
                        break;
                    }
                case RegexOptions.ECMAScript:
                    Foo();
                    break;
                case RegexOptions.ExplicitCapture:
                    {
                        Foo();
                        break;
                    }
            }

            switch (options)
            {
                case RegexOptions.CultureInvariant:
                    {
                        Foo();
                        break;
                    }
                case RegexOptions.ECMAScript:
                    break;
                case RegexOptions.ExplicitCapture:
                    break;
                case RegexOptions.IgnorePatternWhitespace:
                    break;
                case RegexOptions.IgnoreCase:
                    {
                        Foo();
                        break;
                    }
            }

            switch (options)
            {
                case RegexOptions.ECMAScript: break;
                case RegexOptions.ExplicitCapture: break;
                case RegexOptions.IgnoreCase:
                    {
                        Foo();
                        break;
                    }
            }

            //n

            switch (options)
            {
                case RegexOptions.ECMAScript:
                    break; // xxx
                case RegexOptions.ExplicitCapture:
                    break;
            }

            switch (value)
            {
                case int x:
                    break;
                case long x:
                    break;
            }

            switch (value)
            {
                case int x:
                    Foo(x);
                    break;
                case long x:
                    Foo(x);
                    break;
            }

            switch (value)
            {
                case int x:
                    {
                        Foo(x);
                        break;
                    }
                case long x:
                    {
                        Foo(x);
                        break;
                    }
            }
        }
    }
}
