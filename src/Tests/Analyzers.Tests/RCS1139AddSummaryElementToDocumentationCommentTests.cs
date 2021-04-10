// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1139AddSummaryElementToDocumentationCommentTests : AbstractCSharpDiagnosticVerifier<SingleLineDocumentationCommentTriviaAnalyzer, DocumentationCommentCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AddSummaryElementToDocumentationComment;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddSummaryElementToDocumentationComment)]
        public async Task Test_Class()
        {
            await VerifyDiagnosticAndFixAsync(@"
///[| <typeparam name=""T""></typeparam>
|]class C<T>
{
}
", @"
/// <summary>
/// 
/// </summary>
/// <typeparam name=""T""></typeparam>
class C<T>
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddSummaryElementToDocumentationComment)]
        public async Task TestNoDiagnostic_PartialClassWithContentElement()
        {
            await VerifyNoDiagnosticAsync(@"
/// <summary>
/// x
/// </summary>
partial class C
{
}

/// <content>
/// x
/// </content>
partial class C
{
}
");
        }
    }
}
