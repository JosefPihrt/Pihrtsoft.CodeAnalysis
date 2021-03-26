// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CodeAnalysis.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS9003UnnecessaryConditionalAccessTests : AbstractCSharpDiagnosticVerifier<UnnecessaryConditionalAccessAnalyzer, ConditionalAccessExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UnnecessaryConditionalAccess;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryConditionalAccess)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode n = null;

        if(n[|?|].IsKind(SyntaxKind.None) == true) { }
    }
}
", @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode n = null;

        if(n.IsKind(SyntaxKind.None)) { }
    }
}
");
        }
    }
}
