// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.Analysis.UsePatternMatching;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1220UsePatternMatchingInsteadOfIsAndCastTests : AbstractCSharpDiagnosticVerifier<UsePatternMatchingInsteadOfIsAndCastAnalyzer, UsePatternMatchingInsteadOfIsAndCastCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.UsePatternMatchingInsteadOfIsAndCast;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfIsAndCast)]
        public async Task Test_LogicalAndExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private readonly object _f = false;

    public void M()
    {
        string s = null;

        object x = null;

        if ([|x is string && ((string)x) == s|]) { }

        if ([|x is string && ((string)x).Equals((string)x)|]) { }

        if ([|_f is string && (string)(_f) == s|]) { }

        if ([|this._f is string && (string)this._f == s|]) { }

        if ([|_f is string && (string)(this._f) == s|]) { }

        if ([|this._f is string && (string)_f == s|]) { }

        if ([|this._f is string && ((string)_f).Equals((string)this._f)|]) { }
    }
}
", @"
class C
{
    private readonly object _f = false;

    public void M()
    {
        string s = null;

        object x = null;

        if (x is string x2 && (x2) == s) { }

        if (x is string x3 && (x3).Equals(x3)) { }

        if (_f is string x4 && x4 == s) { }

        if (this._f is string x5 && x5 == s) { }

        if (_f is string x6 && x6 == s) { }

        if (this._f is string x7 && x7 == s) { }

        if (this._f is string x8 && (x8).Equals(x8)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfIsAndCast)]
        public async Task Test_IfStatement()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private readonly object _f = false;

    public void M()
    {
        string s = null;

        object x = null;

        if ([|x is string|])
        {
            if (((string)x) == s) { }
        }

        if ([|x is string|])
        {
            if (((string)x).Equals((string)x)) { }
        }

        if ([|_f is string|])
        {
            if ((string)_f == s) { }
        }

        if ([|this._f is string|])
        {
            if ((string)this._f == s) { }
        }

        if ([|_f is string|])
        {
            if ((string)this._f == s) { }
        }

        if ([|this._f is string|])
        {
            if ((string)_f == s) { }
        }

        if ([|this._f is string|])
        {
            if (((string)_f).Equals((string)this._f)) { }
        }
    }
}
", @"
class C
{
    private readonly object _f = false;

    public void M()
    {
        string s = null;

        object x = null;

        if (x is string x2)
        {
            if ((x2) == s) { }
        }

        if (x is string x3)
        {
            if ((x3).Equals(x3)) { }
        }

        if (_f is string x4)
        {
            if (x4 == s) { }
        }

        if (this._f is string x5)
        {
            if (x5 == s) { }
        }

        if (_f is string x6)
        {
            if (x6 == s) { }
        }

        if (this._f is string x7)
        {
            if (x7 == s) { }
        }

        if (this._f is string x8)
        {
            if ((x8).Equals(x8)) { }
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfIsAndCast)]
        public async Task TestNoDiagnostic_LogicalAndExpression()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    private readonly object _f = false;

    public void M()
    {
        string s = null;
        object x = null;
        object x2 = null;

        if (x is string && ReferenceEquals(((string)x), x)) { }

        if (x is string && ReferenceEquals(((string)x2), s)) { }

        if (x is string && ReferenceEquals(x, s)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfIsAndCast)]
        public async Task TestNoDiagnostic_IfStatement()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    private readonly object _f = false;

    public void M()
    {
        string s = null;
        object x = null;
        object x2 = null;

        if (x is string)
        {
            if (ReferenceEquals(((string)x), x)) { }
        }

        if (x is string)
        {
            if (((string)x2) == s) { }
        }

        if (x is string)
        {
            if (ReferenceEquals(x, s)) { }
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfIsAndCast)]
        public async Task TestNoDiagnostic_LogicalAnd_ExpressionTree()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Linq.Expressions;

class C
{
    public void M<T>(Expression<Func<T>> expression)
    {
        object x = null;
        string s = null;

        M(() => x is string && ((string)x) == s);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfIsAndCast)]
        public async Task TestNoDiagnostic_NullableType()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(int? p)
    {
        object x = null;

        if (x is int?)
        {
            M((int?)x);
        }

        if (x is int? && ((int?)x) == 0) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfIsAndCast)]
        public async Task TestNoDiagnostic_LanguageVersion()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    private readonly object _f = false;

    public void M()
    {
        string s = null;

        object x = null;

        if (x is string && ((string)x) == s) { }
    }
}
", options: WellKnownCSharpTestOptions.Default_CSharp6);
        }
    }
}
