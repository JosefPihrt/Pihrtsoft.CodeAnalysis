// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1051ParenthesizeConditionOfConditionalExpressionTests : AbstractCSharpDiagnosticVerifier<ParenthesizeConditionOfConditionalExpressionAnalyzer, ExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.ParenthesizeConditionOfConditionalExpression;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ParenthesizeConditionOfConditionalExpression)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = null;
        s = [|s != null|] ? ""true"" : ""false"";
    }
}
", @"
class C
{
    void M()
    {
        string s = null;
        s = (s != null) ? ""true"" : ""false"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ParenthesizeConditionOfConditionalExpression)]
        public async Task Test_SingleTokenExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool b = false;
        string s = [|b|] ? ""true"" : ""false"";
    }
}
", @"
class C
{
    void M()
    {
        bool b = false;
        string s = (b) ? ""true"" : ""false"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ParenthesizeConditionOfConditionalExpression)]
        public async Task Test_RemoveParenthesesFromSingleTokenExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool b = false;
        string s = [|(b)|] ? ""true"" : ""false"";
    }
}
", @"
class C
{
    void M()
    {
        bool b = false;
        string s = b ? ""true"" : ""false"";
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.RemoveParenthesesFromConditionOfConditionalExpressionWhenExpressionIsSingleToken));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ParenthesizeConditionOfConditionalExpression)]
        public async Task TestNoDiagnostic_SingleTokenExpression()
        {
            await VerifyNoDiagnosticAsync(@"
public class C
{
    void M()
    {
        bool b = false;
        string s = b ? ""true"" : ""false"";
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.RemoveParenthesesFromConditionOfConditionalExpressionWhenExpressionIsSingleToken));
        }
    }
}
