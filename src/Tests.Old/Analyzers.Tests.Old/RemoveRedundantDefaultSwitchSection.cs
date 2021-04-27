// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

#pragma warning disable RCS1111, RCS1136

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class RemoveRedundantDefaultSwitchSection
    {
        private static  void Foo()
        {
            var options = RegexOptions.None;

            switch (options)
            {
                case RegexOptions.Multiline:
                    break;
                case RegexOptions.Singleline:
                    break;
                default:
                    break;
            }

            //n

            switch (options)
            {
                case RegexOptions.Multiline:
                    break;
                case RegexOptions.Singleline:
                    break;
            }

            switch (options)
            {
                case RegexOptions.Multiline:
                    break;
                case RegexOptions.Singleline:
                    break;
                default:
                    Foo();
                    break;
            }

            switch (options)
            {
                case RegexOptions.Multiline:
                    goto default;
                case RegexOptions.Singleline:
                    break;
                default:
                    break;
            }
        }
    }
}
