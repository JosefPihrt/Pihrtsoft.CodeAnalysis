// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1049SimplifyBooleanComparisonTests : AbstractCSharpDiagnosticVerifier<BooleanLiteralAnalyzer, BinaryExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.SimplifyBooleanComparison;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyBooleanComparison)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(bool x)
    {
        if (
#if DEBUG
            x &&
#endif
            [|x == false|])
        {
        }
    }
}
", @"
class C
{
    void M(bool x)
    {
        if (
#if DEBUG
            x &&
#endif
            !x)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyBooleanComparison)]
        public async Task Test2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(bool x)
    {
        if (
#if RELEASE
            x &&
#endif
            [|x == false|])
        {
        }
    }
}
", @"
class C
{
    void M(bool x)
    {
        if (
#if RELEASE
            x &&
#endif
            !x)
        {
        }
    }
}
");
        }
    }
}
