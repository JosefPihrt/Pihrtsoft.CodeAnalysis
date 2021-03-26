// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0199RemoveEnumMemberValueTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.RemoveEnumMemberValue;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveEnumMemberValue)]
        public async Task Test_SingleMember()
        {
            await VerifyRefactoringAsync(@"
enum E
{
    [|A = 0,|]
    B = 1,
    C
}
", @"
enum E
{
    A,
    B = 1,
    C
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveEnumMemberValue)]
        public async Task Test_MultipleMembers()
        {
            await VerifyRefactoringAsync(@"
enum E
{
    A = 0,
    [|B = 1,

    C,

    D = 4|]
}
", @"
enum E
{
    A = 0,
    B,

    C,

    D
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveEnumMemberValue)]
        public async Task Test_AllMembers()
        {
            await VerifyRefactoringAsync(@"
enum [||]E
{
    A = 1,
    B = 2,

    C,

    D = 4
}
", @"
enum E
{
    A,
    B,

    C,

    D
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveEnumMemberValue)]
        public async Task Test_AllMembers_Flags()
        {
            await VerifyRefactoringAsync(@"
using System;

[Flags]
enum [||]E
{
    None = 0,
    A = 1,
    B = 2,
    AB = A | B,

    C,

    D = 4
}
", @"
using System;

[Flags]
enum E
{
    None,
    A,
    B,
    AB = A | B,

    C,

    D
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveEnumMemberValue)]
        public async Task TestNoRefactoring_SingleMember()
        {
            await VerifyNoRefactoringAsync(@"
enum E
{
    A = 0,
    B = 1,
    [|C,|]
    D,
    E = 4
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveEnumMemberValue)]
        public async Task TestNoRefactoring_MultipleMembers()
        {
            await VerifyNoRefactoringAsync(@"
enum E
{
    A = 0,
    B = 1,
    [|C,
    D,|]
    E = 4
}
", equivalenceKey: RefactoringId);
        }
    }
}
