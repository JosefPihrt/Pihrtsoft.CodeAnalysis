// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1236UseExceptionFilterTests : AbstractCSharpDiagnosticVerifier<UseExceptionFilterAnalyzer, IfStatementCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseExceptionFilter;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExceptionFilter)]
        public async Task Test_IfThrow()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            [|if|] (!(ex is InvalidOperationException))
            {
                throw;
            }

            return;
        }
    }
}
", @"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex) when (ex is InvalidOperationException)
        {

            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExceptionFilter)]
        public async Task Test_IfThrow_Embedded()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            [|if|] (!(ex is InvalidOperationException))
                throw;

            return;
        }
    }
}
", @"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex) when (ex is InvalidOperationException)
        {

            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExceptionFilter)]
        public async Task Test_IfThrowElse()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            [|if|] (!(ex is InvalidOperationException))
            {
                throw;
            }
            else
            {
                return;
            }
        }
    }
}
", @"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex) when (ex is InvalidOperationException)
        {
            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExceptionFilter)]
        public async Task Test_IfThrowElse_Embedded()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            [|if|] (!(ex is InvalidOperationException))
                throw;
            else
                return;
        }
    }
}
", @"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex) when (ex is InvalidOperationException)
        {
            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExceptionFilter)]
        public async Task Test_IfThrowElseIf()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        bool f = false;

        try
        {
        }
        catch (Exception ex)
        {
            [|if|] (!(ex is InvalidOperationException))
            {
                throw;
            }
            else if (f)
            {
                return;
            }
        }
    }
}
", @"
using System;

class C
{
    void M()
    {
        bool f = false;

        try
        {
        }
        catch (Exception ex) when (ex is InvalidOperationException)
        {
            if (f)
            {
                return;
            }
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExceptionFilter)]
        public async Task Test_IfElseThrow()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            [|if|] (ex is InvalidOperationException)
            {
                return;
            }
            else
            {
                throw;
            }
        }
    }
}
", @"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex) when (ex is InvalidOperationException)
        {
            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExceptionFilter)]
        public async Task TestNoDiagnostic_HasFilter()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex) when (ex is InvalidOperationException)
        {
            if (!(ex is InvalidOperationException))
            {
                throw;
            }

            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExceptionFilter)]
        public async Task TestNoDiagnostic_ConditionContainsAwait()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    async Task M()
    {
        try
        {
        }
        catch (Exception)
        {
            if (await Async() != null)
            {
                throw;
            }
        }

        Task<object> Async() => Task.FromResult(default(object));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExceptionFilter)]
        public async Task TestNoDiagnostic_BothBranchesThrow()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            if (!(ex is InvalidOperationException))
            {
                throw;
            }
            else
            {
                throw;
            }
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExceptionFilter)]
        public async Task TestNoDiagnostic_ContainsMethodThatCanThrow()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            if (ex != null && ThrowIf(ex))
                throw;
        }
    }

    bool ThrowIf(Exception ex) => throw new Exception();
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExceptionFilter)]
        public async Task TestNoDiagnostic_ContainsMethodThatCanThrow2()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            if (ex != null && ThrowIf<object>(ex))
                throw;
        }
    }

    bool ThrowIf<T>(Exception ex) => throw new Exception();
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExceptionFilter)]
        public async Task TestNoDiagnostic_ContainsMethodThatCanThrow3()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

static class C
{
    static void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            if (ex?.ThrowIf() == true)
                throw;
        }
    }

    static bool ThrowIf(this Exception ex) => false;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExceptionFilter)]
        public async Task TestNoDiagnostic_ContainsMethodThatCanThrow_XmlCommentContainsException()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            if (ex != null && M2(ex))
                throw;
        }
    }

    /// <summary></summary>
    /// <exception cref=""Exception""></exception>
    bool M2(Exception ex)
    {
        return false;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExceptionFilter)]
        public async Task TestNoDiagnostic_ContainsThrowExpression()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            if (M2(ex ?? throw new Exception()))
                throw;
        }
    }

    bool M2(Exception ex) => false;
}
");
        }
    }
}
