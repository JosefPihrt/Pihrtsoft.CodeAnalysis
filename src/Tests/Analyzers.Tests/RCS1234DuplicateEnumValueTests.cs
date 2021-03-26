// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1234DuplicateEnumValueTests : AbstractCSharpDiagnosticVerifier<EnumSymbolAnalyzer, EnumMemberDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.DuplicateEnumValue;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DuplicateEnumValue)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
enum E
{
    A = 1,
    B = 2,
    C = [|2|],
}
", @"
enum E
{
    A = 1,
    B = 2,
    C = B,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DuplicateEnumValue)]
        public async Task Test2()
        {
            await VerifyDiagnosticAndFixAsync(@"
enum E
{
    A = 1,
    C = 2,
    B = [|2|],
}
", @"
enum E
{
    A = 1,
    C = 2,
    B = C,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DuplicateEnumValue)]
        public async Task Test3()
        {
            await VerifyDiagnosticAndFixAsync(@"
enum E
{
    A = 1,
    B = 2,
    C = [|2|],
    D = [|2|],
}
", @"
enum E
{
    A = 1,
    B = 2,
    C = B,
    D = B,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DuplicateEnumValue)]
        public async Task Test4()
        {
            await VerifyDiagnosticAndFixAsync(@"
enum E
{
    A = 1,
    [|B|],
    C = 2,
}
", @"
enum E
{
    A = 1,
    B = C,
    C = 2,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DuplicateEnumValue)]
        public async Task Test5()
        {
            await VerifyDiagnosticAndFixAsync(@"
enum E
{
    B = 2,
    A = 1,
    [|C|],
}
", @"
enum E
{
    B = 2,
    A = 1,
    C = B,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DuplicateEnumValue)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
enum E
{
    A,
    B = A
}");
        }
    }
}
