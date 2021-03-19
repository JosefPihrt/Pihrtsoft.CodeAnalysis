// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0055FixFormattingOfBinaryExpressionChainTests : AbstractCSharpDiagnosticVerifier<FixFormattingOfBinaryExpressionChainAnalyzer, FixFormattingOfBinaryExpressionChainCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.FixFormattingOfBinaryExpressionChain;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfBinaryExpressionChain)]
        public async Task Test_NotWrapped()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M() 
    {
        bool x = false, y = false, z = false;

        x = [|x && y
            && z|];
    }
}
", @"
class C
{
    void M() 
    {
        bool x = false, y = false, z = false;

        x = x
            && y
            && z;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfBinaryExpressionChain)]
        public async Task Test_NotWrapped2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M() 
    {
        bool x = false, y = false, z = false;

        x = [|x &&
            y && z|];
    }
}
", @"
class C
{
    void M() 
    {
        bool x = false, y = false, z = false;

        x = x &&
            y
            && z;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfBinaryExpressionChain)]
        public async Task Test_NoIndentation()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M() 
    {
        bool x = false, y = false, z = false;

        x = [|x
&& y
&& z|];
    }
}
", @"
class C
{
    void M() 
    {
        bool x = false, y = false, z = false;

        x = x
            && y
            && z;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfBinaryExpressionChain)]
        public async Task Test_NoIndentation2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M(string x) 
    {
        return M(
[|"""" +
"""" +
""""|]);
    }
}
", @"
class C
{
    string M(string x) 
    {
        return M(
            """" +
            """" +
            """");
    }
}
", options: Options.EnableDiagnostic(DiagnosticDescriptors.AddNewLineBeforeBinaryOperatorInsteadOfAfterItOrViceVersa, AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterBinaryOperatorInsteadOfBeforeIt));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfBinaryExpressionChain)]
        public async Task Test_WrongIndentation()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M() 
    {
        bool x = false, y = false, z = false;

        x = [|x
        && y
        && z|];
    }
}
", @"
class C
{
    void M() 
    {
        bool x = false, y = false, z = false;

        x = x
            && y
            && z;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfBinaryExpressionChain)]
        public async Task Test_StartsOnSeparateLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M() 
    {
        bool x = false, y = false, z = false;

        x = 
            [|x && y
        && z|];
    }
}
", @"
class C
{
    void M() 
    {
        bool x = false, y = false, z = false;

        x = 
            x
                && y
                && z;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfBinaryExpressionChain)]
        public async Task Test_LeftIsMultiline_RightIsSingleLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M() 
    {
        bool x = false, y = false, z = false;

        x = [|y
            .ToString()
            .Equals("""") && z|];
    }
}
", @"
class C
{
    void M() 
    {
        bool x = false, y = false, z = false;

        x = y
            .ToString()
            .Equals("""")
            && z;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfBinaryExpressionChain)]
        public async Task Test_NestedBinaryExpressionOfDifferentKind()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool x = false, y = false, z = false;

        x = ([|y
            .Equals([|x
            && y|])
                || y
                    .Equals(""b"")|])
            && z;
    }
}
", @"
class C
{
    void M()
    {
        bool x = false, y = false, z = false;

        x = (y
            .Equals(x
                && y)
            || y
                .Equals(""b""))
            && z;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfBinaryExpressionChain)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M() 
    {
        bool x = false, y = false, z = false;

        x = (x || y)
            && z;
    }
}
        ");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfBinaryExpressionChain)]
        public async Task TestNoDiagnostic_StartsOnSeparateLine()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M() 
    {
        bool x = false, y = false, z = false;

        x = 
            x
                && y
                && z;
    }
}
        ");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.FixFormattingOfBinaryExpressionChain)]
        public async Task TestNoDiagnostic_IndentationSizeCannotBeDetermined()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

[Obsolete(""a""
+ ""b""
+ ""c"")]
class C
{
}
        ");
        }
    }
}
