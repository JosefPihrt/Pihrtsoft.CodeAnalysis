// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0211ConvertStatementsToIfElseTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ConvertStatementsToIfElse;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertStatementsToIfElse)]
        public async Task Test_IfStatementsOnly()
        {
            await VerifyRefactoringAsync(@"
class C
{
    int M()
    {
        bool f1 = false, f2 = false, f3 = false, f4 = false;

[|        if (f1)
            return 1;

        if (f2)
        {
            return 2;
        }
        else if (f3)
        {
            return 3;
        }

        if (f4)
        {
            return 4;
        }|]

        return 0;
    }
}
", @"
class C
{
    int M()
    {
        bool f1 = false, f2 = false, f3 = false, f4 = false;

        if (f1)
        {
            return 1;
        }
        else if (f2)
        {
            return 2;
        }
        else if (f3)
        {
            return 3;
        }
        else if (f4)
        {
            return 4;
        }

        return 0;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertStatementsToIfElse)]
        public async Task Test_IfStatementsAndStatements()
        {
            await VerifyRefactoringAsync(@"
class C
{
    int M()
    {
        bool f1 = false, f2 = false, f3 = false;

[|        if (f1)
            return 1;

        if (f2)
        {
            return 2;
        }
        else if (f3)
        {
            return 3;
        }

        object x = null;

        return 0;|]
    }
}
", @"
class C
{
    int M()
    {
        bool f1 = false, f2 = false, f3 = false;

        if (f1)
        {
            return 1;
        }
        else if (f2)
        {
            return 2;
        }
        else if (f3)
        {
            return 3;
        }
        else
        {
            object x = null;

            return 0;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertStatementsToIfElse)]
        public async Task Test_IfStatementAndStatements()
        {
            await VerifyRefactoringAsync(@"
class C
{
    int M()
    {
        bool f1 = false;

[|        if (f1)
            return 1;

        object x = null;

        return 0;|]
    }
}
", @"
class C
{
    int M()
    {
        bool f1 = false;

        if (f1)
        {
            return 1;
        }
        else
        {
            object x = null;

            return 0;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertStatementsToIfElse)]
        public async Task Test_IfElse()
        {
            await VerifyRefactoringAsync(@"
class C
{
    int M()
    {
        bool f1 = false;

[|        if (f1)
            return 1;

        return 0;|]
    }
}
", @"
class C
{
    int M()
    {
        bool f1 = false;

        if (f1)
            return 1;
        else
            return 0;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertStatementsToIfElse)]
        public async Task Test_IfElse_WithBlock()
        {
            await VerifyRefactoringAsync(@"
class C
{
    int M()
    {
        bool f1 = false;

[|        if (f1)
        {
            return 1;
        }

        return 0;|]
    }
}
", @"
class C
{
    int M()
    {
        bool f1 = false;

        if (f1)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertStatementsToIfElse)]
        public async Task TestNoRefactoring_IfWithoutJumpStatement()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
[|    public string M(bool f)
    {
        if (f)
        {
            M(f);
        }

        return null;|]
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertStatementsToIfElse)]
        public async Task TestNoRefactoring_IfEndsWithElse()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    public string M2(bool f)
    {
[|        if (f)
        {
            return null;
        }
        else
        {
        }

        return null;|]
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
