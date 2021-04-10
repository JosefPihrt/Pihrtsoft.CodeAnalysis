// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1066RemoveEmptyFinallyClauseTests : AbstractCSharpDiagnosticVerifier<RemoveEmptyFinallyClauseAnalyzer, FinallyClauseCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.RemoveEmptyFinallyClause;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyFinallyClause)]
        public async Task Test_TryCatchFinally()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        try
        {
        }
        catch
        {
        }
        [|finally
        {
        }|]
    }
}
", @"
class C
{
    void M()
    {
        try
        {
        }
        catch
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyFinallyClause)]
        public async Task Test_TryFinally()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        try
        {
            //x
            M();
            M2();
        }
        [|finally
        {
        }|]
    }

    string M2() => null;
}
", @"
class C
{
    void M()
    {
        //x
        M();
        M2();
    }

    string M2() => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyFinallyClause)]
        public async Task TestNoDiagnostic_NonEmptyFinally()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        try
        {
        }
        catch
        {
        }
        finally
        {
            string foo = null;
        }
    }
}
");
        }
    }
}