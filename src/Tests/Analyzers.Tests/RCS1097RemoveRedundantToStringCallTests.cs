// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1097RemoveRedundantToStringCallTests : AbstractCSharpDiagnosticVerifier<InvocationExpressionAnalyzer, InvocationExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.RemoveRedundantToStringCall;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantToStringCall)]
        public async Task Test_StringVariable()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s1 = null;
        string s2 = s1[|.ToString()|];
    }
}
", @"
class C
{
    void M()
    {
        string s1 = null;
        string s2 = s1;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantToStringCall)]
        public async Task Test_StringLiteral()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = """"[|.ToString()|];
    }
}
", @"
class C
{
    void M()
    {
        string s = """";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantToStringCall)]
        public async Task Test_InterpolatedString()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = $""""[|.ToString()|];
    }
}
", @"
class C
{
    void M()
    {
        string s = $"""";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantToStringCall)]
        public async Task Test_Interpolation()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = null;
        s = $""{s[|.ToString()|]}"";
    }
}
", @"
class C
{
    void M()
    {
        string s = null;
        s = $""{s}"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantToStringCall)]
        public async Task TestNoDiagnostic_ValueType()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        int i = 10;
        string s = $""'{i.ToString()}'"";
    }
}
");
        }
    }
}
