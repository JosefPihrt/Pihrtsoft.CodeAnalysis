// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0046UseSpacesInsteadOfTabTests : AbstractCSharpDiagnosticVerifier<UseSpacesInsteadOfTabAnalyzer, ReplaceTabWithSpacesCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.UseSpacesInsteadOfTab;

        private readonly string _fourSpacesEquivalenceKey;

        public RCS0046UseSpacesInsteadOfTabTests()
        {
            _fourSpacesEquivalenceKey = new ReplaceTabWithSpacesCodeFixProvider().FourSpacesEquivalenceKey;
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseSpacesInsteadOfTab)]
        public async Task Test_FourSpaces()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
[|	|]void M()
[|	|]{
[|		|]M();
[|	|]}
}
", @"
class C
{
    void M()
    {
        M();
    }
}
", equivalenceKey: _fourSpacesEquivalenceKey);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseSpacesInsteadOfTab)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        M();
    }
}
");
        }
    }
}
