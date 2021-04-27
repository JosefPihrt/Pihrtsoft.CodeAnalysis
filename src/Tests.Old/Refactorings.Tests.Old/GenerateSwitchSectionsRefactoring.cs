// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class GenerateSwitchSectionsRefactoring
    {
        public void SomeMethod(string s)
        {

            StringSplitOptions options = StringSplitOptions.None;

            switch (options)
            {

            }

            RegexOptions regexOptions = RegexOptions.None;

            switch (regexOptions)
            {
                default:
                    break;
            }
        }
    }
}
