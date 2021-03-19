// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0214ConvertSwitchExpressionToSwitchStatementTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ConvertSwitchExpressionToSwitchStatement;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertSwitchExpressionToSwitchStatement)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    int M()
    {
        var dayOfWeek = DayOfWeek.Monday;

        return dayOfWeek [||]switch
        {
            DayOfWeek.Monday => 1,
            DayOfWeek.Tuesday => 2,
            DayOfWeek.Wednesday => 3,
            DayOfWeek.Thursday => 4,
            DayOfWeek.Friday => 5,
            _ => throw new Exception(),
        };
    }
}
", @"
using System;

class C
{
    int M()
    {
        var dayOfWeek = DayOfWeek.Monday;

        switch (dayOfWeek)
        {
            case DayOfWeek.Monday:
                return 1;
            case DayOfWeek.Tuesday:
                return 2;
            case DayOfWeek.Wednesday:
                return 3;
            case DayOfWeek.Thursday:
                return 4;
            case DayOfWeek.Friday:
                return 5;
            default:
                throw new Exception();
        }
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
