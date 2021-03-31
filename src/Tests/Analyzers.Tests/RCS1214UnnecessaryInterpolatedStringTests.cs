// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1214UnnecessaryInterpolatedStringTests : AbstractCSharpDiagnosticVerifier<UnnecessaryInterpolatedStringAnalyzer, InterpolatedStringCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UnnecessaryInterpolatedString;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryInterpolatedString)]
        public async Task Test_StringLiteral()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = [|$""{""""}""|];
    }
}
", @"
class C
{
    void M()
    {
        string s = """";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryInterpolatedString)]
        public async Task Test_InterpolatedString()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = null;
        s = [|$""{$""{s}""}""|];
    }
}
", @"
class C
{
    void M()
    {
        string s = null;
        s = $""{s}"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryInterpolatedString)]
        public async Task Test_NonNullStringConstant()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        const string x = """";
        string s = [|$""{x}""|];
    }
}
", @"
class C
{
    void M()
    {
        const string x = """";
        string s = x;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryInterpolatedString)]
        public async Task Test_NoInterpolation()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = [|$|]""abc"";
    }
}
", @"
class C
{
    void M()
    {
        string s = ""abc"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryInterpolatedString)]
        public async Task Test_NoInterpolation2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = [|$|]@""abc"";
    }
}
", @"
class C
{
    void M()
    {
        string s = @""abc"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryInterpolatedString)]
        public async Task Test_NoInterpolation3()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = @[|$|]""abc"";
    }
}
", @"
class C
{
    void M()
    {
        string s = @""abc"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryInterpolatedString)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        const string x = null;
        string s = $""{x}"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryInterpolatedString)]
        public async Task TestNoDiagnostic_FormattableString()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    string Call(FormattableString s) => Call($"""");
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryInterpolatedString)]
        public async Task TestNoDiagnostic_FormattableString2()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    string Call(FormattableString s) => Call($""x"");
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryInterpolatedString)]
        public async Task TestNoDiagnostic_FormattableString3()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    string Call(FormattableString s)
    {
        string x = null;
        return Call($""{""x""}"");
    }

}
");
        }
    }
}
