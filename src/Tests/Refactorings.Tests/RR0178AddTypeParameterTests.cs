// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0178AddTypeParameterTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.AddTypeParameter;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddTypeParameter)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
class C[||]
{
}
", @"
class C<T>
{
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddTypeParameter)]
        public async Task TestNoRefactoring_SpanOnNextLine()
        {
            await VerifyNoRefactoringAsync(@"
class C
[||]{
}
", equivalenceKey: RefactoringId);
        }
    }
}
