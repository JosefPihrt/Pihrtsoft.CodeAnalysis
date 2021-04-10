// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0033AddNewLineBeforeStatementTests : AbstractCSharpDiagnosticVerifier<AddNewLineBeforeStatementAnalyzer, StatementCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AddNewLineBeforeStatement;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeStatement)]
        public async Task Test_Block()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        M(); [||]M();
    }
}
", @"
class C
{
    void M()
    {
        M();
        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeStatement)]
        public async Task Test_Block_SingleLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M() { M(); [||]M(); }
}
", @"
class C
{
    void M()
    {
        M();
        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeStatement)]
        public async Task Test_SwitchSection()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = null;
        switch (s)
        {
            case """":
                M(); [||]M();
                break;
        }
    }
}
", @"
class C
{
    void M()
    {
        string s = null;
        switch (s)
        {
            case """":
                M();
                M();
                break;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeStatement)]
        public async Task TestNoDiagnostic_EmptyStatement()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        M(); ;
    }
}
");
        }
    }
}
