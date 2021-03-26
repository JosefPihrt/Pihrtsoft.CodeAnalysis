// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CSTests : AbstractCSharpCompilerDiagnosticFixVerifier<AddBodyCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.OperatorCannotBeAppliedToOperands;

        //[Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OperatorCannotBeAppliedToOperands)]
        public async Task Test()
        {
            await VerifyFixAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class C
{
    void M()
    {
    }
}
", @"
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        //[Theory, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OperatorCannotBeAppliedToOperands)]
        //[InlineData("", "")]
        public async Task Test(string source, string expected)
        {
            await VerifyFixAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class C
{
    void M()
    {
    }
}
", source, expected, equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        //[Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OperatorCannotBeAppliedToOperands)]
        public async Task TestNoFix()
        {
            await VerifyNoFixAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class C
{
    void M()
    {
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
