// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1174RemoveRedundantAsyncAwaitTests : AbstractCSharpDiagnosticVerifier<RemoveRedundantAsyncAwaitAnalyzer, RemoveRedundantAsyncAwaitCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.RemoveRedundantAsyncAwait;

        public override CSharpTestOptions Options
        {
            get { return base.Options.AddAllowedCompilerDiagnosticId(CompilerDiagnosticIdentifiers.UnreachableCodeDetected); }
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task Test_Method_Body_ReturnAwait()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Threading.Tasks;

class C
{
    [|async|] Task<object> GetAsync()
    {
        return await GetAsync();

        object LF() => null;
    }
}
", @"
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        return GetAsync();

        object LF() => null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task Test_Method_Body_ReturnAwait_ConfigureAwait()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Threading.Tasks;

class C
{
    [|async|] Task<object> GetAsync()
    {
        return await GetAsync().ConfigureAwait(false);
    }
}
", @"
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        return GetAsync();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task Test_Method_ExpressionBody()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Threading.Tasks;

class C
{
    [|async|] Task<object> GetAsync() => await GetAsync();
}
", @"
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync() => GetAsync();
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task Test_LocalFunction_Body_ReturnAwait()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Threading.Tasks;

class C
{
    void M()
    {
        [|async|] Task<object> GetAsync()
        {
            return await GetAsync();
        }
    }
}
", @"
using System.Threading.Tasks;

class C
{
    void M()
    {
        Task<object> GetAsync()
        {
            return GetAsync();
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task Test_LocalFunction_ExpressionBody()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Threading.Tasks;

class C
{
    void M()
    {
        [|async|] Task<object> GetAsync() => await GetAsync();
    }
}
", @"
using System.Threading.Tasks;

class C
{
    void M()
    {
        Task<object> GetAsync() => GetAsync();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task Test_SimpleLambda_Body()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = [|async|] f =>
        {
            return await GetAsync();
        };

        return GetAsync();
    }
}
", @"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = f =>
        {
            return GetAsync();
        };

        return GetAsync();
    }
}
", options: Options.AddAllowedCompilerDiagnosticId("CS1998"));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task Test_SimpleLambda_ExpressionBody()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = [|async|] f => await GetAsync();

        return GetAsync();
    }
}
", @"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = f => GetAsync();

        return GetAsync();
    }
}
", options: Options.AddAllowedCompilerDiagnosticId("CS1998"));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task Test_ParenthesizedLambda_Body()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = [|async|] (f) =>
        {
            return await GetAsync();
        };

        return GetAsync();
    }
}
", @"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = (f) =>
        {
            return GetAsync();
        };

        return GetAsync();
    }
}
", options: Options.AddAllowedCompilerDiagnosticId("CS1998"));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task Test_ParenthesizedLambda_ExpressionBody()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = [|async|] (f) => await GetAsync();

        return GetAsync();
    }
}
", @"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = (f) => GetAsync();

        return GetAsync();
    }
}
", options: Options.AddAllowedCompilerDiagnosticId("CS1998"));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task Test_AnonymousMethod()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = [|async|] delegate (object f)
        {
            return await GetAsync();
        };

        return GetAsync();
    }
}
", @"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = delegate (object f)
        {
            return GetAsync();
        };

        return GetAsync();
    }
}
", options: Options.AddAllowedCompilerDiagnosticId("CS1998"));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task Test_IfElseIfReturn()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Threading.Tasks;

class C
{
    [|async|] Task<object> GetAsync()
    {
        bool f = false;

        if (f)
        {
            return await GetAsync();
        }
        else if (f)
        {
            return await GetAsync();
        }

        return await GetAsync();
    }
}
", @"
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        bool f = false;

        if (f)
        {
            return GetAsync();
        }
        else if (f)
        {
            return GetAsync();
        }

        return GetAsync();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task Test_IfElse()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Threading.Tasks;

