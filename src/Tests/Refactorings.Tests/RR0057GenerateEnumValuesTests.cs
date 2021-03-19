// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0057GenerateEnumValuesTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.GenerateEnumValues;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.GenerateEnumValues)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
using System;

[Flags]
enum Foo
{
    None = 0,
    A,
    B,
    [||]C,
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
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.GenerateEnumValues)]
        public async Task Test_OverwriteExistingValues()
        {
            await VerifyRefactoringAsync(@"
enum [||]Foo
{
    None = 0,

    A = 2,

    B = 1,

    C = 4,
}
", @"
enum Foo
{
    None = 0,

    A = 1,

    B = 2,

    C = 3,
}
", equivalenceKey: GenerateAllEnumValuesRefactoring.EquivalenceKey);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.GenerateEnumValues)]
        public async Task Test_OverwriteExistingValues_Flags()
        {
            await VerifyRefactoringAsync(@"
using System;

[Flags]
enum [||]Foo
{
    None = 0,
    A = 2,
    B = 1,
    AB = A | B,
    C = 4,
}
", @"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    B = 2,
    AB = A | B,
    C = 4,
}
", equivalenceKey: GenerateAllEnumValuesRefactoring.EquivalenceKey);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.GenerateEnumValues)]
        public async Task TestNoRefactoring()
        {
            await VerifyNoRefactoringAsync(@"
enum [||]Foo
{
    None = 0,
    A = 1,
    B = 2,
    C = 3,
    D = 4,
}
", equivalenceKey: GenerateAllEnumValuesRefactoring.EquivalenceKey);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.GenerateEnumValues)]
        public async Task TestNoRefactoring_Flags()
        {
            await VerifyNoRefactoringAsync(@"
using System;

[Flags]
enum [||]Foo
{
    None = 0,
    A = 1,
    B = 2,
    AB = A | B,
    C = 4,
    D = 8,
}
", equivalenceKey: GenerateAllEnumValuesRefactoring.EquivalenceKey);
        }
    }
}
