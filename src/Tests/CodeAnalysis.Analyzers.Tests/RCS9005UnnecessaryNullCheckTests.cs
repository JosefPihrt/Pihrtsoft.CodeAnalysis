// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CodeAnalysis.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS9005UnnecessaryNullCheckTests : AbstractCSharpDiagnosticVerifier<InvocationExpressionAnalyzer, BinaryExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.UnnecessaryNullCheck;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

class C
{
    void M()
    {
        SyntaxNode node = null;

        if ([|node != null &&|] node.IsKind(SyntaxKind.None)) { }
    }
}
", @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

class C
{
    void M()
    {
        SyntaxNode node = null;

        if (node.IsKind(SyntaxKind.None)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task TestNoDiagnostic_ExpressionsAreNotEquivalent()
        {
            await VerifyNoDiagnosticAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

class C
{
    void M()
    {
        SyntaxNode node = null;
        SyntaxNode node2 = null;

        if (node != null && node2.IsKind(SyntaxKind.None)) { }
    }
}
");
        }
    }
}
