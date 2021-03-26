// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1135DeclareEnumMemberWithZeroValueTests : AbstractCSharpDiagnosticVerifier<EnumSymbolAnalyzer, DeclareEnumMemberWithZeroValueCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.DeclareEnumMemberWithZeroValue;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DeclareEnumMemberWithZeroValue)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

[Flags]
enum [|Foo|]
{
    A = 1
}
", @"
using System;

[Flags]
enum Foo
{
    {|r:None|} = 0,
    A = 1
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DeclareEnumMemberWithZeroValue)]
        public async Task TestNoDiagnostic_HasZeroValue()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    B = 2
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DeclareEnumMemberWithZeroValue)]
        public async Task TestNoDiagnostic_WithoutFlags()
        {
            await VerifyNoDiagnosticAsync(@"
enum Foo
{
    A = 1,
    B = 2
}
");
        }
    }
}
