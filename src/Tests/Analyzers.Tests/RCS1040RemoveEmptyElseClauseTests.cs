// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1040RemoveEmptyElseClauseTests : AbstractCSharpDiagnosticVerifier<RemoveEmptyElseClauseAnalyzer, ElseClauseCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.RemoveEmptyElseClause;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyElseClause)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
        {
        }
        [|else
        {
        }|]
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;

        if (f)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyElseClause)]
        public async Task TestNoDiagnostic_ElseIf()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
        {
        }
        else if (f)
        {
        }
}
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyElseClause)]
        public async Task TestNoDiagnostic_NonEmptyElse()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
        {
        }
        else
        {
            M();
        }
}
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyElseClause)]
        public async Task TestNoDiagnostic_IfElseEmbededInIfWithElse()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
            if (f) M(); else { }
        else
        {
            M();
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyElseClause)]
        public async Task TestNoDiagnostic_IfElseEmbededInIfWithElse2()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
            if (f) M(); else if (f) M(); else { }
        else
        {
            M();
        }
    }
}
");
        }
    }
}
