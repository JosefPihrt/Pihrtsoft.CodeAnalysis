// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1191DeclareEnumValueAsCombinationOfNamesTests : AbstractCSharpDiagnosticVerifier<EnumSymbolAnalyzer, EnumMemberDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.DeclareEnumValueAsCombinationOfNames;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DeclareEnumValueAsCombinationOfNames)]
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
    ABD = [|11|],
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
    ABD = A | B | D,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DeclareEnumValueAsCombinationOfNames)]
        public async Task Test2()
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
    ABCD = [|15|],
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
    ABCD = A | B | C | D,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DeclareEnumValueAsCombinationOfNames)]
        public async Task Test3()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    B = 2,
    AB = A | B,
    C = 4,
    D = 8,
    ABD = [|11|],
}
", @"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    B = 2,
    AB = A | B,
    C = 4,
    D = 8,
    ABD = AB | D,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DeclareEnumValueAsCombinationOfNames)]
        public async Task Test4()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

[Flags]
enum Foo
{
    None = 0,
    AB = 3,
    C = 4,
    D = 8,
    ABD = [|11|],
}
", @"
using System;

[Flags]
enum Foo
{
    None = 0,
    AB = 3,
    C = 4,
    D = 8,
    ABD = AB | D,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DeclareEnumValueAsCombinationOfNames)]
        public async Task Test5()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    B = 2,
    AB = A | B,
    C = 4,
    D = 8,
    ABD = ([|3 | D|]),
}
", @"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    B = 2,
    AB = A | B,
    C = 4,
    D = 8,
    ABD = (AB | D),
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DeclareEnumValueAsCombinationOfNames)]
        public async Task Test6()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    B = 2,
    AB = A | B,
    C = 4,
    D = 8,
    ABD = AB | D,
    ABCD = [|1 | 2 | C | D|],
}
", @"
using System;

[Flags]
enum Foo
{
    None = 0,
    A = 1,
    B = 2,
    AB = A | B,
    C = 4,
    D = 8,
    ABD = AB | D,
    ABCD = ABD | C,
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DeclareEnumValueAsCombinationOfNames)]
        public async Task Test_SByte()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

[Flags]
enum Foo : sbyte
{
    None = 0,
    A = 1,
    B = 2,
    AB = [|3|],
}
", @"
using System;

[Flags]
enum Foo : sbyte
{
    None = 0,
    A = 1,
    B = 2,
    AB = A | B,
}
");
        }
    }
}
