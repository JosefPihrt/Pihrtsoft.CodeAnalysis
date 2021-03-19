// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Testing;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1046AsynchronousMethodNameShouldEndWithAsyncTests : AbstractCSharpDiagnosticVerifier<AsyncSuffixAnalyzer, DummyCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AsynchronousMethodNameShouldEndWithAsync;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AsynchronousMethodNameShouldEndWithAsync)]
        public async Task Test_Task()
        {
            await VerifyDiagnosticAsync(@"
using System.Threading.Tasks;

class B
{
    public virtual Task [|Foo|]()
    {
        return Task.CompletedTask;
    }
}

class C : B
{
    public override Task Foo()
    {
        return base.Foo();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AsynchronousMethodNameShouldEndWithAsync)]
        public async Task Test_Task_TypeParameter()
        {
            await VerifyDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    T [|Foo|]<T>() where T : Task
    {
        return default(T);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AsynchronousMethodNameShouldEndWithAsync)]
        public async Task Test_TaskOfT()
        {
            await VerifyDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    Task<object> [|Foo|]()
    {
        return Task.FromResult(default(object));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AsynchronousMethodNameShouldEndWithAsync)]
        public async Task Test_TaskOfT_TypeParameter()
        {
            await VerifyDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    T [|Foo|]<T>() where T : Task<object>
    {
        return default(T);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AsynchronousMethodNameShouldEndWithAsync)]
        public async Task Test_ValueTaskOfT()
        {
            await VerifyDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    ValueTask<object> [|Foo|]()
    {
        return default(ValueTask<object>);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AsynchronousMethodNameShouldEndWithAsync)]
        public async Task TestNoDiagnostic_EntryPointMethod()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        await Task.CompletedTask;
    }
}
");
        }
    }
}
