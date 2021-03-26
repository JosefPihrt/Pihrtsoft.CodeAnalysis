// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1215ExpressionIsAlwaysEqualToTrueOrFalseTests : AbstractCSharpDiagnosticVerifier<BinaryOperatorAnalyzer, ExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.ExpressionIsAlwaysEqualToTrueOrFalse;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ExpressionIsAlwaysEqualToTrueOrFalse)]
        public async Task Test_True()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{
    void M()
    {
        byte by = 0;
        ushort us = 0;
        uint ui = 0;
        ulong ul = 0;
        var lst = new List<string>();
        var ar = System.Array.Empty<string>();
        var ia = ImmutableArray.Create<string>();
        string ss = null;

        if ([|by >= 0|]) { }
        if ([|us >= 0|]) { }
        if ([|ui >= 0|]) { }
        if ([|ul >= 0|]) { }
        if ([|lst.Count >= 0|]) { }
        if ([|ar.Length >= 0|]) { }
        if ([|ia.Length >= 0|]) { }
        if ([|ss.Length >= 0|]) { }

        if ([|0 <= by|]) { }
        if ([|0 <= us|]) { }
        if ([|0 <= ui|]) { }
        if ([|0 <= ul|]) { }
        if ([|0 <= lst.Count|]) { }
        if ([|0 <= ar.Length|]) { }
        if ([|0 <= ia.Length|]) { }
        if ([|0 <= ss.Length|]) { }
    }
}
", @"
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{
    void M()
    {
        byte by = 0;
        ushort us = 0;
        uint ui = 0;
        ulong ul = 0;
        var lst = new List<string>();
        var ar = System.Array.Empty<string>();
        var ia = ImmutableArray.Create<string>();
        string ss = null;

        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }

        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ExpressionIsAlwaysEqualToTrueOrFalse)]
        public async Task Test_False()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{
    void M()
    {
        byte by = 0;
        ushort us = 0;
        uint ui = 0;
        ulong ul = 0;
        var lst = new List<string>();
        var ar = System.Array.Empty<string>();
        var ia = ImmutableArray.Create<string>();
        string ss = null;

        if ([|by < 0|]) { }
        if ([|us < 0|]) { }
        if ([|ui < 0|]) { }
        if ([|ul < 0|]) { }
        if ([|lst.Count < 0|]) { }
        if ([|ar.Length < 0|]) { }
        if ([|ia.Length < 0|]) { }
        if ([|ss.Length < 0|]) { }

        if ([|0 > by|]) { }
        if ([|0 > us|]) { }
        if ([|0 > ui|]) { }
        if ([|0 > ul|]) { }
        if ([|0 > lst.Count|]) { }
        if ([|0 > ar.Length|]) { }
        if ([|0 > ia.Length|]) { }
        if ([|0 > ss.Length|]) { }
    }
}
", @"
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{
    void M()
    {
        byte by = 0;
        ushort us = 0;
        uint ui = 0;
        ulong ul = 0;
        var lst = new List<string>();
        var ar = System.Array.Empty<string>();
        var ia = ImmutableArray.Create<string>();
        string ss = null;

        if (false) { }
        if (false) { }
        if (false) { }
        if (false) { }
        if (false) { }
        if (false) { }
        if (false) { }
        if (false) { }

        if (false) { }
        if (false) { }
        if (false) { }
        if (false) { }
        if (false) { }
        if (false) { }
        if (false) { }
        if (false) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ExpressionIsAlwaysEqualToTrueOrFalse)]
        public async Task Test_NullCheck()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = null;

        if (s == null || [|s != null|] && s.Contains(""a""))
        {
        }
    }
}
", @"
class C
{
    void M()
    {
        string s = null;

        if (s == null || s.Contains(""a""))
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ExpressionIsAlwaysEqualToTrueOrFalse)]
        public async Task Test_EqualsToDoubleNaN()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        double x = default;
        if ([|x == double.NaN|]) { }
    }
}
", @"
class C
{
    void M()
    {
        double x = default;
        if (double.IsNaN(x)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ExpressionIsAlwaysEqualToTrueOrFalse)]
        public async Task Test_EqualsToDoubleNaN_Right()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        double x = default;
        if ([|double.NaN == x|]) { }
    }
}
", @"
class C
{
    void M()
    {
        double x = default;
        if (double.IsNaN(x)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ExpressionIsAlwaysEqualToTrueOrFalse)]
        public async Task Test_NotEqualsToDoubleNaN()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        double x = default;
        if ([|x != double.NaN|]) { }
    }
}
", @"
class C
{
    void M()
    {
        double x = default;
        if (!double.IsNaN(x)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ExpressionIsAlwaysEqualToTrueOrFalse)]
        public async Task TestNoDiagnostic_ReversedForStatement()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var list = new List<string>();

        for (int i = list.Count - 1; i >= 0; i--)
        {
        }
    }
}
");
        }
    }
}
