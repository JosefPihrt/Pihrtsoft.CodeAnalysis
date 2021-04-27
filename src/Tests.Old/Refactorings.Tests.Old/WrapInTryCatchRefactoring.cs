// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class WrapInTryCatchRefactoring
    {
        public void Foo()
        {
            MethodThatCanThrowException();

            MethodThatCanThrowException2();

            switch (0)
            {
                case 0:
                    MethodThatCanThrowException();
                    MethodThatCanThrowException2();
                    break;
            }

            if (true)
                MethodThatCanThrowException();
        }

        private void MethodThatCanThrowException()
        {
        }

        private void MethodThatCanThrowException2()
        {
        }
    }
}
