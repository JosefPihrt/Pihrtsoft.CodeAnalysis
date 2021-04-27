// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class SwapExpressionsInConditionalExpressionRefactoring
    {
        public void SomeMethod()
        {
            bool condition = false;

            bool x = (condition) ? true : false;
        }
    }
}
