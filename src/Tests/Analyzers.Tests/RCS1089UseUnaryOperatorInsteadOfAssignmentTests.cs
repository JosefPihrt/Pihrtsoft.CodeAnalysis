// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1089UseUnaryOperatorInsteadOfAssignmentTests : AbstractCSharpDiagnosticVerifier<UseUnaryOperatorInsteadOfAssignmentAnalyzer, AssignmentExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.UseUnaryOperatorInsteadOfAssignment;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseUnaryOperatorInsteadOfAssignment)]
        public async Task Test_PostIncrement()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    int i;

    void M()
    {
        [|i += 1|];
    }
}
", @"
class C
{
    int i;

    void M()
    {
        i++;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseUnaryOperatorInsteadOfAssignment)]
        public async Task Test_PostIncrement2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    int i;

    void M()
    {
        [|i = i + 1|];
    }
}
", @"
class C
{
    int i;

    void M()
    {
        i++;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseUnaryOperatorInsteadOfAssignment)]
        public async Task Test_PostDecrement()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    int i;

    void M()
    {
        [|i -= 1|];
    }
}
", @"
class C
{
    int i;

    void M()
    {
        i--;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseUnaryOperatorInsteadOfAssignment)]
        public async Task Test_PostDecrement2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    int i;

    void M()
    {
        [|i = i - 1|];
    }
}
", @"
class C
{
    int i;

    void M()
    {
        i--;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseUnaryOperatorInsteadOfAssignment)]
        public async Task Test_PreIncrement()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    int i;

    void M(int i)
    {
        M([|i += 1|]);
    }
}
", @"
class C
{
    int i;

    void M(int i)
    {
        M(++i);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseUnaryOperatorInsteadOfAssignment)]
        public async Task Test_PreIncrement2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    int i;

    void M(int i)
    {
        M([|i = i + 1|]);
    }
}
", @"
class C
{
    int i;

    void M(int i)
    {
        M(++i);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseUnaryOperatorInsteadOfAssignment)]
        public async Task Test_PreDecrement()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    int i;

    void M(int i)
    {
        M([|i -= 1|]);
    }
}
", @"
class C
{
    int i;

    void M(int i)
    {
        M(--i);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseUnaryOperatorInsteadOfAssignment)]
        public async Task Test_PreDecrement2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    int i;

    void M(int i)
    {
        M([|i = i - 1|]);
    }
}
", @"
class C
{
    int i;

    void M(int i)
    {
        M(--i);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseUnaryOperatorInsteadOfAssignment)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        int i = 0, i2 = 0;

        i = i2 + 1; 
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseUnaryOperatorInsteadOfAssignment)]
        public async Task TestNoDiagnostic_ObjectInitializer()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public int P { get; set; }

    void M()
    {
        var x = new C() { P = P + 1 };

        var y = new { P = P + 1 };
    }
}
");
        }
    }
}
