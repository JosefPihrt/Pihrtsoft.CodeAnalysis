// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1237UseBitShiftOperatorTests : AbstractCSharpDiagnosticVerifier<EnumSymbolAnalyzer, EnumDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseBitShiftOperator;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseBitShiftOperator)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

[Flags]
enum [|Foo|]
{
    _ = 0,
    A = 1,
    B = 2,
    AB = 3,
    C = (4),
}
", @"
using System;

[Flags]
enum Foo
{
    _ = 0,
    A = 1,
    B = 1 << 1,
    AB = 3,
    C = (1 << 2),
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseBitShiftOperator)]
        public async Task TestNoDiagnostic_WithoutFlags()
        {
            await VerifyNoDiagnosticAsync(@"
enum Foo
{
    _ = 0,
    A = 1,
    B = 2,
    C = 4,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseBitShiftOperator)]
        public async Task TestNoDiagnostic_BitShift()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

[Flags]
enum Foo
{
    _ = 0,
    A = 1,
    B = 1 << 1,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseBitShiftOperator)]
        public async Task TestNoDiagnostic_CombinedValue()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

[Flags]
enum Foo
{
    _ = 0,
    A = 1,
    B = 1 << 1,
    C = 1 << 2,
    D = 1 << 3,
    AB = 3,
    AC = 1 | 4,
    AD = 0b1001,
    X = int.MaxValue,
}
");
        }
    }
}
