// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class RemoveAllSwitchSectionsRefactoring
    {
        public string GetValue()
        {
            RegexOptions options = RegexOptions.None;

            switch (options)
            {
                case RegexOptions.Multiline:
                    break;
                case RegexOptions.Singleline:
                    break;
            }

            return null;
        }
    }
}
