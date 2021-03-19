// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1124InlineLocalVariableTests : AbstractCSharpDiagnosticVerifier<InlineLocalVariableAnalyzer, LocalDeclarationStatementCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.InlineLocalVariable;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.InlineLocalVariable)]
        public async Task Test_LocalDeclaration()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        // a
        [|int x = 1 // b
            + 1;|]
        int y = x;
    }
}
", @"
class C
{
    void M()
    {
        // a
        int y = 1 // b
            + 1;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.InlineLocalVariable)]
        public async Task Test_YieldReturn()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M()
    {
        [|var s = """";|]
        yield return s;
    }
}
", @"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M()
    {
        yield return """";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.InlineLocalVariable)]
        public async Task TestNoDiagnostic_YieldReturnIsNotLastStatement()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M()
    {
        var s = """";
        yield return s;
        s = null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.InlineLocalVariable)]
        public async Task TestNoDiagnostic_ForEachWithAwait()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;
using System.Threading.Tasks;

class C
{
    async Task<IEnumerable<object>> GetAsync()
    {
        var items = await GetAsync();

        foreach (var item in items)
        {
        }

        return null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.InlineLocalVariable)]
        public async Task TestNoDiagnostic_SwitchWithAwait()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    async Task<string> GetAsync()
    {
        var x = await GetAsync();

        switch (x)
        {
            case """":
                break;
        }

        return null;
    }
}
");
        }
    }
}
