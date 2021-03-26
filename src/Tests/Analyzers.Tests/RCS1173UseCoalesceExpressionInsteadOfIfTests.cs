// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1173UseCoalesceExpressionInsteadOfIfTests : AbstractCSharpDiagnosticVerifier<IfStatementAnalyzer, IfStatementCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseCoalesceExpressionInsteadOfIf;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpressionInsteadOfIf)]
        public async Task Test_IfElseToAssignment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string x, string y, string z)
    {
        //x
        [|if (x != null)
        {
            z = x;
        }
        else
        {
            z = y;
        }|]
    }
}
", @"
class C
{
    void M(string x, string y, string z)
    {
        //x
        z = x ?? y;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpressionInsteadOfIf)]
        public async Task Test_IfElseToReturn()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M(string x, string y)
    {
        //x
        [|if (x != null)
        {
            return x;
        }
        else
        {
            return y;
        }|]
    }
}
", @"
class C
{
    string M(string x, string y)
    {
        //x
        return x ?? y;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpressionInsteadOfIf)]
        public async Task Test_IfElseToYieldReturn()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(string x, string y)
    {
        //x
        [|if (x != null)
        {
            yield return x;
        }
        else
        {
            yield return y;
        }|]
    }
}
", @"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(string x, string y)
    {
        //x
        yield return x ?? y;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpressionInsteadOfIf)]
        public async Task Test_IfReturnToReturn()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M(string x, string y)
    {
        //x
        [|if (x != null)
        {
            return x;
        }|]

        return y;
    }
}
", @"
class C
{
    string M(string x, string y)
    {
        //x
        return x ?? y;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpressionInsteadOfIf)]
        public async Task TestNoDiagnostic_IfElseContainsComment()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(string x, string y, string z)
    {
        if (x != null)
        {
            z = x;
        }
        else
        {
            //x
            z = y;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpressionInsteadOfIf)]
        public async Task TestNoDiagnostic_IfElseContainsDirective()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(string x, string y, string z)
    {
        if (x != null)
        {
            z = x;
        }
        else
        {
#if DEBUG
            z = y;
#endif
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpressionInsteadOfIf)]
        public async Task TestNoDiagnostic_IfReturnContainsComment()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(string x, string y, string z)
    {
        if (x != null)
        {
            z = x;
        }

        //x
        z = y;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseCoalesceExpressionInsteadOfIf)]
        public async Task TestNoDiagnostic_IfReturnContainsDirective()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(string x, string y, string z)
    {
        if (x != null)
        {
            z = x;
        }

#if DEBUG
        z = y;
#endif
    }
}
");
        }
    }
}
