// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1198AvoidBoxingOfValueTypeTests : AbstractCSharpDiagnosticVerifier<AvoidBoxingOfValueTypeAnalyzer, AvoidBoxingOfValueTypeCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AvoidBoxingOfValueType;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidBoxingOfValueType)]
        public async Task Test_Interpolation()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    public TimeSpan P { get; }

    void M()
    {
        var c = new C();

        var x = $""{[|c?.P.TotalMilliseconds|]}"";
    }
}
", @"
using System;

class C
{
    public TimeSpan P { get; }

    void M()
    {
        var c = new C();

        var x = $""{(c?.P.TotalMilliseconds).ToString()}"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidBoxingOfValueType)]
        public async Task Test_Interpolation_NullableType()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        int? i = null;

        string s = $""{[|i|]}"";
    }
}
", @"
class C
{
    void M()
    {
        int? i = null;

        string s = $""{i?.ToString()}"";
    }
}
");
        }

        // https://github.com/dotnet/roslyn/pull/35006
        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidBoxingOfValueType)]
        public async Task TestNoDiagnostic_StringConcatenation()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        int i = 0;
        string s = """" + i;
    }
}
");
        }

        // https://github.com/dotnet/roslyn/pull/35006
        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidBoxingOfValueType)]
        public async Task TestNoDiagnostic_InterpolatedString()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        int i = 0;
        string s = """";

        s = $""{i,1}"";
        s = $""{i:f}"";
        s = $""{i,1:f}"";
    }
}
");
        }
    }
}
