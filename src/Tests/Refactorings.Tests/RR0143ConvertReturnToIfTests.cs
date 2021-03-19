// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0143ConvertReturnToIfTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ConvertReturnToIf;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertReturnToIf)]
        public async Task Test_ReturnStatement()
        {
            await VerifyRefactoringAsync(@"
class C
{
    bool M(bool f)
    {
        [||]return f;
    }
}
", @"
class C
{
    bool M(bool f)
    {
        if (f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertReturnToIf)]
        public async Task Test_ReturnStatement_SelectEntireStatement()
        {
            await VerifyRefactoringAsync(@"
class C
{
    bool M(bool f)
    {
        [|return f;|]
    }
}
", @"
class C
{
    bool M(bool f)
    {
        if (f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertReturnToIf)]
        public async Task Test_YieldReturnStatement()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<bool> M(bool f)
    {
        [||]yield return f;
    }
}
", @"
using System.Collections.Generic;

class C
{
    IEnumerable<bool> M(bool f)
    {
        if (f)
        {
            yield return true;
        }
        else
        {
            yield return false;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertReturnToIf)]
        public async Task Test_YieldReturnStatement_SelectEntireStatement()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<bool> M(bool f)
    {
        [|yield return f;|]
    }
}
", @"
using System.Collections.Generic;

class C
{
    IEnumerable<bool> M(bool f)
    {
        if (f)
        {
            yield return true;
        }
        else
        {
            yield return false;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertReturnToIf)]
        public async Task TestNoRefactoring_NotBooleanExpression()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    string M(string s)
    {
        [||]return s;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertReturnToIf)]
        public async Task TestNoRefactoring_TrueLiteralExpression()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    bool M()
    {
        [||]return true;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertReturnToIf)]
        public async Task TestNoRefactoring_FalseLiteralExpression()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    bool M()
    {
        [||]return false;
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
