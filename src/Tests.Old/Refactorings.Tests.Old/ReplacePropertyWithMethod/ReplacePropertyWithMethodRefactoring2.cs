// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ReplacePropertyWithMethodRefactoring2
    {
        public ReplacePropertyWithMethodRefactoring2()
        {
            var x = new ReplacePropertyWithMethodRefactoring();

            x = x.Value;
            x = x.Value;

            var s = x.Value4;
        }
    }
}
