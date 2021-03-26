// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1141AddParamElementToDocumentationCommentTests : AbstractCSharpDiagnosticVerifier<SingleLineDocumentationCommentTriviaAnalyzer, SingleLineDocumentationCommentTriviaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddParamElementToDocumentationComment;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParamElementToDocumentationComment)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    ///[| <summary>
    /// 
    /// </summary>
    /// <param name=""p""></param>
    /// <param name=""p3""></param>
    /// <param name=""p5""></param>
|]    void M(object p, object p2, object p3, object p4, object p5)
    {
    }
}
", @"
class C
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name=""p""></param>
    /// <param name=""p2""></param>
    /// <param name=""p3""></param>
    /// <param name=""p4""></param>
    /// <param name=""p5""></param>
    void M(object p, object p2, object p3, object p4, object p5)
    {
    }
}
");
        }
    }
}
