// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0027AddNewLineBeforeBinaryOperatorInsteadOfAfterItOrViceVersaTests : AbstractCSharpDiagnosticVerifier<AddNewLineBeforeBinaryOperatorInsteadOfAfterItOrViceVersaAnalyzer, BinaryExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddNewLineBeforeBinaryOperatorInsteadOfAfterItOrViceVersa;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeBinaryOperatorInsteadOfAfterItOrViceVersa)]
        public async Task Test_BeforeInsteadOfAfter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool x = false;
        bool y = false;
        bool z = false;

        if (x [||]&&
            y [||]&&
            z)
        {
        }
    }
}
", @"
class C
{
    void M()
    {
        bool x = false;
        bool y = false;
        bool z = false;

        if (x
            && y
            && z)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeBinaryOperatorInsteadOfAfterItOrViceVersa)]
        public async Task Test_AfterInsteadOfBefore()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool x = false;
        bool y = false;
        bool z = false;

        if (x
            [||]&& y
            [||]&& z)
        {
        }
    }
}
", @"
class C
{
    void M()
    {
        bool x = false;
        bool y = false;
        bool z = false;

        if (x &&
            y &&
            z)
        {
        }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterBinaryOperatorInsteadOfBeforeIt));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeBinaryOperatorInsteadOfAfterItOrViceVersa)]
        public async Task TestNoDiagnostic_BeforeInsteadOfAfter()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool x = false;
        bool y = false;
        bool z = false;

        if (x
            && y
            && z)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeBinaryOperatorInsteadOfAfterItOrViceVersa)]
        public async Task TestNoDiagnostic_BeforeInsteadOfAfter_SingleLine()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool x = false;
        bool y = false;
        bool z = false;

        if (x && y && z)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeBinaryOperatorInsteadOfAfterItOrViceVersa)]
        public async Task TestNoDiagnostic_AfterInsteadOfBefore()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool x = false;
        bool y = false;
        bool z = false;

        if (x &&
            y &&
            z)
        {
        }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterBinaryOperatorInsteadOfBeforeIt));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeBinaryOperatorInsteadOfAfterItOrViceVersa)]
        public async Task TestNoDiagnostic_AfterInsteadOfBefore_SingleLine()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool x = false;
        bool y = false;
        bool z = false;

        if (x && y && z)
        {
        }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterBinaryOperatorInsteadOfBeforeIt));
        }
    }
}
