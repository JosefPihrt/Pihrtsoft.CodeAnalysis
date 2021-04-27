// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class SortEnumMemberDeclarationsRefactoring
    {
        private enum Foo
        {
            B = 4,
            A = 1,
            C = 2,
            E = 8,
            D = 16,
        }
    }
}
