// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1036RemoveRedundantEmptyLineTests : AbstractCSharpDiagnosticVerifier<RemoveRedundantEmptyLineAnalyzer, WhitespaceTriviaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.RemoveRedundantEmptyLine;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantEmptyLine)]
        public async Task Test_ObjectInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        var x = new C()
        {
[|
|]            P1 = 1,
            P2 = 2
[|
|]        };
    }

    public int P1 { get; set; }
    public int P2 { get; set; }
}
", @"
class C
{
    void M()
    {
        var x = new C()
        {
            P1 = 1,
            P2 = 2
        };
    }

    public int P1 { get; set; }
    public int P2 { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantEmptyLine)]
        public async Task Test_ObjectInitializer_WithTrailingComma()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        var x = new C()
        {
[|
|]            P1 = 1,
            P2 = 2,
[|
|]        };
    }

    public int P1 { get; set; }
    public int P2 { get; set; }
}
", @"
class C
{
    void M()
    {
        var x = new C()
        {
            P1 = 1,
            P2 = 2,
        };
    }

    public int P1 { get; set; }
    public int P2 { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantEmptyLine)]
        public async Task Test_ArrayInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        var items = new object[]
        {
[|
|]            null,
            null
[|
|]        };
    }
}
", @"
class C
{
    void M()
    {
        var items = new object[]
        {
            null,
            null
        };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantEmptyLine)]
        public async Task Test_CollectionInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<object>()
        {
[|
|]            null,
            null
[|
|]        };
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<object>()
        {
            null,
            null
        };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantEmptyLine)]
        public async Task Test_EmptyDeclaration()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
[|
|]}
", @"
class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantEmptyLine)]
        public async Task Test_EmptyBlock()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
[|
|]    }
}
", @"
class C
{
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantEmptyLine)]
        public async Task Test_EmptyLineAfterDocComment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary></summary>
[|
|]    void M()
    {
    }
}
", @"
class C
{
    /// <summary></summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantEmptyLine)]
        public async Task Test_EmptyLineBetweenClosingBraceAndSwitchSection()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string x = null;

        switch (x)
        {
            case ""a"":
                {
                    M();
                    break;
                }
[|
|]            case ""b"":
                {
                    M();
                    break;
                }
[|
|]            case ""c"":
                break;
        }
    }
}
", @"
class C
{
    void M()
    {
        string x = null;

        switch (x)
        {
            case ""a"":
                {
                    M();
                    break;
                }
            case ""b"":
                {
                    M();
                    break;
                }
            case ""c"":
                break;
        }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.RemoveEmptyLineBetweenClosingBraceAndSwitchSection));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantEmptyLine)]
        public async Task TestNoDiagnostic_ObjectInitializer()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        var x = new C()
        {
            P1 = 1,
            P2 = 2
        };
    }

    public int P1 { get; set; }
    public int P2 { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantEmptyLine)]
        public async Task TestNoDiagnostic_ObjectInitializer_Singleline()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        var x = new C() { P1 = 1, P2 = 2 };
    }

    public int P1 { get; set; }
    public int P2 { get; set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantEmptyLine)]
        public async Task TestNoDiagnostic_ObjectInitializer_Empty()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        var x = new C() { };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantEmptyLine)]
        public async Task TestNoDiagnostic_EmptyLineBetweenClosingBraceAndSwitchSection()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        string x = null;

        switch (x)
        {
            case ""a"":
                {
                    M();
                    break;
                }

            case ""b"":
                break;
        }
    }
}
");
        }
    }
}
