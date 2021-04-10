// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1222MergePreprocessorDirectivesTests : AbstractCSharpDiagnosticVerifier<MergePreprocessorDirectivesAnalyzer, DirectiveTriviaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.MergePreprocessorDirectives;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MergePreprocessorDirectives)]
        public async Task Test_PragmaWarningDisable()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
[|#pragma warning disable RCS0|]
#pragma warning disable RCS1, RCS2,
    
#pragma warning disable RCS3, RCS4, RCS5 // comment
#pragma warning restore RCS0
    }
}
", @"
class C
{
    void M()
    {
#pragma warning disable RCS0, RCS1, RCS2, RCS3, RCS4, RCS5 // comment
#pragma warning restore RCS0
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MergePreprocessorDirectives)]
        public async Task Test_PragmaWarningRestore()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
[|#pragma warning restore RCS0|]
#pragma warning restore RCS1, RCS2,
    
#pragma warning restore RCS3, RCS4, RCS5 // comment
#pragma warning disable RCS0
    }
}
", @"
class C
{
    void M()
    {
#pragma warning restore RCS0, RCS1, RCS2, RCS3, RCS4, RCS5 // comment
#pragma warning disable RCS0
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MergePreprocessorDirectives)]
        public async Task TestNoDiagnostic_SingleDirective()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
#pragma warning disable RCS0
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MergePreprocessorDirectives)]
        public async Task TestNoDiagnostic_DisableAndRestore()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
#pragma warning disable RCS0
#pragma warning restore RCS0
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MergePreprocessorDirectives)]
        public async Task TestNoDiagnostic_TrailingComment()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
#pragma warning disable RCS1 // comment
#pragma warning disable RCS2
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MergePreprocessorDirectives)]
        public async Task TestNoDiagnostic_TrailingComma_TrailingComment()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
#pragma warning disable RCS1, // comment
#pragma warning disable RCS2
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MergePreprocessorDirectives)]
        public async Task TestNoDiagnostic_PreviousDirectiveIsSuppressingThisAnalyzer()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
#pragma warning disable RCS1222
#pragma warning disable RCS1
#pragma warning disable RCS2
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MergePreprocessorDirectives)]
        public async Task TestNoDiagnostic_NextDirectiveIsSuppressingThisAnalyzer()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
#pragma warning disable RCS1
#pragma warning disable RCS1222
    }
}
");
        }
    }
}