class C
{
    [|async|] Task<object> GetAsync()
    {
        bool f = false;

        if (f)
        {
            return await GetAsync();
        }
        else
        {
            return await GetAsync();
        }
    }
}
", @"
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        bool f = false;

        if (f)
        {
            return GetAsync();
        }
        else
        {
            return GetAsync();
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task Test_SwitchWithoutDefaultSection()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Threading.Tasks;

class C
{
    [|async|] Task<object> GetAsync()
    {
        bool f = false;

        switch (f)
        {
            case true:
                {
                    return await GetAsync();
                }
            case false:
                {
                    return await GetAsync();
                }
        }

        return await GetAsync();
    }
}
", @"
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        bool f = false;

        switch (f)
        {
            case true:
                {
                    return GetAsync();
                }
            case false:
                {
                    return GetAsync();
                }
        }

        return GetAsync();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task Test_SwitchWithDefaultSection()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Threading.Tasks;

class C
{
    [|async|] Task<object> GetAsync()
    {
        string s = null;

        switch (s)
        {
            case ""a"":
                {
                    return await GetAsync();
                }
            case ""b"":
                {
                    return await GetAsync();
                }
            default:
                {
                    return await GetAsync();
                }
        }
    }
}
", @"
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        string s = null;

        switch (s)
        {
            case ""a"":
                {
                    return GetAsync();
                }
            case ""b"":
                {
                    return GetAsync();
                }
            default:
                {
                    return GetAsync();
                }
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task TestNoDiagnostic_IfElse_ReturnWithoutAwait()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    async Task<object> FooAsync()
    {
        bool f = false;
        if (f)
        {
            if (f)
            {
                return default(object);
            }

            return await GetAsync();
        }
        else
        {
            return await GetAsync();
        }
    }

    Task<object> GetAsync() => Task.FromResult(default(object));
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task TestNoDiagnostic_IfElse_AwaitWithoutReturn()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    async Task<object> FooAsync()
    {
        bool f = false;
        if (f)
        {
            await GetAsync();
            return await GetAsync();
        }
        else
        {
            return await GetAsync();
        }
    }

    Task<object> GetAsync() => Task.FromResult(default(object));
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task TestNoDiagnostic_Switch_ReturnWithoutAwait()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    async Task<object> FooAsync()
    {
        bool f = false;
        switch (f)
        {
            case true:
                {
                    if (f)
                    {
                        return default(object);
                    }

                    return await GetAsync();
                }
            case false:
                {
                    return await GetAsync();
                }
        }

        return await GetAsync();
    }

    Task<object> GetAsync() => Task.FromResult(default(object));
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task TestNoDiagnostic_Switch_AwaitWithoutReturn()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    async Task<object> FooAsync()
    {
        bool f = false;
        switch (f)
        {
            case true:
                {
                    await GetAsync();
                    return await GetAsync();
                }
            case false:
                {
                    return await GetAsync();
                }
        }

        return await GetAsync();
    }

    Task<object> GetAsync() => Task.FromResult(default(object));
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task TestNoDiagnostic_Method_ReturnWithoutAwait()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    async Task<object> FooAsync()
    {
        bool f = false;

        if (f)
        {
            if (f)
            {
                return default(object);
            }
        }

        return await GetAsync();
    }

    Task<object> GetAsync() => Task.FromResult(default(object));
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task TestNoDiagnostic_Method_AwaitWithoutReturn()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    async Task<object> FooAsync()
    {
        bool f = false;

        if (f)
        {
            if (f)
            {
                await GetAsync();
            }
        }

        return await GetAsync();
    }

    Task<object> GetAsync() => Task.FromResult(default(object));
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task TestNoDiagnostic_Method_ReturnsTask()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Threading.Tasks;

class C
{
    async Task DoAsync()
    {
        return await DoAsync();
    }
}
", options: Options.AddAllowedCompilerDiagnosticId(CompilerDiagnosticIdentifiers.SinceMethodIsAsyncMethodThatReturnsTaskReturnKeywordMustNotBeFollowedByObjectExpression));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task TestNoDiagnostic_ReturnTypeAndAwaitTypeDoNotEqual()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<string> GetAsync() => Task.FromResult(default(string));

    async Task<object> MethodWitBodyAsync()
    {
        return await GetAsync();

        async Task<object> LocalWithBodyAsync()
        {
            return await GetAsync();
        }

        async Task<object> LocalWithExpressionBodyAsync() => await GetAsync();
    }

    async Task<object> MethodWithExpressionBodyAsync() => await GetAsync();

    void Foo()
    {
        Func<object, Task<object>> func = async f =>
        {
            return await GetAsync();
        };

        Func<object, Task<object>> func2 = async f => await GetAsync();

        Func<object, Task<object>> func3 = async (f) =>
        {
            return await GetAsync();
        };

        Func<object, Task<object>> func4 = async (f) => await GetAsync();

        Func<object, Task<object>> func5 = async delegate (object f)
        {
            return await GetAsync();
        };
    }

    async Task<object> IfElseIfAsync()
    {
        bool f = false;

        if (f)
        {
            return await GetAsync();
        }
        else if (f)
        {
            return await GetAsync();
        }

        return await GetAsync();
    }

    async Task<object> IfElseAsync()
    {
        bool f = false;

        if (f)
        {
            return await GetAsync();
        }
        else
        {
            return await GetAsync();
        }
    }

    async Task<object> SwitchWithoutDefaultAsync()
    {
        bool f = false;

        switch (f)
        {
            case true:
                {
                    return await GetAsync();
                }
            case false:
                {
                    return await GetAsync();
                }
        }

        return await GetAsync();
    }

    async Task<object> SwitchWithDefaultAsync()
    {
        string s = null;

        switch (s)
        {
            case ""a"":
                {
                    return await GetAsync();
                }
            case ""b"":
                {
                    return await GetAsync();
                }
            default:
                {
                    return await GetAsync();
                }
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAsyncAwait)]
        public async Task TestNoDiagnostic_AwaitContainsAwait()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync() => Task.FromResult(default(object));

    Task<object> GetAsync(object p) => Task.FromResult(p);

    async Task<object> MethodWitBodyAsync()
    {
        return await GetAsync(await GetAsync());

        async Task<object> LocalWithBodyAsync()
        {
            return await GetAsync(await GetAsync());
        }

        async Task<object> LocalWithExpressionBodyAsync() => await GetAsync(await GetAsync());
    }

    async Task<object> MethodWithExpressionBodyAsync() => await GetAsync(await GetAsync());

    void AnonymousFunction()
    {
        Func<object, Task<object>> func = async f =>
        {
            return await GetAsync(await GetAsync());
        };

        Func<object, Task<object>> func2 = async f => await GetAsync(await GetAsync());

        Func<object, Task<object>> func3 = async (f) =>
        {
            return await GetAsync(await GetAsync());
        };

        Func<object, Task<object>> func4 = async (f) => await GetAsync(await GetAsync());

        Func<object, Task<object>> func5 = async delegate (object f)
        {
            return await GetAsync(await GetAsync());
        };
    }

    async Task<object> IfElseIfAsync()
    {
        bool f = false;

        if (f)
        {
            return await GetAsync(await GetAsync());
        }
        else if (f)
        {
            return await GetAsync(await GetAsync());
        }

        return await GetAsync(await GetAsync());
    }

    async Task<object> IfElseAsync()
    {
        bool f = false;

        if (f)
        {
            return await GetAsync(await GetAsync());
        }
        else
        {
            return await GetAsync(await GetAsync());
        }
    }

    async Task<object> SwitchWithoutDefaultAsync()
    {
        string s = null;

        switch (s)
        {
            case ""a"":
                {
                    return await GetAsync(await GetAsync());
                }
            case ""b"":
                {
                    return await GetAsync(await GetAsync());
                }
        }

        return await GetAsync(await GetAsync());
    }

    async Task<object> SwitchWithDefaultAsync()
    {
        string s = null;

        switch (s)
        {
            case ""a"":
                {
                    return await GetAsync(await GetAsync());
                }
            case ""b"":
                {
                    return await GetAsync(await GetAsync());
                }
            default:
                {
                    return await GetAsync(await GetAsync());
                }
        }
    }
}
");
        }
    }
}
