// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0042RemoveNewLinesFromAccessorListOfAutoPropertyTests : AbstractCSharpDiagnosticVerifier<AccessorListAnalyzer, AccessorListCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.RemoveNewLinesFromAccessorListOfAutoProperty;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLinesFromAccessorListOfAutoProperty)]
        public async Task Test_ReadOnlyProperty()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string P
    [|{
        get;
    }|]
}
", @"
class C
{
    string P { get; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLinesFromAccessorListOfAutoProperty)]
        public async Task Test_Property()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string P
    [|{
        get;
        set;
    }|]
}
", @"
class C
{
    string P { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLinesFromAccessorListOfAutoProperty)]
        public async Task Test_ReadOnlyIndexer()
        {
            await VerifyDiagnosticAndFixAsync(@"
interface I
{
    string this[int index]
    [|{
        get;
    }|]
}
", @"
interface I
{
    string this[int index] { get; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLinesFromAccessorListOfAutoProperty)]
        public async Task Test_Indexer()
        {
            await VerifyDiagnosticAndFixAsync(@"
interface I
{
    string this[int index]
    [|{
        get;
        set;
    }|]
}
", @"
interface I
{
    string this[int index] { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLinesFromAccessorListOfAutoProperty)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    string P { get; set; }
}

interface I
{
    string this[int index] { get; set; }
}
");
        }
    }
}
