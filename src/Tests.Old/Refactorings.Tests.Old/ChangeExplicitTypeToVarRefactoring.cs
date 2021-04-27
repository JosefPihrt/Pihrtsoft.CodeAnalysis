// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ChangeExplicitTypeToVarRefactoring
    {
        public void SomeMethod()
        {
            object x = new object();

            string value = null;
            if (DateTime.TryParse(value, out DateTime result))
            {
            }
        }
    }
}
