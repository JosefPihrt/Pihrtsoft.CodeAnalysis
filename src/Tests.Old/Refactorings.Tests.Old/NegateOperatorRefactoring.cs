// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class NegateOperatorRefactoring
    {
        public void SomeMethod()
        {
            string s = null;

            if (s == null)
            {

            }
        }
    }
}
