// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1128UseCoalesceExpressionTests2 : AbstractCSharpDiagnosticVerifier<InvocationExpressionAnalyzer, UseCoalesceExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseCoalesceExpression;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task Test_GetValueOrDefault()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        int? x = 0;
        int y = 1;
        int z = x.[|GetValueOrDefault|](y);
    }
}
", @"
class C
{
    void M()
    {
        int? x = 0;
        int y = 1;
        int z = x ?? y;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task Test_GetValueOrDefault_ConditionalAccess()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        var x = new C();

        int? i = x.P1?.P2.[|GetValueOrDefault|](0);
    }

    public C P1 { get; }

    public int? P2 { get; }
}
", @"
class C
{
    void M()
    {
        var x = new C();

        int? i = (x.P1?.P2) ?? 0;
    }

    public C P1 { get; }

    public int? P2 { get; }
}
");
        }
    }
}
