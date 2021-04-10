// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1041RemoveEmptyInitializerTests : AbstractCSharpDiagnosticVerifier<RemoveEmptyInitializerAnalyzer, RemoveEmptyInitializerCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.RemoveEmptyInitializer;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyInitializer)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        var x = new C [|{ }|];
    }
}
", @"
class C
{
    void M()
    {
        var x = new C();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyInitializer)]
        public async Task TestNoDiagnostic_ExpressionTree()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Linq.Expressions;

class C
{
    public void M<T>(Expression<Func<T>> e)
    {
        M(() => new C { });
    }
}
");
        }
    }
}
