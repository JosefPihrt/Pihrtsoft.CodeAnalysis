// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1243DuplicateWordInCommentTests : AbstractCSharpDiagnosticVerifier<DuplicateWordInCommentAnalyzer, DuplicateWordInCommentCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.DuplicateWordInComment;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DuplicateWordInComment)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary>
    /// aa bb [|bb|] cc
    /// </summary>
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// aa bb cc
    /// </summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DuplicateWordInComment)]
        public async Task Test_MoreWhiteSpace()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary>
    /// aa  bb  [|bb|]  cc
    /// </summary>
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// aa  bb  cc
    /// </summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DuplicateWordInComment)]
        public async Task Test_EndOfText()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary>
    /// aa bb [|bb|]
    /// </summary>
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// aa bb
    /// </summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DuplicateWordInComment)]
        public async Task Test_StartOfText()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary>
    ///bb [|bb|] cc
    /// </summary>
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    ///bb cc
    /// </summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DuplicateWordInComment)]
        public async Task Test_ThreeConsecutiveWords()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary>
    /// aa bb [|bb|] [|bb|] cc
    /// </summary>
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// aa bb cc
    /// </summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DuplicateWordInComment)]
        public async Task Test_TwoConsecutiveDuplicateWords()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary>
    /// aa bb [|bb|] cc [|cc|] dd
    /// </summary>
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// aa bb cc dd
    /// </summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.DuplicateWordInComment)]
        public async Task Test_TwoDuplicateWords()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary>
    /// aa bb [|bb|] cc dd [|dd|]
    /// </summary>
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// aa bb cc dd
    /// </summary>
    void M()
    {
    }
}
");
        }
    }
}
