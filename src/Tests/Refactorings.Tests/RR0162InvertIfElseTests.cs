// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

#pragma warning disable RCS1090

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0162InvertIfElseTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.InvertIfElse;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertIfElse)]
        public async Task Test_IfElse()
        {
            await VerifyRefactoringAsync(@"
class C
{
    bool M(bool f = false)
    {
        [||]if (f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
", @"
class C
{
    bool M(bool f = false)
    {
        if (!f)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertIfElse)]
        public async Task Test_IfElse_Nested()
        {
            await VerifyRefactoringAsync(@"
class C
{
    bool M()
    {
        bool f = false, f2 = false, f3 = false;

        if (f)
        {
            return f;
        }
        else [||]if (f2)
        {
            return f2;
        }
        else
        {
            return f3;
        }
    }
}
", @"
class C
{
    bool M()
    {
        bool f = false, f2 = false, f3 = false;

        if (f)
        {
            return f;
        }
        else if (!f2)
        {
            return f3;
        }
        else
        {
            return f2;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertIfElse)]
        public async Task TestNoRefactoring_IfElseIf()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M(bool f = false, bool f2 = false)
    {
        [||]if (f)
        {
            return;
        }
        else [||]if (f2)
        {
            return;
        }

        M();
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
