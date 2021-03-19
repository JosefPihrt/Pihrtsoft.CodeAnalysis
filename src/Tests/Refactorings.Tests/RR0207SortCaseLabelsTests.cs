// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0207SortCaseLabelsTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.SortCaseLabels;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.SortCaseLabels)]
        public async Task Test_StringLiteral()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(string s)
    {
        switch (s)
        {
[|            case ""d"":
            case ""a"":
            case ""c"":|]
            case ""b"":
                break;
            default:
                break;
        }
    }
}
", @"
class C
{
    void M(string s)
    {
        switch (s)
        {
            case ""a"":
            case ""c"":
            case ""d"":
            case ""b"":
                break;
            default:
                break;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.SortCaseLabels)]
        public async Task Test_SimpleMemberAccessExpression()
        {
            await VerifyRefactoringAsync(@"
using System.Text.RegularExpressions;

class C
{
    void M(RegexOptions options)
    {
        switch (options)
        {
            case RegexOptions.CultureInvariant:
[|            case RegexOptions.Compiled:
            case RegexOptions.Singleline:
            case RegexOptions.ExplicitCapture:
            case RegexOptions.Multiline:
            case RegexOptions.IgnoreCase:
            case RegexOptions.IgnorePatternWhitespace:
            case RegexOptions.ECMAScript:
            case RegexOptions.RightToLeft:|]
                break;
        }
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    void M(RegexOptions options)
    {
        switch (options)
        {
            case RegexOptions.CultureInvariant:
            case RegexOptions.Compiled:
            case RegexOptions.ECMAScript:
            case RegexOptions.ExplicitCapture:
            case RegexOptions.IgnoreCase:
            case RegexOptions.IgnorePatternWhitespace:
            case RegexOptions.Multiline:
            case RegexOptions.RightToLeft:
            case RegexOptions.Singleline:
                break;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.SortCaseLabels)]
        public async Task TestNoRefactoring_StringLiteral_IsSorted()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M(string s)
    {
        switch (s)
        {
[|            case ""a"":
            case ""b"":
            case ""c"":
            case ""d"":|]
                break;
            default:
                break;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.SortCaseLabels)]
        public async Task TestNoRefactoring_SimpleMemberAccessExpression_IsSorted()
        {
            await VerifyNoRefactoringAsync(@"
using System.Text.RegularExpressions;

class C
{
    void M(RegexOptions options)
    {
        switch (options)
        {
[|            case RegexOptions.Compiled:
            case RegexOptions.CultureInvariant:
            case RegexOptions.ECMAScript:
            case RegexOptions.ExplicitCapture:
            case RegexOptions.IgnoreCase:
            case RegexOptions.IgnorePatternWhitespace:
            case RegexOptions.Multiline:
            case RegexOptions.None:
            case RegexOptions.RightToLeft:
            case RegexOptions.Singleline:|]
                break;
        }
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
