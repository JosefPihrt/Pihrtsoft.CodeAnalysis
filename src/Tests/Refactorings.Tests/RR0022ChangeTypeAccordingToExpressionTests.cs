// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0022ChangeTypeAccordingToExpressionTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ChangeTypeAccordingToExpression;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ChangeVarToExplicitType)]
        public async Task Test_LocalVariable()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        [||]IEnumerable<object> x = new List<object>();
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        List<object> x = new List<object>();
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ChangeVarToExplicitType)]
        public async Task Test_ForEachVariable()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();
        foreach ([||]object item in items)
        {
        }
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();
        foreach (string item in items)
        {
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ChangeVarToExplicitType)]
        public async Task Test_NoRefactoring_NullLiteral()
        {
            await VerifyNoRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        [||]List<string> items = null;
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
