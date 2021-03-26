// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0054FixFormattingOfCallChainTests : AbstractCSharpDiagnosticVerifier<FixFormattingOfCallChainAnalyzer, FixFormattingOfCallChainCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.FixFormattingOfCallChain;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfCallChain)]
        public async Task Test_WrongIndentation()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    C M() 
    {
        var x = new C();

        return [|x.M().M()
        .M()|];
    }
}
", @"
class C
{
    C M() 
    {
        var x = new C();

        return x.M().M()
            .M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfCallChain)]
        public async Task Test_Invocation_NoIndentation()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    C M() 
    {
        var x = new C();

        return [|x.M()
.M()|];
    }
}
", @"
class C
{
    C M() 
    {
        var x = new C();

        return x.M()
            .M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfCallChain)]
        public async Task Test_Invocation_NoIndentation_EmptyLineBetween()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    C M() 
    {
        var x = new C();

        return [|x.M()
.M()|];
    }
}
", @"
class C
{
    C M() 
    {
        var x = new C();

        return x.M()
            .M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfCallChain)]
        public async Task Test_Invocation_WrapAndIndent()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    C M() 
    {
        var x = new C();

        return [|x.M()
            .M().M()|];
    }
}
", @"
class C
{
    C M() 
    {
        var x = new C();

        return x.M()
            .M()
            .M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfCallChain)]
        public async Task Test_Invocation_EmptyLineBetween()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    C M() 
    {
        var x = new C();

        return [|x.M()
    
.M()|];
    }
}
", @"
class C
{
    C M() 
    {
        var x = new C();

        return x.M()
            .M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfCallChain)]
        public async Task Test_Invocation_CommentBetween()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    C M() 
    {
        var x = new C();

        return [|x.M()
        // x
        .M()|];
    }
}
", @"
class C
{
    C M() 
    {
        var x = new C();

        return x.M()
        // x
            .M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfCallChain)]
        public async Task Test_Invocation_IndendationsDiffer()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    C M(string s) 
    {
        var x = new C();

        return [|x.M("""")
        .M(new string(
            ' ',
            1))
    .M(new string(
        ' ',
        1))
            .M(new string(
                ' ',
                1))|];
    }
}
", @"
class C
{
    C M(string s) 
    {
        var x = new C();

        return x.M("""")
            .M(new string(
                ' ',
                1))
            .M(new string(
                ' ',
                1))
            .M(new string(
                ' ',
                1));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfCallChain)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    C M() 
    {
        var x = new C();

        return x.M().M();
    }
}
");
        }
    }
}
