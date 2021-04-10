// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0022AddNewLineAfterOpeningBraceOfEmptyBlockTests : AbstractCSharpDiagnosticVerifier<AddNewLineAfterOpeningBraceOfEmptyBlockAnalyzer, BlockCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AddNewLineAfterOpeningBraceOfEmptyBlock;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfEmptyBlock)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {[||]}
}
", @"
class C
{
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfEmptyBlock)]
        public async Task Test_WithWhitespace()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {[||] }
}
", @"
class C
{
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfEmptyBlock)]
        public async Task TestNoDiagnostic_EmptyLine()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {

    }
}
");
        }
    }
}
