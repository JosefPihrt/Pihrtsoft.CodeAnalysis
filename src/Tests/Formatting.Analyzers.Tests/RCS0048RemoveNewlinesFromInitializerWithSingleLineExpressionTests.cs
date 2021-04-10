// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0048RemoveNewlinesFromInitializerWithSingleLineExpressionTests : AbstractCSharpDiagnosticVerifier<RemoveNewlinesFromInitializerWithSingleLineExpressionAnalyzer, InitializerCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.RemoveNewlinesFromInitializerWithSingleLineExpression;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task Test_ObjectInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object P { get; set; }

    void M()
    {
        var x = new C()
        [|{
            P = null
        }|];
    }
}
", @"
class C
{
    object P { get; set; }

    void M()
    {
        var x = new C() { P = null };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task Test_ObjectInitializer_TrailingComma()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object P { get; set; }

    void M()
    {
        var x = new C()
        [|{
            P = null,
        }|];
    }
}
", @"
class C
{
    object P { get; set; }

    void M()
    {
        var x = new C() { P = null };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task Test_ObjectInitializer_TrailingComma2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    public C P { get; set; }

    void M()
    {
        var items = new List<C>()
        {
            new C
            {
                P =
                new C()
            },
            new C
            [|{
                P = new C(),
            }|],
        };
    }
}
", @"
using System.Collections.Generic;

class C
{
    public C P { get; set; }

    void M()
    {
        var items = new List<C>()
        {
            new C
            {
                P =
                new C()
            },
            new C { P = new C() },
        };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task Test_ObjectInitializer_AssignmentExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    C P1 { get; set; }

    string P2 { get; set; }

    void M()
    {
        var x = new C()
        {
            P2 = null,
            P1 =
            [|{
                P2 = null
            }|]
        };
    }
}
", @"
class C
{
    C P1 { get; set; }

    string P2 { get; set; }

    void M()
    {
        var x = new C()
        {
            P2 = null,
            P1 = { P2 = null }
        };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task Test_ObjectInitializer_TrailingComma_AssignmentExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    C P1 { get; set; }

    string P2 { get; set; }

    void M()
    {
        var x = new C()
        {
            P2 = null,
            P1 =
            [|{
                P2 = null,
            }|]
        };
    }
}
", @"
class C
{
    C P1 { get; set; }

    string P2 { get; set; }

    void M()
    {
        var x = new C()
        {
            P2 = null,
            P1 = { P2 = null }
        };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task Test_DictionaryInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var dic = new Dictionary<int, string>()
        [|{
            { 0, null }
        }|];
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var dic = new Dictionary<int, string>() { { 0, null } };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task Test_DictionaryInitializer_CSharp6()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var dic = new Dictionary<int, string>()
        [|{
            [0] = null
        }|];
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var dic = new Dictionary<int, string>() { [0] = null };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task Test_CollectionInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>()
        [|{
            null
        }|];
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>() { null };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task Test_ArrayInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        var items = new C[]
        [|{
            null
        }|];
    }
}
", @"
class C
{
    void M()
    {
        var items = new C[] { null };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task Test_ArrayInitializer_Field()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string[] _f =
    [|{
        null
    }|];
}
", @"
class C
{
    string[] _f = { null };
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task Test_ArrayInitializer_TrailingComma_Field()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string[] _f =
    [|{
        null,
    }|];
}
", @"
class C
{
    string[] _f = { null };
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task Test_ArrayInitializer_Local()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string[] x =
        [|{
            null
        }|];
    }
}
", @"
class C
{
    void M()
    {
        string[] x = { null };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task Test_ArrayInitializer_TrailingComma_Local()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string[] x =
        [|{
            null,
        }|];
    }
}
", @"
class C
{
    void M()
    {
        string[] x = { null };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task TestNoDiagnostic_MultipleExpressions()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    object P1 { get; set; }
    object P2 { get; set; }

    void M()
    {
        var x = new C()
        {
            P1 = null,
            P2 = null
        };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task TestNoDiagnostic_MultilineExpression()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    object P1 { get; set; }
    object P2 { get; set; }

    void M()
    {
        var x = new C()
        {
            P1
                = null
        };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task TestNoDiagnostic_CommentAfterCloseParenthesis()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    object P { get; set; }

    void M()
    {
        var x = new C() // x
        {
            P = null
        };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task TestNoDiagnostic_CommentBeforeOpenBrace()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    object P { get; set; }

    void M()
    {
        var x = new C()
       /**/ {
            P = null
        };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task TestNoDiagnostic_CommentAfterOpenBrace()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    object P { get; set; }

    void M()
    {
        var x = new C()
        { // x
            P = null
        };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task TestNoDiagnostic_CommentBeforeExpression()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    object P { get; set; }

    void M()
    {
        var x = new C()
        {
           /**/ P = null
        };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task TestNoDiagnostic_CommentAfterExpression()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    object P { get; set; }

    void M()
    {
        var x = new C()
        {
            P = null // x
        };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task TestNoDiagnostic_CommentAfterComma()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    object P { get; set; }

    void M()
    {
        var x = new C()
        {
            P = null, // x
        };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewlinesFromInitializerWithSingleLineExpression)]
        public async Task TestNoDiagnostic_CommentBeforeCloseBrace()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    object P { get; set; }

    void M()
    {
        var x = new C()
        {
            P = null
       /**/ };
    }
}
");
        }
    }
}
