// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable CS8321

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class MoveUnsafeContextToContainingDeclarationRefactoring
    {
        private static class Foo
        {
            public static void Bar()
            {
                void Local()
                {
                    void Local2()
                    {
                        unsafe
                        {
                            Bar();
                            Bar();
                        }
                    }
                }
            }
        }
    }
}
