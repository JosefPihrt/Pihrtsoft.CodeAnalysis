// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1129RemoveRedundantFieldInitializationTests : AbstractCSharpDiagnosticVerifier<RemoveRedundantFieldInitializationAnalyzer, VariableDeclaratorCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.RemoveRedundantFieldInitialization;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantFieldInitialization)]
        public async Task Test_MultipleDeclarations()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object _f [|= null|], _f2;
}
", @"
class C
{
    object _f, _f2;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantFieldInitialization)]
        public async Task Test_Number()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    sbyte _sb [|= (sbyte)0|];
    byte _be [|= (byte)0|];
    short _st [|= (short)0|];
    ushort _us [|= (ushort)0|];
    int _ii [|= (int)0|];
    uint _ui [|= (uint)0|];
    long _lg [|= (long)0|];
    ulong _ul [|= (ulong)0|];
    float _ft [|= (float)0|];
    double _de [|= (double)0|];
    decimal _dl [|= (decimal)0|];
}
", @"
class C
{
    sbyte _sb;
    byte _be;
    short _st;
    ushort _us;
    int _ii;
    uint _ui;
    long _lg;
    ulong _ul;
    float _ft;
    double _de;
    decimal _dl;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantFieldInitialization)]
        public async Task Test_Bool()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    bool _f [|= false|];
}
", @"
class C
{
    bool _f;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantFieldInitialization)]
        public async Task Test_Bool_Nullable()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    bool? _f [|= null|];
    bool? _f2 [|= default(bool?)|];
}
", @"
class C
{
    bool? _f;
    bool? _f2;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantFieldInitialization)]
        public async Task Test_BoolConst()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    const bool K = false;

    bool _f [|= K|];
}
", @"
class C
{
    const bool K = false;

    bool _f;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantFieldInitialization)]
        public async Task Test_Char()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    char _f [|= '\0'|];
}
", @"
class C
{
    char _f;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantFieldInitialization)]
        public async Task Test_CharConst()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    const char K = '\0';

    char _f [|= K|];
}
", @"
class C
{
    const char K = '\0';

    char _f;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantFieldInitialization)]
        public async Task Test_Int()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    int _f [|= 0|];
}
", @"
class C
{
    int _f;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantFieldInitialization)]
        public async Task Test_IntConst()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    const int K = 0;

    int _f [|= K|];
}
", @"
class C
{
    const int K = 0;

    int _f;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantFieldInitialization)]
        public async Task Test_ULong()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    ulong _f [|= 0|];
}
", @"
class C
{
    ulong _f;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantFieldInitialization)]
        public async Task Test_ULongConst()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    const ulong K = 0;

    ulong _f [|= K|];
}
", @"
class C
{
    const ulong K = 0;

    ulong _f;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantFieldInitialization)]
        public async Task Test_Enum()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    const RegexOptions RegexOptionsConst = RegexOptions.None;

    RegexOptions _f1 [|= 0|];
    RegexOptions _f2 [|= RegexOptions.None|];
    RegexOptions _f3 [|= (RegexOptions)0|];
    RegexOptions _f4 [|= RegexOptionsConst|];
}
", @"
using System.Text.RegularExpressions;

class C
{
    const RegexOptions RegexOptionsConst = RegexOptions.None;

    RegexOptions _f1;
    RegexOptions _f2;
    RegexOptions _f3;
    RegexOptions _f4;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantFieldInitialization)]
        public async Task TestString()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    const string K = null;

    string _s2 [|= null|];
    string _s3 [|= default(string)|];
    string _s4 [|= K|];
}
", @"
class C
{
    const string K = null;

    string _s2;
    string _s3;
    string _s4;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantFieldInitialization)]
        public async Task TestNoDiagnostic_SuppressNullableWarningExpression()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    string _f = null!;
}
", options: CSharpTestOptions.Default
                .AddAllowedCompilerDiagnosticId("CS0414")
                .WithParseOptions(CSharpTestOptions.Default.ParseOptions.WithLanguageVersion(LanguageVersion.CSharp9)));
        }
    }
}
