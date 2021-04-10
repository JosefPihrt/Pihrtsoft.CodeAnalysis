// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1181ConvertCommentToDocumentationCommentTests : AbstractCSharpDiagnosticVerifier<ConvertCommentToDocumentationCommentAnalyzer, MemberDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.ConvertCommentToDocumentationComment;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task Test_LeadingComment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    [|// x|]
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// x
    /// </summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task Test_LeadingMultipleComments()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    [|// x
    // x|]
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// x
    /// x
    /// </summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task Test_TrailingComment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M() [|// x|]
    {
    }
}
", @"
class C
{
    /// <summary>
    /// x
    /// </summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task Test_TrailingComment2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M<T1, T2>() where T1 : class where T2 : class [|// x|]
    {
    }
}
", @"
class C
{
    /// <summary>
    /// x
    /// </summary>
    /// <typeparam name=""T1""></typeparam>
    /// <typeparam name=""T2""></typeparam>
    void M<T1, T2>() where T1 : class where T2 : class
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task Test_TrailingComment_EnumMember()
        {
            await VerifyDiagnosticAndFixAsync(@"
enum E
{
    A = 0 [|//x|]
}
", @"
enum E
{
    /// <summary>
    /// x
    /// </summary>
    A = 0
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task Test_TrailingComment_EnumMemberWithValueAndWithComma()
        {
            await VerifyDiagnosticAndFixAsync(@"
enum E
{
    A = 0, [|//x|]
    B = 1
}
", @"
enum E
{
    /// <summary>
    /// x
    /// </summary>
    A = 0,
    B = 1
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task Test_TrailingComment_EnumMemberWithoutValueAndWithComma()
        {
            await VerifyDiagnosticAndFixAsync(@"
enum E
{
    A, [|//x|]
    B
}
", @"
enum E
{
    /// <summary>
    /// x
    /// </summary>
    A,
    B
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task Test_LeadingTodoCommentTrailingComment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    //TODO x
    void M() [|// y|]
    {
    }
}
", @"
class C
{
    //TODO x
    /// <summary>
    /// y
    /// </summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task TestNoDiagnostic_DocumentationComment()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    /// <summary>
    /// x
    /// </summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task TestNoDiagnostic_DocumentationCommentAndTrailingComment()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    /// <summary>
    /// x
    /// </summary>
    void M() // x
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task TestNoDiagnostic_DocumentationComment_DocumentationModeIsEqualToNone()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    /// <summary>
    /// x
    /// </summary>
    void M()
    {
    }
}
", options: Options.WithParseOptions(Options.ParseOptions.WithDocumentationMode(DocumentationMode.None)));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task TestNoDiagnostic_DocumentationCommentAndTrailingComment_DocumentationModeIsEqualToNone()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    /// <summary>
    /// x
    /// </summary>
    void M() // x
    {
    }
}
", options: Options.WithParseOptions(Options.ParseOptions.WithDocumentationMode(DocumentationMode.None)));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task TestNoDiagnostic_LeadingTaskListItem()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    //todo
    object M1() => null;

    // todo
    object M2() => null;

    //todo 
    object M3() => null;

    // todo 
    object M4() => null;

    //todo x
    object M5() => null;

    // todo x
    object M6() => null;

    //todo: x
    object M7() => null;

    //TODO
    object M8() => null;

    //undone
    object M9() => null;

    //hack
    object M10() => null;

    //unresolvedmergeconflict
    object M11() => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task TestNoDiagnostic_TrailingTaskListItem()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M1() //todo
    {
    }

}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertCommentToDocumentationComment)]
        public async Task TestNoDiagnostic_LeadingTaskListItemWithNonTaskListItem()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    //todo
    //x
    void M1() //x
    {
    }
}
");
        }
    }
}
