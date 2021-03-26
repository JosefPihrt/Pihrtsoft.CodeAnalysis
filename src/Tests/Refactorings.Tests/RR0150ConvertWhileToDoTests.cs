// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0150ConvertWhileToDoTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ConvertWhileToDo;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertWhileToDo)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        bool f = false;

        // leading
        [||]while (f)
        {
            M();
        } // trailing
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;

        // leading
        if (f)
        {
            do
            {
                M();
            }
            while (f);
        } // trailing
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertWhileToDo)]
        public async Task Test_WithoutIf()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        bool f = false;

        // leading
        [||]while (f)
        {
            M();
        } // trailing
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;

        // leading
        do
        {
            M();
        }
        while (f); // trailing
    }
}
", equivalenceKey: WhileStatementRefactoring.ConvertWhileToDoWithoutIfEquivalenceKey);
        }
    }
}
