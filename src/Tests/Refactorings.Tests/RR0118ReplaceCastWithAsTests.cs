// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0118ReplaceCastWithAsTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ReplaceCastWithAs;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ReplaceCastWithAs)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        object x = null;

        var y = (C)[||]x;
    }
}
", @"
class C
{
    void M()
    {
        object x = null;

        var y = x as C;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ReplaceCastWithAs)]
        public async Task TestNoRefactoring_ValueType()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M()
    {
        object i = 0;
        int j = (int)[||]i;
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
