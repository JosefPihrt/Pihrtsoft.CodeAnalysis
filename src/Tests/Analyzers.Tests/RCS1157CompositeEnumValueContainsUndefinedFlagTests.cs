// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1157CompositeEnumValueContainsUndefinedFlagTests : AbstractCSharpDiagnosticVerifier<EnumSymbolAnalyzer, CompositeEnumValueContainsUndefinedFlagCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.CompositeEnumValueContainsUndefinedFlag;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.CompositeEnumValueContainsUndefinedFlag)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    B = 2,
    C = 4,
    D = 8,
    ABD = 11,
    ABCD = 15,
    [|X = 17|]
}
", @"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    B = 2,
    C = 4,
    D = 8,
    ABD = 11,
    ABCD = 15,
    X = 17,
    EnumMember = 16
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.CompositeEnumValueContainsUndefinedFlag)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    B = 2,
    C = 4,
    D = 8,
    ABD = 11,
    ABCD = 15,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.CompositeEnumValueContainsUndefinedFlag)]
        public async Task TestNoDiagnostic_NegativeValue()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

[Flags]
public enum E
{
    A = 1 << 31
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.CompositeEnumValueContainsUndefinedFlag)]
        public async Task TestNoDiagnostic_MaxValue()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

[Flags]
public enum E
{
    A = int.MaxValue
}
");
        }
    }
}
