// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1143SimplifyCoalesceExpressionTests : AbstractCSharpDiagnosticVerifier<SimplifyCoalesceExpressionAnalyzer, BinaryExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.SimplifyCoalesceExpression;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyCoalesceExpression)]
        public async Task Test_DefaultOfNullableType()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        C x = null;

        int? y = x?.M2() [|?? default(int?)|];
    }

    int M2() => default;
}
", @"
class C
{
    void M()
    {
        C x = null;

        int? y = x?.M2();
    }

    int M2() => default;
}
");
        }
    }
}
