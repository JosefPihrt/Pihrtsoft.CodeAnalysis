// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0085PromoteLocalToParameterTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.PromoteLocalToParameter;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.PromoteLocalToParameter)]
        public async Task Test_MethodWithSingleLocalDeclaration()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        var [||]value = new object();
    }
}
", @"
class C
{
    void M(object value)
    {
        value = new object();
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.PromoteLocalToParameter)]
        public async Task Test_LocalFunctionWithSingleLocalDeclaration()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        void LF()
        {
            var [||]value = new object();
        }
    }
}
", @"
class C
{
    void M()
    {
        void LF(object value)
        {
            value = new object();
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.PromoteLocalToParameter)]
        public async Task Test_MethodWithMultipleLocalDeclarationsWithoutInitialization()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(object parameter)
    {
        string [||]x, y;
    }
}
", @"
class C
{
    void M(object parameter, string x)
    {
        string y;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.PromoteLocalToParameter)]
        public async Task Test_MethodWithMultipleLocalDeclarationsWithInitialization()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(object parameter)
    {
        string [||]x = null, y = null;
    }
}
", @"
class C
{
    void M(object parameter, string x)
    {
        string y = null;
        x = null;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.PromoteLocalToParameter)]
        public async Task TestNoRefactoring_TypeDoesNotSupportExplicitDeclaration()
        {
            await VerifyNoRefactoringAsync(@"
using System.Linq;

class C
{
    void M()
    {
        var [||]q = Enumerable.Range(1, 1).Select(f => new { Value = f });
        var [||]q2 = q;
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
