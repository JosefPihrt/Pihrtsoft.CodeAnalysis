// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1031RemoveUnnecessaryBracesTests : AbstractCSharpDiagnosticVerifier<RemoveUnnecessaryBracesAnalyzer, BlockCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.RemoveUnnecessaryBraces;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveUnnecessaryBraces)]
        public async Task Test_Section()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = null;

        switch (s)
        {
            case """":
                [|{|]
                    M();
                    break;
                }
            default:
                break;
        }
    }
}
", @"
class C
{
    void M()
    {
        string s = null;

        switch (s)
        {
            case """":
                M();
                break;

            default:
                break;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveUnnecessaryBraces)]
        public async Task Test_SectionWithComments()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = null;

        switch (s)
        {
            case """":
                // a
                [|{|] // b
                    M();
                    break; // c
                    // d
                } // e
            default:
                break;
        }
    }
}
", @"
class C
{
    void M()
    {
        string s = null;

        switch (s)
        {
            case """":
                // a
                // b
                M();
                break; // c
                       // d
                       // e

            default:
                break;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveUnnecessaryBraces)]
        public async Task Test_LastSection()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = null;

        switch (s)
        {
            case """":
                [|{|]
                    M();
                    break;
                }
        }
    }
}
", @"
class C
{
    void M()
    {
        string s = null;

        switch (s)
        {
            case """":
                M();
                break;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveUnnecessaryBraces)]
        public async Task TestNoDiagnostic_SectionWithoutBlock()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        string s = null;

        switch (s)
        {
            case """":
                break;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveUnnecessaryBraces)]
        public async Task TestNoDiagnostic_UsingLocalVariable()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        string s = null;

        switch (s)
        {
            case """":
                {
                    using IDisposable disposable = default;
                    break;
                }
        }
    }
}
");
        }
    }
}
