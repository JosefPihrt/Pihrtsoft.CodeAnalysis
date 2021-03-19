// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1123AddParenthesesWhenNecessaryTests : AbstractCSharpDiagnosticVerifier<AddParenthesesWhenNecessaryAnalyzer, AddParenthesesWhenNecessaryCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddParenthesesWhenNecessary;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParenthesesWhenNecessary)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool a = false, b = false, c = false, d = false;

        if ([|a
#if DEBUG
            && b
#endif
            && c|] || d)
        {
        }
    }
}
", @"
class C
{
    void M()
    {
        bool a = false, b = false, c = false, d = false;

        if ((a
#if DEBUG
            && b
#endif
            && c) || d)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddParenthesesWhenNecessary)]
        public async Task TestNoDiagnostic_PreprocessorDirectives()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool a = false, b = false, c = false, d = false;

        if (a
#if X
            && c || d)
#else //X
#if X2
#else //X2
#endif //X2
            && b
            && c || d)
#endif //X
        {
        }
    }
}
");
        }
    }
}
