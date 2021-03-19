// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

#pragma warning disable RCS1090

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0189InvertIfTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.InvertIf;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertIf)]
        public async Task Test_If_SingleStatement()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f = false)
    {
        {
            [||]if (f)
            {
                return;
            }

            M();
        }
    }
}
", @"
class C
{
    void M(bool f = false)
    {
        {
            if (!f)
            {
                M();
            }
            else
            {
                return;
            }
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertIf)]
        public async Task Test_If_Return_Recursive()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f1 = false, bool f2 = false, bool f3 = false)
    {
        [||]if (f1)
        {
            return;
        }

        if (f2)
        {
            return;
        }

        if (f3)
        {
            return;
        }

        M();
    }
}
", @"
class C
{
    void M(bool f1 = false, bool f2 = false, bool f3 = false)
    {
        if (!f1)
        {
            if (!f2)
            {
                if (!f3)
                {
                    M();
                }
            }
        }
    }
}
", equivalenceKey: InvertIfRefactoring.RecursiveRefactoringIdentifier);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertIf)]
        public async Task Test_If_ReturnBool_Recursive()
        {
            await VerifyRefactoringAsync(@"
class C
{
    bool M(bool f1 = false, bool f2 = false, bool f3 = false)
    {
        [||]if (f1)
        {
            M(f1);
            return f1;
        }

        if (f2)
        {
            M(f1);
            M(f1, f2);
            return f2;
        }

        if (f3)
        {
            M(f1);
            M(f1, f2);
            M(f1, f2, f3);
            return f3;
        }

        return true;
    }
}
", @"
class C
{
    bool M(bool f1 = false, bool f2 = false, bool f3 = false)
    {
        if (!f1)
        {
            if (!f2)
            {
                if (!f3)
                {
                    return true;
                }
                M(f1);
                M(f1, f2);
                M(f1, f2, f3);
                return f3;
            }
            M(f1);
            M(f1, f2);
            return f2;
        }
        M(f1);
            return f1;
    }
}
", equivalenceKey: InvertIfRefactoring.RecursiveRefactoringIdentifier);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertIf)]
        public async Task Test_If_MultipleStatementsInIf_Recursive()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f1 = false, bool f2 = false, bool f3 = false)
    {
        [||]if (f1)
        {
            M1();
            return;
        }

        if (f2)
        {
            M2();
            return;
        }

        if (f3)
        {
            M3();
            return;
        }

        M();
    }

    void M1() => M();
    void M2() => M();
    void M3() => M();
}
", @"
class C
{
    void M(bool f1 = false, bool f2 = false, bool f3 = false)
    {
        if (!f1)
        {
            if (!f2)
            {
                if (!f3)
                {
                    M();
                }
                else
                {
                    M3();
                }
            }
            else
            {
                M2();
            }
        }
        else
        {
            M1();
        }
    }

    void M1() => M();
    void M2() => M();
    void M3() => M();
}
", equivalenceKey: EquivalenceKey.Join(RefactoringId, "Recursive"));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertIf)]
        public async Task Test_If_SingleStatement_LastStatementIsRedundant()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f = false)
    {
        [||]if (f)
        {
            return;
        }

        M();
    }
}
", @"
class C
{
    void M(bool f = false)
    {
        if (!f)
        {
            M();
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertIf)]
        public async Task Test_If_EmbeddedStatement()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f = false)
    {
        [||]if (f)
            return;

        M();
    }
}
", @"
class C
{
    void M(bool f = false)
    {
        if (!f)
        {
            M();
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertIf)]
        public async Task Test_If_MultipleStatementInIf()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f = false)
    {
        [||]if (f)
        {
            M();
            return;
        }

        M2();
    }

    void M2() => M();
}
", @"
class C
{
    void M(bool f = false)
    {
        if (!f)
        {
            M2();
        }
        else
        {
            M();
        }
    }

    void M2() => M();
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertIf)]
        public async Task Test_If_MultipleStatementAfterIf()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f = false)
    {
        [||]if (f)
        {
            return;
        }

        M();
        M2();
    }

    void M2() => M();
}
", @"
class C
{
    void M(bool f = false)
    {
        if (!f)
        {
            M();
            M2();
        }
    }

    void M2() => M();
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertIf)]
        public async Task Test_If_MultipleStatements_LocalFunction()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f = false)
    {
        [||]if (f)
        {
            M();
            return;
        }

        M2();
        M3();

        void LF() => LF();
    }

    void M2() => M();
    void M3() => M();
}
", @"
class C
{
    void M(bool f = false)
    {
        if (!f)
        {
            M2();
            M3();
        }
        else
        {
            M();
        }

        void LF() => LF();
    }

    void M2() => M();
    void M3() => M();
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertIf)]
        public async Task Test_If_LastStatementIsJumpStatement()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f = false)
    {
        [||]if (f)
        {
            M();
            return;
        }

        M2();
        return;
    }

    void M2() => M();
}
", @"
class C
{
    void M(bool f = false)
    {
        if (!f)
        {
            M2();
            return;
        }
        M();
    }

    void M2() => M();
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertIf)]
        public async Task TestNoRefactoring_NotTopmostIf()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M()
    {
        bool f = false, f2 = false;

        if (f)
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
