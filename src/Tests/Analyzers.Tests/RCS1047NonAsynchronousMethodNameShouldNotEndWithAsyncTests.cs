// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Testing;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1047NonAsynchronousMethodNameShouldNotEndWithAsyncTests : AbstractCSharpDiagnosticVerifier<AsyncSuffixAnalyzer, DummyCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.NonAsynchronousMethodNameShouldNotEndWithAsync;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.NonAsynchronousMethodNameShouldNotEndWithAsync)]
        public async Task Test()
        {
            await VerifyDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    void [|FooAsync|]()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.NonAsynchronousMethodNameShouldNotEndWithAsync)]
        public async Task Test_Tuple()
        {
            await VerifyDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    (string s1, string s2) [|FooAsync|]()
    {
        return default((string, string));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.NonAsynchronousMethodNameShouldNotEndWithAsync)]
        public async Task Test_String()
        {
            await VerifyDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    string [|FooAsync|]()
    {
        return null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.NonAsynchronousMethodNameShouldNotEndWithAsync)]
        public async Task Test_Array()
        {
            await VerifyDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    string[] [|FooAsync|]()
    {
        return null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.NonAsynchronousMethodNameShouldNotEndWithAsync)]
        public async Task Test_T()
        {
            await VerifyDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    T [|FooAsync|]<T>()
    {
        return default(T);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.NonAsynchronousMethodNameShouldNotEndWithAsync)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    async Task<object> GetAsync()
    {
        return await Task.FromResult<object>(null);
    }

    Task<object> TaskOfTAsync()
    {
        return Task.FromResult<object>(null);
    }

    T TaskOfTAsync<T>() where T : Task<object>
    {
        return default(T);
    }

    Task TaskAsync()
    {
        return default(Task);
    }

    T TaskAsync<T>() where T : Task
    {
        return default(T);
    }

    ValueTask<object> ValueTaskOfTAsync()
    {
        return default(ValueTask<object>);
    }
}
");
        }
    }
}
