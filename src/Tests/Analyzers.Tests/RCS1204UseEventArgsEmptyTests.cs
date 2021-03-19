// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1204UseEventArgsEmptyTests : AbstractCSharpDiagnosticVerifier<UseEventArgsEmptyAnalyzer, ObjectCreationExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseEventArgsEmpty;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseEventArgsEmpty)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        var x = [|new EventArgs()|];
    }
}
", @"
using System;

class C
{
    void M()
    {
        var x = EventArgs.Empty;
    }
}
");
        }
    }
}
