// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1044RemoveOriginalExceptionFromThrowStatementTests : AbstractCSharpDiagnosticVerifier<RemoveOriginalExceptionFromThrowStatementAnalyzer, RemoveOriginalExceptionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.RemoveOriginalExceptionFromThrowStatement;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveOriginalExceptionFromThrowStatement)]
        public async Task Test_OriginalExceptionUsed()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

public class A
{
    public void Foo()
    {
        try
        {
            Foo();
        }
        catch (Exception ex)
        {        
            throw [|ex|];
        }
    }
}
", @"
using System;

public class A
{
    public void Foo()
    {
        try
        {
            Foo();
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveOriginalExceptionFromThrowStatement)]
        public async Task TestNoDiagnostic_OnlyThrowStatement()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

public class A
{
    public void Foo()
    {
        try
        {
            Foo();
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveOriginalExceptionFromThrowStatement)]
        public async Task TestNoDiagnostic_NewExceptionInstantiated()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

public class A
{
    public void Foo()
    {
        try
        {
            Foo();
        }
        catch (Exception ex)
        {        
            throw new Exception();
        }
    }
}
");
        }
    }
}
