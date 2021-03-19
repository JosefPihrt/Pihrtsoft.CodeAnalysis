// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1156UseStringLengthInsteadOfComparisonWithEmptyStringTests : AbstractCSharpDiagnosticVerifier<UseStringLengthInsteadOfComparisonWithEmptyStringAnalyzer, BinaryExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseStringLengthInsteadOfComparisonWithEmptyString;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseStringLengthInsteadOfComparisonWithEmptyString)]
        public async Task Test_ComparisonToEmptyString()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = default;
        if ([|s == """"|]) { }
    }
}
", @"
class C
{
    void M()
    {
        string s = default;
        if (s?.Length == 0) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseStringLengthInsteadOfComparisonWithEmptyString)]
        public async Task Test_ComparisonToEmptyString2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = default;
        if ([|("""") == (s)|]) { }
    }
}
", @"
class C
{
    void M()
    {
        string s = default;
        if (0 == (s)?.Length) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseStringLengthInsteadOfComparisonWithEmptyString)]
        public async Task Test_ComparisonToStringEmpty()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = default;
        if ([|s == string.Empty|]) { }
    }
}
", @"
class C
{
    void M()
    {
        string s = default;
        if (s?.Length == 0) { }
    }
}
");
        }
    }
}
