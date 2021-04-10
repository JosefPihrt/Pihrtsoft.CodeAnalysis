// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1188RemoveRedundantAutoPropertyInitializationTests : AbstractCSharpDiagnosticVerifier<RemoveRedundantAutoPropertyInitializationAnalyzer, PropertyDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.RemoveRedundantAutoPropertyInitialization;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAutoPropertyInitialization)]
        public async Task Test_Bool()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    const bool K = false;

    bool P1 { get; set; } = [|false|];
    bool P2 { get; set; } = [|K|];
    bool P3 { get; set; } = [|default|];
}
", @"
class C
{
    const bool K = false;

    bool P1 { get; set; }
    bool P2 { get; set; }
    bool P3 { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAutoPropertyInitialization)]
        public async Task Test_Bool_Nullable()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    const bool K = false;

    bool? P1 { get; set; } = [|null|];
    bool? P2 { get; set; } = [|default(bool?)|];
    bool? P3 { get; set; } = [|default|];
}
", @"
class C
{
    const bool K = false;

    bool? P1 { get; set; }
    bool? P2 { get; set; }
    bool? P3 { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAutoPropertyInitialization)]
        public async Task Test_Char()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    const char K = '\0';

    char P1 { get; set; } = [|'\0'|];
    char P2 { get; set; } = [|K|];
    char P3 { get; set; } = [|default|];
}
", @"
class C
{
    const char K = '\0';

    char P1 { get; set; }
    char P2 { get; set; }
    char P3 { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAutoPropertyInitialization)]
        public async Task Test_String()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    const string K = null;

    string P1 { get; set; } = [|null|];
    string P2 { get; set; } = [|default(string)|];
    string P3 { get; set; } = [|default|];
    string P4 { get; set; } = [|K|];
}
", @"
class C
{
    const string K = null;

    string P1 { get; set; }
    string P2 { get; set; }
    string P3 { get; set; }
    string P4 { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAutoPropertyInitialization)]
        public async Task Test_Numeric_Int()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    const int K = 0;

    int P1 { get; set; } = [|0|];
    int P2 { get; set; } = [|(int)0|];
    int P3 { get; set; } = [|K|];
    int P4 { get; set; } = [|default|];
}
", @"
class C
{
    const int K = 0;

    int P1 { get; set; }
    int P2 { get; set; }
    int P3 { get; set; }
    int P4 { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAutoPropertyInitialization)]
        public async Task Test_ULong()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    const ulong K = 0;

    ulong P1 { get; set; } = [|(ulong)0|];
    ulong P2 { get; set; } = [|K|];
    ulong P3 { get; set; } = [|default|];
}
", @"
class C
{
    const ulong K = 0;

    ulong P1 { get; set; }
    ulong P2 { get; set; }
    ulong P3 { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAutoPropertyInitialization)]
        public async Task Test_Numeric()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    sbyte PSB { get; set; } = [|(sbyte)0|];
    byte PB { get; set; } = [|(byte)0|];
    short PS { get; set; } = [|(short)0|];
    ushort PUS { get; set; } = [|(ushort)0|];
    uint PUI { get; set; } = [|(uint)0|];
    long PL { get; set; } = [|(long)0|];
    float PF { get; set; } = [|(float)0|];
    double PDO { get; set; } = [|(double)0|];
    decimal PDE { get; set; } = [|(decimal)0|];
}
", @"
class C
{
    sbyte PSB { get; set; }
    byte PB { get; set; }
    short PS { get; set; }
    ushort PUS { get; set; }
    uint PUI { get; set; }
    long PL { get; set; }
    float PF { get; set; }
    double PDO { get; set; }
    decimal PDE { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAutoPropertyInitialization)]
        public async Task TestNoDiagnostic_NoInitializer()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    string P { get; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAutoPropertyInitialization)]
        public async Task TestNoDiagnostic_SuppressNullableWarningExpression()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    string P { get; } = null!;
}
", options: CSharpTestOptions.Default.WithParseOptions(CSharpTestOptions.Default.ParseOptions.WithLanguageVersion(LanguageVersion.Preview)));
        }
    }
}
