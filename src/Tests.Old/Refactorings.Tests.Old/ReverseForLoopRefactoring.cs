// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ReverseForLoopRefactoring
    {
        public void SomeMethod()
        {
            var items = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                string value = items[i];
            }
        }
    }
}
