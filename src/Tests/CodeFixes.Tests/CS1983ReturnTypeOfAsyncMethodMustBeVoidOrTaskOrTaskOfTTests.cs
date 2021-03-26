// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS1983ReturnTypeOfAsyncMethodMustBeVoidOrTaskOrTaskOfTTests : AbstractCSharpCompilerDiagnosticFixVerifier<ReturnTypeOfAsyncMethodMustBeVoidOrTaskOrTaskOfTCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.ReturnTypeOfAsyncMethodMustBeVoidOrTaskOrTaskOfT;

        public override CSharpTestOptions Options
        {
            get { return base.Options.AddAllowedCompilerDiagnosticId(CompilerDiagnosticIdentifiers.SinceMethodIsAsyncMethodThatReturnsTaskReturnKeywordMustNotBeFollowedByObjectExpression); }
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.ReturnTypeOfAsyncMethodMustBeVoidOrTaskOrTaskOfT)]
        public async Task Test_Task()
        {
            await VerifyFixAsync(@"
using System.Threading.Tasks;

public class Foo
{
    public async object Bar()
    {
        await DoAsync().ConfigureAwait(false);

        return await GetAsync().ConfigureAwait(false);
    }

    public async object Bar2()
    {
        await DoAsync().ConfigureAwait(false);
    }

    public void Bar3()
    {
        async object LocalBar()
        {
            await DoAsync().ConfigureAwait(false);

            return await GetAsync().ConfigureAwait(false);
        }

        async object LocalBar2()
        {
            await DoAsync().ConfigureAwait(false);
        }
    }

    public Task<object> GetAsync()
    {
        return Task.FromResult(default(object));
    }

    public Task DoAsync()
    {
        return Task.CompletedTask;
    }
}
", @"
using System.Threading.Tasks;

public class Foo
{
    public async Task Bar()
    {
        await DoAsync().ConfigureAwait(false);

        return await GetAsync().ConfigureAwait(false);
    }

    public async Task Bar2()
    {
        await DoAsync().ConfigureAwait(false);
    }

    public void Bar3()
    {
        async Task LocalBar()
        {
            await DoAsync().ConfigureAwait(false);

            return await GetAsync().ConfigureAwait(false);
        }

        async Task LocalBar2()
        {
            await DoAsync().ConfigureAwait(false);
        }
    }

    public Task<object> GetAsync()
    {
        return Task.FromResult(default(object));
    }

    public Task DoAsync()
    {
        return Task.CompletedTask;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, "Task"));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.ReturnTypeOfAsyncMethodMustBeVoidOrTaskOrTaskOfT)]
        public async Task Test_TaskOfT()
        {
            await VerifyFixAsync(@"
using System.Threading.Tasks;

public class Foo
{
    public async object Bar()
    {
        object x = await GetAsync().ConfigureAwait(false);

        return await GetAsync().ConfigureAwait(false);
    }

    public async object Bar2()
    {
        await DoAsync().ConfigureAwait(false);

        return await GetAsync().ConfigureAwait(false);
    }

    public async object Bar3() => await GetAsync().ConfigureAwait(false);

    public void Bar4()
    {
        async object LocalBar()
        {
            object x = await GetAsync().ConfigureAwait(false);

            return await GetAsync().ConfigureAwait(false);
        }

        async object LocalBar2()
        {
            await DoAsync().ConfigureAwait(false);

            return await GetAsync().ConfigureAwait(false);
        }

        async object LocalBar3() => await GetAsync().ConfigureAwait(false);
    }

    public Task<object> GetAsync()
    {
        return Task.FromResult(default(object));
    }

    public Task DoAsync()
    {
        return Task.CompletedTask;
    }
}
", @"
using System.Threading.Tasks;

public class Foo
{
    public async Task<object> Bar()
    {
        object x = await GetAsync().ConfigureAwait(false);

        return await GetAsync().ConfigureAwait(false);
    }

    public async Task<object> Bar2()
    {
        await DoAsync().ConfigureAwait(false);

        return await GetAsync().ConfigureAwait(false);
    }

    public async Task<object> Bar3() => await GetAsync().ConfigureAwait(false);

    public void Bar4()
    {
        async Task<object> LocalBar()
        {
            object x = await GetAsync().ConfigureAwait(false);

            return await GetAsync().ConfigureAwait(false);
        }

        async Task<object> LocalBar2()
        {
            await DoAsync().ConfigureAwait(false);

            return await GetAsync().ConfigureAwait(false);
        }

        async Task<object> LocalBar3() => await GetAsync().ConfigureAwait(false);
    }

    public Task<object> GetAsync()
    {
        return Task.FromResult(default(object));
    }

    public Task DoAsync()
    {
        return Task.CompletedTask;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, "TaskOfT"));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.ReturnTypeOfAsyncMethodMustBeVoidOrTaskOrTaskOfT)]
        public async Task TestNoFix()
        {
            const string source = @"
using System.Threading.Tasks;

public class Foo
{
    public async X Bar()
    {
        await DoAsync().ConfigureAwait(false);

        return await GetAsync().ConfigureAwait(false);
    }

    public async X Bar2() => await GetAsync().ConfigureAwait(false);

    public void LocalFunction()
    {
        async X LocalBar()
        {
            await DoAsync().ConfigureAwait(false);

            return await GetAsync().ConfigureAwait(false);
        }

        async X LocalBar2() => await GetAsync().ConfigureAwait(false);
    }

    public Task<object> GetAsync()
    {
        return Task.FromResult(default(object));
    }

    public Task DoAsync()
    {
        return Task.CompletedTask;
    }
}
";
            await VerifyNoFixAsync(source, equivalenceKey: EquivalenceKey.Create(DiagnosticId, "Task"));

            await VerifyNoFixAsync(source, equivalenceKey: EquivalenceKey.Create(DiagnosticId, "TaskOfT"));
        }
    }
}
