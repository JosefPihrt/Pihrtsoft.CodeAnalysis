// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1073ConvertIfToReturnStatementTests : AbstractCSharpDiagnosticVerifier<IfStatementAnalyzer, IfStatementCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.ConvertIfToReturnStatement;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertIfToReturnStatement)]
        public async Task Test_IfElse()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    bool M()
    {
        bool x = false;
        //x
        [|if (x)
        {
            return true;
        }
        else
        {
            return false;
        }|]
    }
}
", @"
class C
{
    bool M()
    {
        bool x = false;
        //x
        return x;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertIfToReturnStatement)]
        public async Task Test_IfReturn()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    bool M()
    {
        bool x = false;
        //x
        [|if (x)
        {
            return true;
        }|]

        return false;
    }
}
", @"
class C
{
    bool M()
    {
        bool x = false;
        //x
        return x;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertIfToReturnStatement)]
        public async Task TestNoDiagnostic_IfElseContainsComment()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    bool M()
    {
        bool x = false;
        if (x)
        {
            return true;
        }
        else
        {
            //x
            return false;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertIfToReturnStatement)]
        public async Task TestNoDiagnostic_IfElseContainsDirective()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    bool M()
    {
        bool x = false;
        if (x)
        {
            return true;
        }
        else
        {
#if DEBUG
            return false;
#endif
        }
    }
}
", options: Options.WithDebugPreprocessorSymbol());
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertIfToReturnStatement)]
        public async Task TestNoDiagnostic_IfReturnContainsComment()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    bool M()
    {
        bool x = false;
        if (x)
        {
            return true;
        }

        //x
        return false;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertIfToReturnStatement)]
        public async Task TestNoDiagnostic_IfReturnContainsDirective()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    bool M()
    {
        bool x = false;
        if (x)
        {
            return true;
        }

#if DEBUG
        return false;
#endif
    }
}
", options: Options.WithDebugPreprocessorSymbol());
        }
    }
}
