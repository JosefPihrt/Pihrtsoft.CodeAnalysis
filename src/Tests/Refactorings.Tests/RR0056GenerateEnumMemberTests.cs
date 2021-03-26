// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0056GenerateEnumMemberTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.GenerateEnumMember;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.GenerateEnumMember)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
enum Foo
{
    A = 1,
    B = 2,
    [||]C = 3,
}
", @"
enum Foo
{
    A = 1,
    B = 2,
    C = 3,
    EnumMember
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.GenerateEnumMember)]
        public async Task Test_Flags()
        {
            await VerifyRefactoringAsync(@"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    B = 2,
    [||]C = 4,
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
    EnumMember = 8
}
", equivalenceKey: RefactoringId);
        }
    }
}
