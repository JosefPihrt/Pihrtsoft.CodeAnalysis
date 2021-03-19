// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0117ReplaceAsWithCastTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ReplaceAsWithCast;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ReplaceAsWithCast)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        object x = null;

        var y = x [||]as C;
    }
}
", @"
class C
{
    void M()
    {
        object x = null;

        var y = (C)x;
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
