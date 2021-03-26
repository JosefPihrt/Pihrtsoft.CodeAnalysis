// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0078JoinStringExpressionsTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.JoinStringExpressions;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.JoinStringExpressions)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(string s)
    {
        s = s + [|""a"" + @""b""|];
    }
}
", @"
class C
{
    void M(string s)
    {
        s = s + ""ab"";
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.JoinStringExpressions)]
        public async Task Test_ToInterpolatedString()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M(string s1, string s2, string s3)
    {
        s1 = [|s1 + ""a"" + s2 +  @""b"" + s3|];
    }
}
", @"
class C
{
    void M(string s1, string s2, string s3)
    {
        s1 = $""{s1}a{s2}b{s3}"";
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.JoinStringExpressions)]
        public async Task Test_RegularAndInterpolated()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        string s = null;
        s = [|""{}"" + $""{s}""|];
    }
}
", @"
class C
{
    void M()
    {
        string s = null;
        s = $""{{}}{s}"";
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.JoinStringExpressions)]
        public async Task Test_VerbatimAndInterpolated()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        string s = null;
        s = [|@"" """" {} "" + $"" \"" {s} ""|];
    }
}
", @"
class C
{
    void M()
    {
        string s = null;
        s = $"" \"" {{}}  \"" {s} "";
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.JoinStringExpressions)]
        public async Task Test_RegularAndVerbatimInterpolated()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        string s = null;
        s = [|"" \"" {} "" + $@"" """" {s} ""|];
    }
}
", @"
class C
{
    void M()
    {
        string s = null;
        s = $"" \"" {{}}  \"" {s} "";
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.JoinStringExpressions)]
        public async Task Test_RegularAndMultilineVerbatim()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        string s = null;
        s = [|"" \r\n "" + @""
""|];
    }
}
", @"
class C
{
    void M()
    {
        string s = null;
        s = "" \r\n \r\n"";
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.JoinStringExpressions)]
        public async Task Test_ToMultilineStringLiteral()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        string s = null;
        s = [|""\r\n"" +
"" ""|];
    }
}
", @"
class C
{
    void M()
    {
        string s = null;
        s = @""
 "";
    }
}
", equivalenceKey: EquivalenceKey.Join(RefactoringId, "Multiline"));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.JoinStringExpressions)]
        public async Task Test_StringConcatenationOnMultipleLines_LeadingTriviaIncluded()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        string s =
[|            ""a"" +
            ""b"" +
            ""c""|];
    }
}
", @"
class C
{
    void M()
    {
        string s =
            ""abc"";
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.JoinStringExpressions)]
        public async Task Test_StringConcatenationOnMultipleLines_TrailingTriviaIncluded()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        string s =
            [|""a"" +
            ""b"" +
            ""c"" //|]
;
    }
}
", @"
class C
{
    void M()
    {
        string s =
            ""abc"" //
;
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
