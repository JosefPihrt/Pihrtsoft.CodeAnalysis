// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0075MergeIfStatementsTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.MergeIfStatements;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.MergeIfStatements)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
class C
{
    bool M()
    {
        bool f1 = false;
        bool f2 = false;
        bool f3 = false;
        bool f4 = false;

[|        if (f1)
        {
            return true;
        }

        if (f2)
            return true;

        if (f3)
        {
            return true;
        }

        if (f4)
        {
            return true;
        }|]

        return false;
    }
}
", @"
class C
{
    bool M()
    {
        bool f1 = false;
        bool f2 = false;
        bool f3 = false;
        bool f4 = false;

        if (f1 || f2 || f3 || f4)
        {
            return true;
        }

        return false;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.MergeIfStatements)]
        public async Task Test_SwitchSection()
        {
            await VerifyRefactoringAsync(@"
class C
{
    bool M()
    {
        bool f1 = false;
        bool f2 = false;

        switch (0)
        {
            case 0:
[|                if (f1)
                    return false;

                if (f2)
                {
                    return false;
                }|]

                break;
        }

        return true;
    }
}
", @"
class C
{
    bool M()
    {
        bool f1 = false;
        bool f2 = false;

        switch (0)
        {
            case 0:
                if (f1 || f2)
                    return false;

                break;
        }

        return true;
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
