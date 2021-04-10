// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1239UseForStatementInsteadOfWhileStatementTests : AbstractCSharpDiagnosticVerifier<UseForStatementInsteadOfWhileStatementAnalyzer, WhileStatementCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.UseForStatementInsteadOfWhileStatement;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseForStatementInsteadOfWhileStatement)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;

        int i = 0;
        [|while|] (f)
        {
            M();
            i++;
        }
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;

        for (int i = 0; f; i++)
        {
            M();
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseForStatementInsteadOfWhileStatement)]
        public async Task Test_ContinueStatementInsideNestedLoop()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f1 = false, f2 = false, f3 = false;

        int i = 0;
        [|while|] (f1)
        {
            int j = 0;
            while (f2)
            {
                if (f3)
                    continue;

                j++;
            }
            
            i++;
        }
    }
}
", @"
class C
{
    void M()
    {
        bool f1 = false, f2 = false, f3 = false;

        for (int i = 0; f1; i++)
        {
            int j = 0;
            while (f2)
            {
                if (f3)
                    continue;

                j++;
            }
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseForStatementInsteadOfWhileStatement)]
        public async Task TestNoDiagnostic_LocalVariableReferencedAfterWhileStatement()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool f = false;

        int i = 0;
        while (f)
        {
            M();
            i++;
        }

        i = 0;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseForStatementInsteadOfWhileStatement)]
        public async Task TestNoDiagnostic_ContainsContinueStatement()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool f1 = false, f2 = false;

        int i = 0;
        while (f1)
        {
            M();

            if (f2)
            {
                continue;
            }

            i++;
        }

        i = 0;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseForStatementInsteadOfWhileStatement)]
        public async Task TestNoDiagnostic_MultipleIncrementedVariables()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool f = false;

        int i = 0;
        int j = 0;
        while (f)
        {
            M();
            i++;
            j++;
        }
    }
}
");
        }
    }
}
