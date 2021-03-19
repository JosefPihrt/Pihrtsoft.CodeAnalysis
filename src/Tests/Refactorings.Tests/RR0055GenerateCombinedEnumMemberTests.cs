// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0055GenerateCombinedEnumMemberTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.GenerateCombinedEnumMember;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.GenerateCombinedEnumMember)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    [|B = 2,
    C = 4|]
}
", @"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    B = 2,
    C = 4,
    BC = B | C
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.GenerateCombinedEnumMember)]
        public async Task Test_CombinedMemberInSelection()
        {
            await VerifyRefactoringAsync(@"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    [|B = 2,
    AB = 3,
    C = 4|]
}
", @"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    B = 2,
    AB = 3,
    C = 4,
    BC = B | C
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.GenerateCombinedEnumMember)]
        public async Task TestNoRefactoring_WithoutFlags()
        {
            await VerifyNoRefactoringAsync(@"
enum Foo
{
    A = 1,
    [|B = 2,
    C = 3|]
}
", equivalenceKey: RefactoringId);
        }
    }
}
