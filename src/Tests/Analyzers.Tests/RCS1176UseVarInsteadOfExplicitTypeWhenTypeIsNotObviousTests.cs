// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1176UseVarInsteadOfExplicitTypeWhenTypeIsNotObviousTests : AbstractCSharpDiagnosticVerifier<UseVarInsteadOfExplicitTypeWhenTypeIsNotObviousAnalyzer, UseVarInsteadOfExplicitTypeCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.UseVarInsteadOfExplicitTypeWhenTypeIsNotObvious;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseVarInsteadOfExplicitTypeWhenTypeIsNotObvious)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object M()
    {
        [|object|] x = M();

        return default;
    }
}
", @"
class C
{
    object M()
    {
        var x = M();

        return default;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseVarInsteadOfExplicitTypeWhenTypeIsNotObvious)]
        public async Task Test_TupleExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    (object x, object y) M()
    {
        [|(object x, object y)|] = M();

        return default;
    }
}
", @"
class C
{
    (object x, object y) M()
    {
        var (x, y) = M();

        return default;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseVarInsteadOfExplicitTypeWhenTypeIsNotObvious)]
        public async Task Test_TupleExpression_Var()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    (object x, object y) M()
    {
        [|(var x, object y)|] = M();

        return default;
    }
}
", @"
class C
{
    (object x, object y) M()
    {
        var (x, y) = M();

        return default;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseVarInsteadOfExplicitTypeWhenTypeIsNotObvious)]
        public async Task Test_DiscardDesignation()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        if (int.TryParse("""", out [|int|] result))
        {
        }
    }
}
", @"
class C
{
    void M()
    {
        if (int.TryParse("""", out var result))
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseVarInsteadOfExplicitTypeWhenTypeIsNotObvious)]
        public async Task TestNoDiagnostic_ForEach_DeclarationExpression()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<(object x, string y)> M()
    {
        foreach (var (x, y) in M())
        {
        }

        return default;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseVarInsteadOfExplicitTypeWhenTypeIsNotObvious)]
        public async Task TestNoDiagnostic_ForEach_TupleExpression()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<(object x, string y)> M()
    {
        foreach ((object x, string y) in M())
        {
        }

        return default;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseVarInsteadOfExplicitTypeWhenTypeIsNotObvious)]
        public async Task TestNoDiagnostic_ParseMethod()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        TimeSpan timeSpan = TimeSpan.Parse(null);
    }
}
");
        }
    }
}
