// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1145RemoveRedundantAsOperatorTests : AbstractCSharpDiagnosticVerifier<RemoveRedundantAsOperatorAnalyzer, BinaryExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.RemoveRedundantAsOperator;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsOperator)]
        public async Task TestNoDiagnostic_Dynamic()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        dynamic d = null;

        object o = null;

        d = o as dynamic;

        o = d as object;
    }
}
");
        }
    }
}
