// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1068SimplifyLogicalNegationTests : AbstractCSharpDiagnosticVerifier<SimplifyLogicalNegationAnalyzer, SimplifyLogicalNegationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.SimplifyLogicalNegation;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_NotTrue()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool x = [|!true|];
    }
}
", @"
class C
{
    void M()
    {
        bool x = false;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_NotTrue2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool x = [|!(true)|];
    }
}
", @"
class C
{
    void M()
    {
        bool x = false;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_NotFalse()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool x = [|!false|];
    }
}
", @"
class C
{
    void M()
    {
        bool x = true;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_NotNot()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool x = false, y = false;

        x = [|!!(y)|];
    }
}
", @"
class C
{
    void M()
    {
        bool x = false, y = false;

        x = y;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_NotNot2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool x = false, y = false;

        x = [|!(!(y))|];
    }
}
", @"
class C
{
    void M()
    {
        bool x = false, y = false;

        x = y;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_EqualsExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool x = false, y = false;

        x = [|!(x == y)|];
    }
}
", @"
class C
{
    void M()
    {
        bool x = false, y = false;

        x = x != y;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_NotEqualsExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool x = false, y = false;

        x = [|!(x != y)|];
    }
}
", @"
class C
{
    void M()
    {
        bool x = false, y = false;

        x = x == y;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_LessThanExpression_Int32()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        int x = 1, y = 2;

        if ([|!(x < y)|]) { }
    }
}
", @"
class C
{
    void M()
    {
        int x = 1, y = 2;

        if (x >= y) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_LessThanExpression_Single()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        const float x = 1;

        if ([|!(x < 1f)|]) { }
    }
}
", @"
class C
{
    void M()
    {
        const float x = 1;

        if (x >= 1f) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_LessThanExpression_Double()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        const double x = 1;

        if ([|!(x < 1d)|]) { }
    }
}
", @"
class C
{
    void M()
    {
        const double x = 1;

        if (x >= 1d) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_LessThanOrEqualsExpression_Int32()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        int x = 1, y = 2;

        if ([|!(x <= y)|]) { }
    }
}
", @"
class C
{
    void M()
    {
        int x = 1, y = 2;

        if (x > y) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_LessThanOrEqualsExpression_Single()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        const float x = 1;

        if ([|!(x <= 1f)|]) { }
    }
}
", @"
class C
{
    void M()
    {
        const float x = 1;

        if (x > 1f) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_LessThanOrEqualsExpression_Double()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        const double x = 1;

        if ([|!(x <= 1d)|]) { }
    }
}
", @"
class C
{
    void M()
    {
        const double x = 1;

        if (x > 1d) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_GreaterThanExpression_Int32()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        int x = 1, y = 2;

        if ([|!(x > y)|]) { }
    }
}
", @"
class C
{
    void M()
    {
        int x = 1, y = 2;

        if (x <= y) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_GreaterThanExpression_Single()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        const float x = 1;

        if ([|!(x > 1f)|]) { }
    }
}
", @"
class C
{
    void M()
    {
        const float x = 1;

        if (x <= 1f) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_GreaterThanExpression_Double()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        const double x = 1;

        if ([|!(x > 1d)|]) { }
    }
}
", @"
class C
{
    void M()
    {
        const double x = 1;

        if (x <= 1d) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_GreaterThanOrEqualsExpression_Int32()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        int x = 1, y = 2;

        if ([|!(x >= y)|]) { }
    }
}
", @"
class C
{
    void M()
    {
        int x = 1, y = 2;

        if (x < y) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_GreaterThanOrEqualsExpression_Single()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        const float x = 1;

        if ([|!(x >= 1f)|]) { }
    }
}
", @"
class C
{
    void M()
    {
        const float x = 1;

        if (x < 1f) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_GreaterThanOrEqualsExpression_Double()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        const double x = 1;

        if ([|!(x >= 1d)|]) { }
    }
}
", @"
class C
{
    void M()
    {
        const double x = 1;

        if (x < 1d) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task TestNoDiagnostic_NotEqualsOperator()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public static bool operator ==(C left, C right)
    {
        return false;
    }

    public static bool operator !=(C left, C right)
    {
        return !(left == right);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task TestNoDiagnostic_EqualsOperator()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public static bool operator ==(C left, C right)
    {
        return !(left != right);
    }

    public static bool operator !=(C left, C right)
    {
        return false;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task TestNoDiagnostic_Double_NaN()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        double x = double.NaN, y = 1;

        if (!(x > y)) { }
        if (!(x < y)) { }
        if (!(x >= y)) { }
        if (!(x >= y)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task TestNoDiagnostic_Float_NaN()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        float x = float.NaN, y = 1;

        if (!(x > y)) { }
        if (!(x < y)) { }
        if (!(x >= y)) { }
        if (!(x >= y)) { }
    }
}
");
        }
    }
}
