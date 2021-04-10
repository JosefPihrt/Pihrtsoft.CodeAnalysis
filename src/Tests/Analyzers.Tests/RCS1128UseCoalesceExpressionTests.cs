// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1128UseCoalesceExpressionTests : AbstractCSharpDiagnosticVerifier<UseCoalesceExpressionAnalyzer, UseCoalesceExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.UseCoalesceExpression;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task Test_LocalDeclarationStatement()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s)
    {
        [|string x = s;|]

        if (x == null)
        {
            x = (true) ? ""a"" : ""b"";
        }
    }
}
", @"
class C
{
    void M(string s)
    {
        string x = s ?? ((true) ? ""a"" : ""b"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task Test_LocalDeclarationStatement_IsNull()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s)
    {
        [|string x = s;|]

        if (x is null)
        {
            x = (true) ? ""a"" : ""b"";
        }
    }
}
", @"
class C
{
    void M(string s)
    {
        string x = s ?? ((true) ? ""a"" : ""b"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task Test_LocalDeclarationStatement_WithComments()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s)
    {
        [|string x = s;|]

        // a
        if (x == null)
        {
            // b
            x = (true) ? ""a"" : ""b"";
        }
    }
}
", @"
class C
{
    void M(string s)
    {
        string x = s ?? ((true) ? ""a"" : ""b"");

        // a
           
        
            // b
                  
        
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task Test_LocalDeclarationStatement_EmbeddedStatement()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s)
    {
        [|string x = s;|]

        if (x == null)
            x = """";
    }
}
", @"
class C
{
    void M(string s)
    {
        string x = s ?? """";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task Test_LocalDeclarationStatement_CastExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C : B
{
    void M(C c)
    {
        [|B b = c;|]

        if (b == null)
        {
            b = default(C);
        }
    }
}

class B { }
", @"
class C : B
{
    void M(C c)
    {
        B b = c ?? (B)default(C);
    }
}

class B { }
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task Test_SimpleAssignmentStatement()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s, string x)
    {
        [|x = """";|]

        if (x == null)
        {
            x = (true) ? ""a"" : ""b"";
        }
    }
}
", @"
class C
{
    void M(string s, string x)
    {
        x = """" ?? ((true) ? ""a"" : ""b"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task Test_SimpleAssignmentStatement_CastExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C : B
{
    void M(C c, B b)
    {
        [|b = c;|]

        if (b == null)
        {
            b = default(C);
        }
    }
}

class B { }
", @"
class C : B
{
    void M(C c, B b)
    {
        b = c ?? (B)default(C);
    }
}

class B { }
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task Test_IfStatement_Nullable()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(int? i)
    {
        [|int? x = i;|]

        if (x == null)
        {
            x = (true) ? 1 : 2;
        }
    }
}
", @"
class C
{
    void M(int? i)
    {
        int? x = i ?? (int?)((true) ? 1 : 2);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task Test_IfStatement_Nullable_NotHasValue()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(int? i)
    {
        [|int? x = i;|]

        if (!x.HasValue)
        {
            x = (true) ? 1 : 2;
        }
    }
}
", @"
class C
{
    void M(int? i)
    {
        int? x = i ?? (int?)((true) ? 1 : 2);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task Test_LocalDeclarationStatement_Nullable()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(int? i)
    {
        [|int? x = i;|]

        if (x == null)
        {
            x = (true) ? 1 : 2;
        }
    }
}
", @"
class C
{
    void M(int? i)
    {
        int? x = i ?? (int?)((true) ? 1 : 2);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task Test_SimpleAssignmentStatement_Nullable()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(int? i, int? x)
    {
        [|x = i;|]

        if (x == null)
        {
            x = (true) ? 1 : 2;
        }
    }
}
", @"
class C
{
    void M(int? i, int? x)
    {
        x = i ?? (int?)((true) ? 1 : 2);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task TestNoDiagnostic_NotEqualsToNull()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        string x = """";

        if (x != null)
            x = """";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task TestNoDiagnostic_NotIsNull()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        string x = """";

        if (!(x is null))
            x = """";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task TestNoDiagnostic_HasValue()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        int? x = 0;

        if (x.HasValue)
            x = 0;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpression)]
        public async Task TestNoDiagnostic_RefType()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        ref object x = ref GetRef();

        if (x == null)
            x = new object();
    }

    private ref object GetRef()
    {
        throw new NotImplementedException();
    }
}
");
        }
    }
}
