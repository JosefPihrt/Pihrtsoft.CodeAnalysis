// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class FormatParameterListRefactoring
    {
        private class Foo
        {
            public void Bar(
                string value,
                string value2,
                string value3)
            {
            }

            //n

            public void Bar2(
                string value, //x
                string value2,
                string value3)
            {
            }
        }
    }
}
