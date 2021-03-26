// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1247FixDocumentationCommentTagTests : AbstractCSharpDiagnosticVerifier<SingleLineDocumentationCommentTriviaAnalyzer, XmlNodeCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.FixDocumentationCommentTag;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixDocumentationCommentTag)]
        public async Task Test_C_Multiline()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary>
    /// [|<c>
    /// a
    /// </c>|]
    /// <para>
    /// [|<c>
    /// b
    /// </c>|]
    /// </para>
    /// </summary>
    /// <param name=""p"">
    /// [|<c>
    /// c
    /// </c>|]
    /// </param>
    /// <returns>
    /// [|<c>
    /// d
    /// </c>|]
    /// </returns>
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// <code>
    /// a
    /// </code>
    /// <para>
    /// <code>
    /// b
    /// </code>
    /// </para>
    /// </summary>
    /// <param name=""p"">
    /// <code>
    /// c
    /// </code>
    /// </param>
    /// <returns>
    /// <code>
    /// d
    /// </code>
    /// </returns>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixDocumentationCommentTag)]
        public async Task Test_C_Multiline_List()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary>
    /// <list type=""bullet"">
    /// <listheader>
    /// <term>
    /// [|<c>
    /// a
    /// </c>|]
    /// </term>
    /// <description>
    /// [|<c>
    /// b
    /// </c>|]
    /// </description>
    /// </listheader>
    /// <item>
    /// <term>
    /// [|<c>
    /// c
    /// </c>|]
    /// </term>
    /// <description>
    /// [|<c>
    /// d
    /// </c>|]
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// <list type=""bullet"">
    /// <listheader>
    /// <term>
    /// <code>
    /// a
    /// </code>
    /// </term>
    /// <description>
    /// <code>
    /// b
    /// </code>
    /// </description>
    /// </listheader>
    /// <item>
    /// <term>
    /// <code>
    /// c
    /// </code>
    /// </term>
    /// <description>
    /// <code>
    /// d
    /// </code>
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixDocumentationCommentTag)]
        public async Task Test_Code_Singleline()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary>
    /// [|<code>a</code>|]
    /// <para>
    /// [|<code>b</code>|]
    /// </para>
    /// </summary>
    /// <param name=""p"">
    /// [|<code>c</code>|]
    /// </param>
    /// <returns>
    /// [|<code>d</code>|]
    /// </returns>
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// <c>a</c>
    /// <para>
    /// <c>b</c>
    /// </para>
    /// </summary>
    /// <param name=""p"">
    /// <c>c</c>
    /// </param>
    /// <returns>
    /// <c>d</c>
    /// </returns>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixDocumentationCommentTag)]
        public async Task Test_Code_Singleline_List()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary>
    /// <list type=""bullet"">
    /// <listheader>
    /// <term>
    /// [|<code>a</code>|]
    /// </term>
    /// <description>
    /// [|<code>b</code>|]
    /// </description>
    /// </listheader>
    /// <item>
    /// <term>
    /// [|<code>c</code>|]
    /// </term>
    /// <description>
    /// [|<code>d</code>|]
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// <list type=""bullet"">
    /// <listheader>
    /// <term>
    /// <c>a</c>
    /// </term>
    /// <description>
    /// <c>b</c>
    /// </description>
    /// </listheader>
    /// <item>
    /// <term>
    /// <c>c</c>
    /// </term>
    /// <description>
    /// <c>d</c>
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixDocumentationCommentTag)]
        public async Task TestNoDiagnostic_C_Singleline()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    /// <summary>
    /// <c></c>
    /// <c>x</c>
    /// <para>
    /// <c>b</c>
    /// </para>
    /// </summary>
    /// <param name=""p""><c>x</c></param>
    /// <returns>
    /// <c>x</c>
    /// </returns>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixDocumentationCommentTag)]
        public async Task TestNoDiagnostic_Code_Multiline()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    /// <summary>
    /// <code>
    /// </code>
    /// <code>
    /// x
    /// </code>
    /// <para>
    /// <code>
    /// x
    /// </code>
    /// </para>
    /// </summary>
    /// <param name=""p"">
    /// <code>
    /// x
    /// </code>
    /// </param>
    /// <returns>
    /// <code>
    /// x
    /// </code>
    /// </returns>
    void M()
    {
    }
}
");
        }
    }
}
