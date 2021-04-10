// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1008UseExplicitTypeInsteadOfVarWhenTypeIsNotObviousTests : AbstractCSharpDiagnosticVerifier<UseExplicitTypeInsteadOfVarWhenTypeIsNotObviousAnalyzer, UseExplicitTypeInsteadOfVarCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.UseExplicitTypeInsteadOfVarWhenTypeIsNotObvious;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsNotObvious)]
        public async Task Test_LocalVariable()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        var a = ""a"";
        [|var|] s = a;
    }
}
", @"
class C
{
    void M()
    {
        var a = ""a"";
        string s = a;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsNotObvious)]
        public async Task Test_DeclarationExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        string value = null;
        if (DateTime.TryParse(value, out [|var|] result)) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        string value = null;
        if (DateTime.TryParse(value, out DateTime result)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsNotObvious)]
        public async Task Test_Tuple()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Collections.Generic;

class C
{
    (IEnumerable<DateTime> e1, string e2) M()
    {
        [|var|] x = M();

        return default((IEnumerable<DateTime>, string));
    }
}
", @"
using System;
using System.Collections.Generic;

class C
{
    (IEnumerable<DateTime> e1, string e2) M()
    {
        (IEnumerable<DateTime> e1, string e2) x = M();

        return default((IEnumerable<DateTime>, string));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsNotObvious)]
        public async Task Test_Parameter_NullableReferenceType()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string? p)
    {
        [|var|] s = p;
    }
}
", @"
class C
{
    void M(string? p)
    {
        string? s = p;
    }
}
", options: WellKnownCSharpTestOptions.Default_NullableReferenceTypes);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsNotObvious)]
        public async Task Test_Parameter_NullableReferenceType_Disable()
        {
            await VerifyDiagnosticAndFixAsync(@"
#nullable disable

class C
{
    void M(string? p)
    {
        [|var|] s = p;
    }
}
", @"
#nullable disable

class C
{
    void M(string? p)
    {
        string s = p;
    }
}
",
options: WellKnownCSharpTestOptions.Default_NullableReferenceTypes.AddAllowedCompilerDiagnosticId("CS8632"));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsNotObvious)]
        public async Task Test_Tuple_DeclarationExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    (object x, System.DateTime y) M()
    {
        [|var|] (x, y) = M();

        return default;
    }
}
", @"
class C
{
    (object x, System.DateTime y) M()
    {
        (object x, System.DateTime y) = M();

        return default;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsNotObvious)]
        public async Task Test_TupleExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    (object x, System.DateTime y) M()
    {
        (object x, [|var|] y) = M();

        return default;
    }
}
", @"
class C
{
    (object x, System.DateTime y) M()
    {
        (object x, System.DateTime y) = M();

        return default;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsNotObvious)]
        public async Task Test_TupleExpression_AllVar()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    (object x, System.DateTime y) M()
    {
        ([|var|] x, [|var|] y) = M();

        return default;
    }
}
", @"
class C
{
    (object x, System.DateTime y) M()
    {
        (object x, System.DateTime y) = M();

        return default;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsNotObvious)]
        public async Task Test_DiscardDesignation()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        if (int.TryParse("""", out [|var|] result))
        {
        }
    }
}
", @"
class C
{
    void M()
    {
        if (int.TryParse("""", out int result))
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsNotObvious)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        string a = ""a"";

        string s = a;

        string value = null;
        if (DateTime.TryParse(s, out DateTime result))
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsNotObvious)]
        public async Task TestNoDiagnostic_ForEach()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<(object x, System.DateTime y)> M()
    {
        foreach (var (x, y) in M())
        {
        }

        return default;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsNotObvious)]
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
