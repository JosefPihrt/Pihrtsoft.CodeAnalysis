// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class MakeMemberVirtualRefactoring
    {
        private abstract class Foo
        {
            public abstract bool Bar();

            public abstract void FooVoidMethod();

            public abstract object FooProperty { get; set; }

            public abstract object this[int index] { get; set; }
        }
    }
}
