// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ChangeTypeAccordingToExpressionRefactoring
    {
        public void SomeMethod()
        {
            var list = new List<int>();

            HashSet<int> hashSet = list;

            foreach (string item in list)
            {
            }

            string items = list.Select(f => new { f });

            foreach (string item in list.Select(f => new { f }))
            {

            }
        }
    }
}
