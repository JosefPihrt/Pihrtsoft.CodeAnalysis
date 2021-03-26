// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0147ConvertSwitchToIfTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ConvertSwitchToIf;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertSwitchToIf)]
        public async Task Test_WhenClause()
        {
            await VerifyRefactoringAsync(@"
class C
{
    bool M()
    {
        object x = 0;

        [||]switch (x)
        {
            case double y when (y < 1 || y >= 2):
                return false;
            case double z when (z >= 1 && z < 2):
                return true;
            default:
                return false;
        }
    }
}
", @"
class C
{
    bool M()
    {
        object x = 0;

        if (x is double y && (y < 1 || y >= 2))
        {
            return false;
        }
        else if (x is double z && (z >= 1 && z < 2))
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

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertSwitchToIf)]
        public async Task TestNoRefactoring()
        {
            await VerifyNoRefactoringAsync(@"
using System;

class C
{
    void M()
    {
        StringSplitOptions x = StringSplitOptions.None;

        [||]switch (x)
        {
            case StringSplitOptions.None:
            case StringSplitOptions.RemoveEmptyEntries:
            default:
                break;
        }
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
