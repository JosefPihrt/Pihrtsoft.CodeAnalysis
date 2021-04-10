// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0012AddEmptyLineBetweenSingleLineDeclarationsTests : AbstractCSharpDiagnosticVerifier<EmptyLineBetweenDeclarationsAnalyzer, EmptyLineBetweenDeclarationsCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AddEmptyLineBetweenSingleLineDeclarations;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineDeclarations)]
        public async Task Test_MemberDeclaration()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string P1 { get; set; }[||]
    string P2 { get; set; }
}
", @"
class C
{
    string P1 { get; set; }

    string P2 { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineDeclarations)]
        public async Task Test_EnumMemberDeclaration()
        {
            await VerifyDiagnosticAndFixAsync(@"
enum E
{
    A = 0,[||]
    B = 1
}
", @"
enum E
{
    A = 0,

    B = 1
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineDeclarations)]
        public async Task TestNoDiagnostic_MemberDeclaration_FirstIsMultiline()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
    }
    string P { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineDeclarations)]
        public async Task TestNoDiagnostic_MemberDeclaration_SecondIsMultiline()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    string P { get; set; }
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineDeclarations)]
        public async Task TestNoDiagnostic_EnumMemberDeclaration_FirstIsMultiline()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

enum E
{
    [Obsolete]
    A = 0,
    B = 1
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineDeclarations)]
        public async Task TestNoDiagnostic_EnumMemberDeclaration_SecondIsMultiline()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

enum E
{
    A = 0,
    [Obsolete]
    B = 1
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineDeclarations)]
        public async Task TestNoDiagnostic_MemberDeclaration_DocumentationComment()
        {
            await VerifyNoDiagnosticAsync(@"
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

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineDeclarations)]
        public async Task TestNoDiagnostic_EnumMemberDeclaration_DocumentationComment()
        {
            await VerifyNoDiagnosticAsync(@"
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
    }
}
