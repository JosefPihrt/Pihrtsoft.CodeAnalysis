// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0031AddNewLineBeforeEnumMemberTests : AbstractCSharpDiagnosticVerifier<AddNewLineBeforeEnumMemberAnalyzer, EnumDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddNewLineBeforeEnumMember;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEnumMember)]
        public async Task Test_WithoutExplicitValues()
        {
            await VerifyDiagnosticAndFixAsync(@"
enum Foo
{
    A, [||]B, C, D,
}
", @"
enum Foo
{
    A,
    B,
    C,
    D,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEnumMember)]
        public async Task Test_WithExplicitValues()
        {
            await VerifyDiagnosticAndFixAsync(@"
enum Foo
{
    A = 0, [||]B = 1, C = 2, D = 3,
}
", @"
enum Foo
{
    A = 0,
    B = 1,
    C = 2,
    D = 3,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEnumMember)]
        public async Task Test_WithoutTrailingComma()
        {
            await VerifyDiagnosticAndFixAsync(@"
enum Foo
{
    A, [||]B, C, D
}
", @"
enum Foo
{
    A,
    B,
    C,
    D
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEnumMember)]
        public async Task TestNoDiagnostic_SingleMember()
        {
            await VerifyNoDiagnosticAsync(@"
enum Foo
{
    A
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEnumMember)]
        public async Task TestNoDiagnostic_MultipleMembers()
        {
            await VerifyNoDiagnosticAsync(@"
enum Foo
{
    A,
    B,
    C,
}
");
        }
    }
}
