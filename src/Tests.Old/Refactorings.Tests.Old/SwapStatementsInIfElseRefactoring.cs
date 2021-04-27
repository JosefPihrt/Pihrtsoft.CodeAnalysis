// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class SwapStatementsInIfElseRefactoring
    {
        public bool SomeMethod()
        {
            string s = string.Empty;

            if (s.StartsWith("a"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
