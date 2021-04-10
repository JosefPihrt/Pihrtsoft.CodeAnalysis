// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1104SimplifyConditionalExpressionTests : AbstractCSharpDiagnosticVerifier<SimplifyConditionalExpressionAnalyzer, ConditionalExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.SimplifyConditionalExpression;

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyConditionalExpression)]
        [InlineData("f ? true : false", "f")]
        [InlineData("!f ? false : true", "f")]
        [InlineData("((f)) ? ((true)) : ((false))", "f")]
        [InlineData("f ? false : true", "!f")]
        [InlineData("f == g ? false : true", "f != g")]
        [InlineData("f != g ? false : true", "f == g")]

        [InlineData(@"f
            ? true
            : false", "f")]

        [InlineData(@"[|f //a
              /*b*/ ? /*c*/ true //d
                                 /*e*/ : /*f*/ false|] /*g*/", @"f //a
              /*b*/  /*c*/  //d
                                 /*e*/  /*f*/  /*g*/")]
        public async Task Test_TrueFalse(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(bool f, bool g)
    {
        if ([||]) { }
}
}
", source, expected);
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyConditionalExpression)]
        [InlineData("f ? g : false", "f && g")]
        [InlineData("f ? g || g : false", "f && (g || g)")]
        [InlineData(@"[|f
            ? g
            : false|] /**/", @"f
            && g /**/")]
        public async Task Test_LogicalAnd(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(bool f, bool g)
    {
        if ([||]) { }
    }
}
", source, expected);
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyConditionalExpression)]
        [InlineData("f ? true : g", "f || g")]
        [InlineData(@"[|f
            ? true
            : g|] /**/", @"f
            || g /**/")]
        public async Task Test_LogicalOr(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(bool f, bool g)
    {
        if ([||]) { }
    }
}
", source, expected);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyConditionalExpression)]
        public async Task Test_NegateCondition()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool x = false, y = false;

        bool z = [|x ? false : y|];
    }
}
", @"
class C
{
    void M()
    {
        bool x = false, y = false;

        bool z = !x && y;
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.SimplifyConditionalExpressionWhenItIncludesNegationOfCondition));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyConditionalExpression)]
        public async Task Test_NegateCondition2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool x = false, y = false;

        bool z = [|x ? y : true|];
    }
}
", @"
class C
{
    void M()
    {
        bool x = false, y = false;

        bool z = !x || y;
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.SimplifyConditionalExpressionWhenItIncludesNegationOfCondition));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyConditionalExpression)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(bool f, bool g, bool h)
    {
        if ((f) ? g : h) { }
        if ((f) ? false : g) { }
        if ((f) ? g : true) { }

        if ((f)
#if DEBUG
                ? false
                : true) { }
#else
                ? true
                : false;
#endif
    }
}
", options: Options.WithDebugPreprocessorSymbol());
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyConditionalExpression)]
        public async Task TestNoDiagnostic_NullableBool()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool x = false;

        bool? y = (x) ? default(bool?) : false;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyConditionalExpression)]
        public async Task TestNoDiagnostic_NegationOfCondition()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool x = false, y = false;

        bool z1 = x ? false : y;

        bool z2 = x ? y : true;
    }
}
");
        }
    }
}
