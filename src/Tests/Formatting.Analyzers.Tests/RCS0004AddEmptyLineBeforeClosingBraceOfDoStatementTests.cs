// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0004AddEmptyLineBeforeClosingBraceOfDoStatementTests : AbstractCSharpDiagnosticVerifier<AddEmptyLineBeforeClosingBraceOfDoStatementAnalyzer, StatementCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddEmptyLineBeforeClosingBraceOfDoStatement;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeClosingBraceOfDoStatement)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;

        do
        {
            M();[||]
        } while (f);
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;

        do
        {
            M();

        } while (f);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeClosingBraceOfDoStatement)]
        public async Task TestNoDiagnostic_EmptyBlock()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool f = false;

        do
        {
        } while (f);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeClosingBraceOfDoStatement)]
        public async Task TestNoDiagnostic_WhileOnNextLine()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool f = false;

        do
        {
            M();
        }
        while (f);
    }
}
");
        }
    }
}
