// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0036RemoveEmptyLineBetweenSingleLineDeclarationsOfSameKindTests : AbstractCSharpDiagnosticVerifier<EmptyLineBetweenDeclarationsAnalyzer, EmptyLineBetweenDeclarationsCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.RemoveEmptyLineBetweenSingleLineDeclarationsOfSameKind;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyLineBetweenSingleLineDeclarationsOfSameKind)]
        public async Task Test_Properties()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string P1 { get; set; }
[||]
    string P2 { get; set; }
}
", @"
class C
{
    string P1 { get; set; }
    string P2 { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyLineBetweenSingleLineDeclarationsOfSameKind)]
        public async Task Test_Events()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    public event EventHandler E1;
[||]    
    
    public event EventHandler E2;
}
", @"
using System;

class C
{
    public event EventHandler E1;
    public event EventHandler E2;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyLineBetweenSingleLineDeclarationsOfSameKind)]
        public async Task TestNoDiagnostic_MultilineEvent()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    public event EventHandler E1;

    public event EventHandler E2
    {
        add { }
        remove { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyLineBetweenSingleLineDeclarationsOfSameKind)]
        public async Task TestNoDiagnostic_PropertyAndIndexer()
        {
            await VerifyNoDiagnosticAsync(@"
abstract class C
{
    public abstract string P { get; set; }

    public abstract string this[int index] { get; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveEmptyLineBetweenSingleLineDeclarationsOfSameKind)]
        public async Task TestNoDiagnostic_ConstAndField()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    const string K = null;

    string F = null;
}
");
        }
    }
}
