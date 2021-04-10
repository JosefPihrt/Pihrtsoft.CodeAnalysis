// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1229UseAsyncAwaitTests : AbstractCSharpDiagnosticVerifier<UseAsyncAwaitAnalyzer, UseAsyncAwaitCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.UseAsyncAwait;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseAsyncAwait)]
        public async Task Test_Method_TaskOfT()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<string> [|M|]()
    {
        using (default(IDisposable))
        {
            return GetAsync();
        }
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
", @"
using System;
using System.Threading.Tasks;

class C
{
    async Task<string> M()
    {
        using (default(IDisposable))
        {
            return await GetAsync();
        }
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseAsyncAwait)]
        public async Task Test_Method_Task()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task [|M|]()
    {
        using (default(IDisposable))
        {
            return DoAsync();
        }
    }

    Task DoAsync() => Task.CompletedTask;
}
", @"
using System;
using System.Threading.Tasks;

class C
{
    async Task M()
    {
        using (default(IDisposable))
        {
            await DoAsync();
            return;
        }
    }

    Task DoAsync() => Task.CompletedTask;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseAsyncAwait)]
        public async Task Test_Method_MultipleReturnStatements()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<string> [|M|]()
    {
        bool f = false;
        if (f)
        {
            using (default(IDisposable))
            {
                return GetAsync();
            }
        }
        else
        {
            using (default(IDisposable))
            {
                return GetAsync();
            }
        }
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
", @"
using System;
using System.Threading.Tasks;

class C
{
    async Task<string> M()
    {
        bool f = false;
        if (f)
        {
            using (default(IDisposable))
            {
                return await GetAsync();
            }
        }
        else
        {
            using (default(IDisposable))
            {
                return await GetAsync();
            }
        }
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseAsyncAwait)]
        public async Task Test_LocalFunction()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    void M()
    {
        Task<string> [|LF|]()
        {
            using (default(IDisposable))
            {
                return GetAsync();
            }
        }
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
", @"
using System;
using System.Threading.Tasks;

class C
{
    void M()
    {
        async Task<string> LF()
        {
            using (default(IDisposable))
            {
                return await GetAsync();
            }
        }
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseAsyncAwait)]
        public async Task Test_SimpleLambda()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    void M()
    {
        Func<object, Task<string>> func = [|_ =>
        {
            using (default(IDisposable))
            {
                return GetAsync();
            }
        }|];
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
", @"
using System;
using System.Threading.Tasks;

class C
{
    void M()
    {
        Func<object, Task<string>> func = async _ =>
        {
            using (default(IDisposable))
            {
                return await GetAsync();
            }
        };
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseAsyncAwait)]
        public async Task Test_ParenthesizedLambda()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    void M()
    {
        Func<object, Task<string>> func = [|(_) =>
        {
            using (default(IDisposable))
            {
                return GetAsync();
            }
        }|];
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
", @"
using System;
using System.Threading.Tasks;

class C
{
    void M()
    {
        Func<object, Task<string>> func = async (_) =>
        {
            using (default(IDisposable))
            {
                return await GetAsync();
            }
        };
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseAsyncAwait)]
        public async Task Test_AnonymousMethod()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    void M()
    {
        Func<object, Task<string>> func = [|delegate (object o)
        {
            using (default(IDisposable))
            {
                return GetAsync();
            }
        }|];
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
", @"
using System;
using System.Threading.Tasks;

class C
{
    void M()
    {
        Func<object, Task<string>> func = async delegate (object o)
        {
            using (default(IDisposable))
            {
                return await GetAsync();
            }
        };
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseAsyncAwait)]
        public async Task Test_UsingLocalDeclaration()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

public class C
{
    public Task<string> [|M|]()
    {
        using var disposable = default(IDisposable);
        return GetAsync();
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
", @"
using System;
using System.Threading.Tasks;

public class C
{
    public async Task<string> M()
    {
        using var disposable = default(IDisposable);
        return await GetAsync();
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseAsyncAwait)]
        public async Task Test_UsingLocalDeclaration2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

public class C
{
    public Task<string> [|M|]()
    {
        {
            using var disposable = default(IDisposable);

            {
            }

            return GetAsync();
        }
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
", @"
using System;
using System.Threading.Tasks;

public class C
{
    public async Task<string> M()
    {
        {
            using var disposable = default(IDisposable);

            {
            }

            return await GetAsync();
        }
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseAsyncAwait)]
        public async Task Test_TryCatch()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<int> [|M|]()
    {
        var x = new C();

        try
        {
            return x.GetAsync();
        }
        finally
        {
        }
    }

    Task<int> GetAsync() => Task.FromResult(0);
}
", @"
using System;
using System.Threading.Tasks;

class C
{
    async Task<int> M()
    {
        var x = new C();

        try
        {
            return await x.GetAsync();
        }
        finally
        {
        }
    }

    Task<int> GetAsync() => Task.FromResult(0);
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnusedElementInDocumentationComment)]
        public async Task TestNoDiagnostic_UsingLocalDeclaration()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Threading.Tasks;

public class C
{
    public Task<string> M()
    {
        {
            {
                using var disposable = default(IDisposable);
            }

            return GetAsync();
        }
    }

    Task<string> GetAsync() => Task.FromResult(default(string));
}
");
        }
    }
}
