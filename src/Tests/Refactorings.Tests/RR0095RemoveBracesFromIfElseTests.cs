// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0095RemoveBracesFromIfElseTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.RemoveBracesFromIfElse;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveBracesFromIfElse)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
        {
            [||]M();
        }
        else if (f)
        {
            M();
        }
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;

        if (f)
            M();
        else if (f)
            M();
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveBracesFromIfElse)]
        public async Task Test2()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
        {
            [||]if (f) M(); else M();
        }
        else if (f)
        {
            M();
        }
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;

        if (f)
            if (f) M(); else M();
        else if (f)
            M();
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveBracesFromIfElse)]
        public async Task TestNoRefactoring_SimpleIfInsideIfWithElse()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
        {
            [||]if (f) M();
        }
        else if (f)
        {
            M();
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveBracesFromIfElse)]
        public async Task TestNoRefactoring_SimpleIfInsideIfWithElse2()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
        {
            [||]if (f) M(); else if (f) M();
        }
        else if (f)
        {
            M();
        }
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
