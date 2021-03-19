// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CodeAnalysis.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS9011SpecifyExportCodeFixProviderAttributeNameTests : AbstractCSharpDiagnosticVerifier<NamedTypeSymbolAnalyzer, AttributeCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.SpecifyExportCodeFixProviderAttributeName;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SpecifyExportCodeFixProviderAttributeName)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

[[|ExportCodeFixProvider|](LanguageNames.CSharp)]
abstract class C : CodeFixProvider
{
}
", @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(C))]
abstract class C : CodeFixProvider
{
}
");
        }
    }
}
