// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0201OnlyAssignmentCallIncrementDecrementAndNewObjectExpressionsCanBeUsedAsStatementTests : AbstractCSharpCompilerDiagnosticFixVerifier<ExpressionCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.OnlyAssignmentCallIncrementDecrementAndNewObjectExpressionsCanBeUsedAsStatement;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OnlyAssignmentCallIncrementDecrementAndNewObjectExpressionsCanBeUsedAsStatement)]
        public async Task Test_RemoveParentheses()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        (M());
    }
}
", @"
class C
{
    void M()
    {
        M();
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OnlyAssignmentCallIncrementDecrementAndNewObjectExpressionsCanBeUsedAsStatement)]
        public async Task Test_IntroduceLocal()
        {
            await VerifyFixAsync(@"
using System;

class C
{
    void M()
    {
        DateTime.Now;
    }
}
", @"
using System;

class C
{
    void M()
    {
        var dateTime = DateTime.Now;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.IntroduceLocalVariable));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OnlyAssignmentCallIncrementDecrementAndNewObjectExpressionsCanBeUsedAsStatement)]
        public async Task Test_IntroduceField()
        {
            await VerifyFixAsync(@"
using System;

class C
{
    void M()
    {
        DateTime.Now;
    }
}
", @"
using System;

class C
{
    private DateTime dateTime;

    void M()
    {
        dateTime = DateTime.Now;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.IntroduceField));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OnlyAssignmentCallIncrementDecrementAndNewObjectExpressionsCanBeUsedAsStatement)]
        public async Task Test_IntroduceStaticField()
        {
            await VerifyFixAsync(@"
using System;

class C
{
    static void M()
    {
        DateTime.Now;
    }
}
", @"
using System;

class C
{
    private static DateTime dateTime;

    static void M()
    {
        dateTime = DateTime.Now;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.IntroduceField));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OnlyAssignmentCallIncrementDecrementAndNewObjectExpressionsCanBeUsedAsStatement)]
        public async Task Test_AddArgumentList()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        C x = null;

        M;
        x.M;
    }
}
", @"
class C
{
    void M()
    {
        C x = null;

        M();
        x.M();
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.AddArgumentList));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OnlyAssignmentCallIncrementDecrementAndNewObjectExpressionsCanBeUsedAsStatement)]
        public async Task Test_ReplaceConditionalExpressionWithIfElse()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        bool f = false;
        (f) ? M() : M2();
    }

    void M2()
    {
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;
        if (f)
        {
            M();
        }
        else
        {
            M2();
        }
    }

    void M2()
    {
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.ReplaceConditionalExpressionWithIfElse));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OnlyAssignmentCallIncrementDecrementAndNewObjectExpressionsCanBeUsedAsStatement)]
        public async Task Test_ReplaceComparisonWithAssignment()
        {
            await VerifyFixAsync(@"
class C
{
    void M(string s)
    {
        s == null;
        s == """";
    }
}
", @"
class C
{
    void M(string s)
    {
        s = null;
        s = """";
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.ReplaceComparisonWithAssignment));
        }
    }
}
