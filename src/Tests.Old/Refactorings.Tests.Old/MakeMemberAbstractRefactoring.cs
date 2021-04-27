// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1170

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class MakeMemberAbstractRefactoring
    {
        private abstract class Foo
        {
            public void Bar()
            {
            }

            public string Property { get; private set; }
        }
    }
}
