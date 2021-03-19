// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0139ConvertNullLiteralToDefaultExpressionTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ConvertNullLiteralToDefaultExpression;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertNullLiteralToDefaultExpression)]
        public async Task Test_Argument()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(C p)
    {
        M([||]null);
    }
}
", @"
class C
{
    void M(C p)
    {
        M(default(C));
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertNullLiteralToDefaultExpression)]
        public async Task Test_ReturnExpression()
        {
            await VerifyRefactoringAsync(@"
class C
{
    object M()
    {
        return [||]null;
    }
}
", @"
class C
{
    object M()
    {
        return default(object);
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertNullLiteralToDefaultExpression)]
        public async Task TestNoRefactoring_ParameterDefaultValue()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M(C p = [||]null)
    {
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
