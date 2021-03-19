// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0198InlinePropertyTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.InlineProperty;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineProperty)]
        public async Task TestNoRefactoring()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M()
    {
        var c = this;

        C a = c.[||]A;
    }

    C A
    {
        get { return B; }
    }

    C B
    {
        get { return null; }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineProperty)]
        public async Task TestNoRefactoring2()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M()
    {
        var c = this;

        C a = c?.[||]A;
    }

    C A
    {
        get { return B; }
    }

    C B
    {
        get { return null; }
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
