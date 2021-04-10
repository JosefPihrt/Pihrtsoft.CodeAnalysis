// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1226AddParagraphToDocumentationCommentTests : AbstractCSharpDiagnosticVerifier<AddParagraphToDocumentationCommentAnalyzer, AddParagraphToDocumentationCommentCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AddParagraphToDocumentationComment;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task Test_TwoParagraphs_Summary()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary>
    ///[| a
    /// 
    /// b|]
    /// </summary>
    class C
    {
    }
}
", @"
namespace N
{
    /// <summary>
    /// <para>a</para>
    /// <para>b</para>
    /// </summary>
    class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task Test_TwoParagraphs_Returns()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary></summary>
    /// <returns>
    ///[| a
    /// 
    /// b|]
    /// </returns>
    class C
    {
    }
}
", @"
namespace N
{
    /// <summary></summary>
    /// <returns>
    /// <para>a</para>
    /// <para>b</para>
    /// </returns>
    class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task Test_TwoParagraphs_Remarks()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary></summary>
    /// <remarks>
    ///[| a
    /// 
    /// b|]
    /// </remarks>
    class C
    {
    }
}
", @"
namespace N
{
    /// <summary></summary>
    /// <remarks>
    /// <para>a</para>
    /// <para>b</para>
    /// </remarks>
    class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task Test_TwoParagraphs_Multiline()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary>
    ///[| a
    /// b
    /// 
    /// c
    /// d|]
    /// </summary>
    class C
    {
    }
}
", @"
namespace N
{
    /// <summary>
    /// <para>
    /// a
    /// b
    /// </para>
    /// <para>
    /// c
    /// d
    /// </para>
    /// </summary>
    class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task Test_TwoParagraphs_Multiline2()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary>
    ///[| a
    /// b
    /// 
    /// c|]
    /// </summary>
    class C
    {
    }
}
", @"
namespace N
{
    /// <summary>
    /// <para>
    /// a
    /// b
    /// </para>
    /// <para>c</para>
    /// </summary>
    class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task Test_TwoParagraphs_Multiline3()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary>
    ///[| a
    /// 
    /// c
    /// d|]
    /// </summary>
    class C
    {
    }
}
", @"
namespace N
{
    /// <summary>
    /// <para>a</para>
    /// <para>
    /// c
    /// d
    /// </para>
    /// </summary>
    class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task Test_ThreeParagraphs()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary>
    ///[| a
    /// 
    /// b|]
    /// 
    /// c
    /// </summary>
    class C
    {
    }
}
", @"
namespace N
{
    /// <summary>
    /// <para>a</para>
    /// <para>b</para>
    /// <para>c</para>
    /// </summary>
    class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task Test_ThreeParagraphs_Multiline()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary>
    ///[| a
    /// b
    /// 
    /// c
    /// d|]
    /// 
    /// e
    /// f
    /// </summary>
    class C
    {
    }
}
", @"
namespace N
{
    /// <summary>
    /// <para>
    /// a
    /// b
    /// </para>
    /// <para>
    /// c
    /// d
    /// </para>
    /// <para>
    /// e
    /// f
    /// </para>
    /// </summary>
    class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task Test_TwoParagraphs_ElementsOnly()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary>
    /// [|<c></c>
    /// <c></c>
    /// 
    /// <c>
    /// </c>|]
    /// </summary>
    class C
    {
    }
}
", @"
namespace N
{
    /// <summary>
    /// <para>
    /// <c></c>
    /// <c></c>
    /// </para>
    /// <para>
    /// <c>
    /// </c>
    /// </para>
    /// </summary>
    class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task Test_ClassWithAttribute()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

namespace N
{
    /// <summary>
    ///[| a
    /// 
    /// b|]
    /// </summary>
    [Obsolete]
    class C
    {
    }
}
", @"
using System;

namespace N
{
    /// <summary>
    /// <para>a</para>
    /// <para>b</para>
    /// </summary>
    [Obsolete]
    class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task Test_StartsWithParaElement()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary>
    /// <para>a</para>
    ///[| b
    /// 
    /// c|]
    /// </summary>
    class C
    {
    }
}
", @"
namespace N
{
    /// <summary>
    /// <para>a</para>
    /// <para>b</para>
    /// <para>c</para>
    /// </summary>
    class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task Test_ContainsParaElement()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary>
    /// a
    /// <para>b</para>
    ///[| c
    ///
    /// d|]
    /// </summary>
    class C
    {
    }
}
", @"
namespace N
{
    /// <summary>
    /// a
    /// <para>b</para>
    /// <para>c</para>
    /// <para>d</para>
    /// </summary>
    class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task Test_EndsWithParaElement()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary>
    ///[| a
    /// 
    /// b|]
    /// <para>c</para>
    /// </summary>
    class C
    {
    }
}
", @"
namespace N
{
    /// <summary>
    /// <para>a</para>
    /// <para>b</para>
    /// <para>c</para>
    /// </summary>
    class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task Test_EndsWithParaElement2()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary>
    ///[| a
    /// 
    /// b|]
    /// 
    /// c
    /// <para>d</para>
    /// </summary>
    class C
    {
    }
}
", @"
namespace N
{
    /// <summary>
    /// <para>a</para>
    /// <para>b</para>
    /// <para>c</para>
    /// <para>d</para>
    /// </summary>
    class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task TestNoDiagnostic_SimpleComment()
        {
            await VerifyNoDiagnosticAsync(@"
/// <summary>
/// a
/// </summary>
class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task TestNoDiagnostic_NoEmptyLine()
        {
            await VerifyNoDiagnosticAsync(@"
/// <summary>
/// a
/// b
/// </summary>
class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task TestNoDiagnostic_MissingEndTag()
        {
            await VerifyNoDiagnosticAsync(@"
/// <summary>
/// x
///
/// <a>x
///
/// </summary>
class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task TestNoDiagnostic_CodeElement()
        {
            await VerifyNoDiagnosticAsync(@"
/// <summary>
/// a
/// 
/// <code>
/// b
/// </code>
/// </summary>
class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task TestNoDiagnostic_CodeElement2()
        {
            await VerifyNoDiagnosticAsync(@"
/// <summary>
/// <code>
/// a
/// </code>
/// 
/// b
/// </summary>
class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task TestNoDiagnostic_InheritDocElement()
        {
            await VerifyNoDiagnosticAsync(@"
/// <summary>
/// a
/// 
/// <inheritdoc>b</inheritdoc>
/// </summary>
class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task TestNoDiagnostic_InheritDocElement2()
        {
            await VerifyNoDiagnosticAsync(@"
/// <summary>
/// <inheritdoc>a</inheritdoc>
/// 
/// b
/// </summary>
class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task TestNoDiagnostic_ListElement()
        {
            await VerifyNoDiagnosticAsync(@"
/// <summary>
/// a
/// 
/// <list type=""bullet"">
/// <item><description>b</description></item>
/// </list>
/// </summary>
class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParagraphToDocumentationComment)]
        public async Task TestNoDiagnostic_ListElement2()
        {
            await VerifyNoDiagnosticAsync(@"
/// <summary>
/// <list type=""bullet"">
/// <item><description>b</description></item>
/// </list>
/// 
/// b
/// </summary>
class C
{
}
");
        }
    }
}
