// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1037RemoveTrailingWhitespaceTests : AbstractCSharpDiagnosticVerifier<WhitespaceAnalyzer, WhitespaceTriviaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.RemoveTrailingWhitespace;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveTrailingWhitespace)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"[| |]
class C[|  |]
{
[|
   |]
}
", @"
class C
{

}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveTrailingWhitespace)]
        public async Task Test_DocumentationComment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary>[| |]
    /// x[|  |]
    /// </summary>[|   |]
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
    }
}
