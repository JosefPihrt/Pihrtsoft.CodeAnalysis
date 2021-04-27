// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal abstract class RemoveAllStatementsRefactoring
    {
        public List<int> GetValues()
        {
            var values = new List<int>();

            for (int i = 0; i < 10; i++)
                values.Add(i);

            return values;
        }
    }
}
