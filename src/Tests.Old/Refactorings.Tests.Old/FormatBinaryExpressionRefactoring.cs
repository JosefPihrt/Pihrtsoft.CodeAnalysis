// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class FormatBinaryExpressionRefactoring
    {
        private class Foo
        {
            public void Bar()
            {
                bool expression = false;
                bool expression2 = false;
                bool expression3 = false;

                if (expression && expression2 && expression3)
                {
                }

                if (expression
                    && expression2
                    && expression3)
                {
                }

                // n

                if (expression
                    && expression2 //x
                    && expression3)
                {
                }
            }
        }
    }
}
