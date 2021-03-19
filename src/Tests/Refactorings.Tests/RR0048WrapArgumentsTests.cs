// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0048WrapArgumentsTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.WrapArguments;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.WrapArguments)]
        public async Task Test_ToMultiLine()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(string p1, string p2, string p3)
    {
        M(p1[||], p2, p3);
    }
}
", @"
class C
{
    void M(string p1, string p2, string p3)
    {
        M(
            p1,
            p2,
            p3);
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.WrapArguments)]
        public async Task Test_ToMultiLine2()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(string p1, string p2, string p3)
    {
        M[|(p1, p2, p3)|];
    }
}
", @"
class C
{
    void M(string p1, string p2, string p3)
    {
        M(
            p1,
            p2,
            p3);
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.WrapArguments)]
        public async Task Test_ToSingleLine()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(string p1, string p2, string p3)
    {
        M(
            p1[||],
            p2,
            p3);
    }
}
", @"
class C
{
    void M(string p1, string p2, string p3)
    {
        M(p1, p2, p3);
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.WrapArguments)]
        public async Task Test_ToSingleLine2()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(string p1, string p2, string p3)
    {
        M[|(
            p1,
            p2,
            p3)|];
    }
}
", @"
class C
{
    void M(string p1, string p2, string p3)
    {
        M(p1, p2, p3);
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.WrapArguments)]
        public async Task Test_ToSingleLine3()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public C(int[] values)
    {
    }

    C()
    {
        C[] c = { new C[|(new int[]
        {
            1,
            2
        })|]
        };
    }
}
", @"
class C
{
    public C(int[] values)
    {
    }

    C()
    {
        C[] c = { new C(new int[] { 1, 2 }) };
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.WrapArguments)]
        public async Task TestNoRefactoring()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M(string p1, string p2, string p3)
    {
        M(
            [|p1,
            p2, //x
            p3|]);
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
