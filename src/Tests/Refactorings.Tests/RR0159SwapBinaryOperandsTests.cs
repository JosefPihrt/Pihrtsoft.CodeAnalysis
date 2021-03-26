// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0159SwapBinaryOperandsTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.SwapBinaryOperands;

        [Theory, Trait(Traits.Refactoring, RefactoringIdentifiers.SwapBinaryOperands)]
        [InlineData("f &[||]& f2", "f2 && f")]
        [InlineData("f |[||]| f2", "f2 || f")]
        [InlineData("i =[||]= j", "j == i")]
        [InlineData("i ![||]= j", "j != i")]
        [InlineData("i [||]> j", "j < i")]
        [InlineData("i >[||]= j", "j <= i")]
        [InlineData("i [||]< j", "j > i")]
        [InlineData("i <[||]= j", "j >= i")]
        public async Task Test(string source, string expected)
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        bool f = false;
        bool f2 = false;
        int i = 0;
        int j = 0;
            
        if ([||]) { }
    }
}
", source, expected, equivalenceKey: RefactoringId);
        }

        [Theory, Trait(Traits.Refactoring, RefactoringIdentifiers.SwapBinaryOperands)]
        [InlineData("i [||]+ j", "j + i")]
        [InlineData("i [||]* j", "j * i")]
        public async Task Test_AddMultiply(string source, string expected)
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(int i, int j)
    {
        int k = [||];
    }
}
", source, expected, equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.SwapBinaryOperands)]
        public async Task Test_BitwiseAnd()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f1, bool f2)
    {
        bool f = f1 [||]& f2;
    }
}
", @"
class C
{
    void M(bool f1, bool f2)
    {
        bool f = f2 {|n:&|} f1;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.SwapBinaryOperands)]
        public async Task Test_BitwiseOr()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f1, bool f2)
    {
        bool f = f1 [||]| f2;
    }
}
", @"
class C
{
    void M(bool f1, bool f2)
    {
        bool f = f2 | f1;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.SwapBinaryOperands)]
        public async Task Test_ExclusiveOr()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(bool f1, bool f2)
    {
        bool f = f1 [||]^ f2;
    }
}
", @"
class C
{
    void M(bool f1, bool f2)
    {
        bool f = f2 ^ f1;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.SwapBinaryOperands)]
        public async Task TestNoRefactoring()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M(object x, bool f)
    {
        if (x =[||]= null) { }
        if (x ![||]= null) { }
        if (f =[||]= false) { }
        if (f =[||]= true) { }
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
