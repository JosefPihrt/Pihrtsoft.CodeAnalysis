// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1101FormatDocumentationSummaryOnMultipleLinesTests : AbstractCSharpDiagnosticVerifier<FormatSummaryOnMultipleLinesAnalyzer, SingleLineDocumentationCommentTriviaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.FormatDocumentationSummaryOnMultipleLines;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FormatDocumentationSummaryOnMultipleLines)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
/// [|<summary>a<code>b</code>c</summary>|]
class C
{
}
", @"
/// <summary>
/// a<code>b</code>c
/// </summary>
class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FormatDocumentationSummaryOnMultipleLines)]
        public async Task Test_EmptySummary()
        {
            await VerifyDiagnosticAndFixAsync(@"
/// [|<summary></summary>|]
class C
{
}
", @"
/// <summary>
/// 
/// </summary>
class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FormatDocumentationSummaryOnMultipleLines)]
        public async Task Test_Tab()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
	/// [|<summary>x</summary>|]
	void M()
	{
	}
}
", @"
class C
{
	/// <summary>
	/// x
	/// </summary>
	void M()
	{
	}
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FormatDocumentationSummaryOnMultipleLines)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
/// <summary>
/// x
/// </summary>
class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FormatDocumentationSummaryOnMultipleLines)]
        public async Task TestNoDiagnostic_EmptySummary()
        {
            await VerifyNoDiagnosticAsync(@"
/// <summary>
/// </summary>
class C
{
}
");
        }
    }
}
