// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0059AddMissingCasesTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.AddMissingCases;

        public override CSharpTestOptions Options
        {
            get { return base.Options.AddAllowedCompilerDiagnosticId(CompilerDiagnosticIdentifiers.EmptySwitchBlock); }
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddMissingCases)]
        public async Task Test_Empty()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M()
    {
        DayOfWeek dayOfWeek = DayOfWeek.Monday;

        [||]switch (dayOfWeek)
        {
        }
    }
}
", @"
using System;

class C
{
    void M()
    {
        DayOfWeek dayOfWeek = DayOfWeek.Monday;

        switch (dayOfWeek)
        {
            case DayOfWeek.Sunday:
                break;
            case DayOfWeek.Monday:
                break;
            case DayOfWeek.Tuesday:
                break;
            case DayOfWeek.Wednesday:
                break;
            case DayOfWeek.Thursday:
                break;
            case DayOfWeek.Friday:
                break;
            case DayOfWeek.Saturday:
                break;
            default:
                break;
        }
    }
}
",
equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddMissingCases)]
        public async Task Test_Default()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M()
    {
        DayOfWeek dayOfWeek = DayOfWeek.Monday;

        [||]switch (dayOfWeek)
        {
            default:
                break;
        }
    }
}
", @"
using System;

class C
{
    void M()
    {
        DayOfWeek dayOfWeek = DayOfWeek.Monday;

        switch (dayOfWeek)
        {
            case DayOfWeek.Sunday:
                break;
            case DayOfWeek.Monday:
                break;
            case DayOfWeek.Tuesday:
                break;
            case DayOfWeek.Wednesday:
                break;
            case DayOfWeek.Thursday:
                break;
            case DayOfWeek.Friday:
                break;
            case DayOfWeek.Saturday:
                break;
            default:
                break;
        }
    }
}
",
equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddMissingCases)]
        public async Task Test2()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M()
    {
        DayOfWeek dayOfWeek = DayOfWeek.Monday;

        [||]switch (dayOfWeek)
        {
            case (DayOfWeek.Friday):
                break;
            case DayOfWeek.Monday:
                break;
            case DayOfWeek.Saturday:
                break;
            case (DayOfWeek)0:
                break;
            case DayOfWeek.Thursday:
                break;
        }
    }
}
", @"
using System;

class C
{
    void M()
    {
        DayOfWeek dayOfWeek = DayOfWeek.Monday;

        switch (dayOfWeek)
        {
            case (DayOfWeek.Friday):
                break;
            case DayOfWeek.Monday:
                break;
            case DayOfWeek.Saturday:
                break;
            case (DayOfWeek)0:
                break;
            case DayOfWeek.Thursday:
                break;
            case DayOfWeek.Tuesday:
                break;
            case DayOfWeek.Wednesday:
                break;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddMissingCases)]
        public async Task Test_Flags()
        {
            await VerifyRefactoringAsync(@"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        RegexOptions options = RegexOptions.None;

        [||]switch (options)
        {
            case (RegexOptions.Compiled):
                break;
            case RegexOptions.CultureInvariant | RegexOptions.ECMAScript:
                break;
            case (RegexOptions)256:
                break;
            case System.Text.RegularExpressions.RegexOptions.ExplicitCapture:
                break;
            case RegexOptions.IgnoreCase:
                break;
            case RegexOptions.IgnorePatternWhitespace:
                break;
            case RegexOptions.Multiline:
                break;
            case RegexOptions.None:
                break;
            case RegexOptions.RightToLeft:
                break;
            case RegexOptions.Singleline:
                break;
            default:
                break;
        }
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        RegexOptions options = RegexOptions.None;

        switch (options)
        {
            case (RegexOptions.Compiled):
                break;
            case RegexOptions.CultureInvariant | RegexOptions.ECMAScript:
                break;
            case (RegexOptions)256:
                break;
            case System.Text.RegularExpressions.RegexOptions.ExplicitCapture:
                break;
            case RegexOptions.IgnoreCase:
                break;
            case RegexOptions.IgnorePatternWhitespace:
                break;
            case RegexOptions.Multiline:
                break;
            case RegexOptions.None:
                break;
            case RegexOptions.RightToLeft:
                break;
            case RegexOptions.Singleline:
                break;
            case RegexOptions.CultureInvariant:
                break;
            default:
                break;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddMissingCases)]
        public async Task Test_TwoFieldsWithSameValue()
        {
            await VerifyRefactoringAsync(@"
enum E
{
    A = 0,
    B = 1,
    C = 2,
    D = 2
}

class C
{
    void M()
    {
        var e = E.A;

        [||]switch (e)
        {
            case E.A:
                break;
        }
    }
}
", @"
enum E
{
    A = 0,
    B = 1,
    C = 2,
    D = 2
}

class C
{
    void M()
    {
        var e = E.A;

        switch (e)
        {
            case E.A:
                break;
            case E.B:
                break;
            case E.C:
                break;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddMissingCases)]
        public async Task TestNoRefactoring()
        {
            await VerifyNoRefactoringAsync(@"
using System;

class C
{
    void M()
    {
        DayOfWeek dayOfWeek = DayOfWeek.Monday;

        [||]switch (dayOfWeek)
        {
            case DayOfWeek.Friday:
                break;
            case DayOfWeek.Monday:
                break;
            case DayOfWeek.Saturday:
                break;
            case DayOfWeek.Sunday:
                break;
            case DayOfWeek.Thursday:
                break;
            case DayOfWeek.Tuesday:
                break;
            case DayOfWeek.Wednesday:
                break;
            default:
                break;
        }
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
