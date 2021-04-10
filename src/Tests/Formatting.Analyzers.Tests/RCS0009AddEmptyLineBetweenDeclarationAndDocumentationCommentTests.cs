// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0009AddEmptyLineBetweenDeclarationAndDocumentationCommentTests : AbstractCSharpDiagnosticVerifier<EmptyLineBetweenDeclarationsAnalyzer, EmptyLineBetweenDeclarationsCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AddEmptyLineBetweenDeclarationAndDocumentationComment;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenDeclarationAndDocumentationComment)]
        public async Task Test_MemberDeclaration_SingleLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string P1 { get; set; }[||]
    /// <summary>
    /// x
    /// </summary>
    string P2 { get; set; }
}
", @"
class C
{
    string P1 { get; set; }

    /// <summary>
    /// x
    /// </summary>
    string P2 { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenDeclarationAndDocumentationComment)]
        public async Task Test_MemberDeclaration_Multiline()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M1()
    {
    }[||]
    /// <summary>
    /// x
    /// </summary>
    void M2()
    {
    }
}
", @"
class C
{
    void M1()
    {
    }

    /// <summary>
    /// x
    /// </summary>
    void M2()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenDeclarationAndDocumentationComment)]
        public async Task Test_EnumMemberDeclaration_SingleLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
enum E
{
    A = 0,[||]
    /// <summary>
    /// x
    /// </summary>
    B = 1
}
", @"
enum E
{
    A = 0,

    /// <summary>
    /// x
    /// </summary>
    B = 1
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenDeclarationAndDocumentationComment)]
        public async Task Test_EnumMemberDeclaration_Multiline()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

enum E
{
    [Obsolete]
    A = 0,[||]
    /// <summary>
    /// x
    /// </summary>
    [Obsolete]
    B = 1
}
", @"
using System;

enum E
{
    [Obsolete]
    A = 0,

    /// <summary>
    /// x
    /// </summary>
    [Obsolete]
    B = 1
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenDeclarationAndDocumentationComment)]
        public async Task TestNoDiagnostic_MemberDeclaration()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M1()
    {
    }
    void M2()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenDeclarationAndDocumentationComment)]
        public async Task TestNoDiagnostic_EnumMemberDeclaration()
        {
            await VerifyNoDiagnosticAsync(@"
enum E
{
    A = 0,
    B = 1
}
");
        }
    }
}
