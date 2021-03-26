// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0098RemoveCommentTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.RemoveComment;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveComment)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
#pragma warning disable RCS1018 // [||]Add accessibility modifiers.

class C
{
    void M()
    {
    }
}
", @"
#pragma warning disable RCS1018

class C
{
    void M()
    {
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
