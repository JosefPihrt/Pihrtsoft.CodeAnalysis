// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1081SplitVariableDeclarationTests : AbstractCSharpDiagnosticVerifier<SplitVariableDeclarationAnalyzer, VariableDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.SplitVariableDeclaration;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SplitVariableDeclaration)]
        public async Task Test_SwitchSection()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        switch ("""")
        {
            case """":
                [|object x1, x2|];
                break;
        }
    }
}
", @"
class C
{
    void M()
    {
        switch ("""")
        {
            case """":
                object x1;
                object x2;
                break;
        }
    }
}
");
        }
    }
}
