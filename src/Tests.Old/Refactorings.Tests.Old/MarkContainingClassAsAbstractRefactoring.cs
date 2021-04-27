// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class MarkContainingClassAsAbstractRefactoring
    {
        public class Foo
        {
            public abstract void FooMethod();

            public abstract object FooProperty { get; }

            public abstract object this[int index] { get; }

            public abstract event EventHandler FooEvent, FooEvent2;
        }
    }
}
