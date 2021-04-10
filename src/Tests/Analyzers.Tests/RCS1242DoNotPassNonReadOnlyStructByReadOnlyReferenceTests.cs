// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1242DoNotPassNonReadOnlyStructByReadOnlyReferenceTests : AbstractCSharpDiagnosticVerifier<RefReadOnlyParameterAnalyzer, ParameterCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.DoNotPassNonReadOnlyStructByReadOnlyReference;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DoNotPassNonReadOnlyStructByReadOnlyReference)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
struct C
{
    void M(in C [|c|])
    {
    }
}
", @"
struct C
{
    void M(C c)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DoNotPassNonReadOnlyStructByReadOnlyReference)]
        public async Task TestNoDiagnostic_ReadOnlyStruct()
        {
            await VerifyNoDiagnosticAsync(@"
readonly struct C
{
    void M(in C c)
    {
    }
}
");
        }
    }
}
