// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0162UnreachableCodeDetectedTests : AbstractCSharpCompilerDiagnosticFixVerifier<UnreachableCodeCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.UnreachableCodeDetected;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.UnreachableCodeDetected)]
        public async Task Test()
        {
            await VerifyFixAsync(@"
class C
{
    int M()
    {
        if (1 == 2)
        {
            return 1;
        }
        else if (2 == 3)
        {
            return 2;
        }
        else
        {
            return 0;
        }
    }
}
", @"
class C
{
    int M()
    {
        return 0;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.UnreachableCodeDetected)]
        public async Task Test_LocalFunction()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        LF();

        return;

        LF();

        void LF()
        {
            return;
        }
    }
}
", @"
class C
{
    void M()
    {
        LF();

        return;

        void LF()
        {
            return;
        }
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.UnreachableCodeDetected)]
        public async Task Test_LocalFunction2()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        LF();

        return;

        LF();

        void LF()
        {
            return;
        }

        LF2();

        void LF2()
        {
            return;
        }
    }
}
", @"
class C
{
    void M()
    {
        LF();

        return;

        void LF()
        {
            return;
        }

        void LF2()
        {
            return;
        }
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
