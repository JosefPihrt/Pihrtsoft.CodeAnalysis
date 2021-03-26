// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0109RemoveStatementTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.RemoveStatement;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveStatement)]
        public async Task Test_If()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
        {
        }[||]
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveStatement)]
        public async Task Test_IfInSwitchSection()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        string s = null;
        bool f = false;

        switch (s)
        {
            case """":
                if (f)
                {
                }[||]
                break;
            default:
                break;
        }
    }
}
", @"
class C
{
    void M()
    {
        string s = null;
        bool f = false;

        switch (s)
        {
            case """":
                break;
            default:
                break;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveStatement)]
        public async Task Test_IfElseIf()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
        {
        }
        else if (f)
        {
        }[||]
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveStatement)]
        public async Task Test_UsingInsideUsing()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M()
    {
        IDisposable x = null;
    
        using (x)
        using (x)
        {
        }[||]
    }
}
", @"
using System;

class C
{
    void M()
    {
        IDisposable x = null;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveStatement)]
        public async Task TestNoRefactoring_EmbeddedIf()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
            if (f)
            {
            }[||]
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
