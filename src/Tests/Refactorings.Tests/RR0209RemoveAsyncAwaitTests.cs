// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0209RemoveAsyncAwaitTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.RemoveAsyncAwait;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAsyncAwait)]
        public async Task Test_Method_Body_ReturnAwait()
        {
            await VerifyRefactoringAsync(@"
using System.Threading.Tasks;

class C
{
    [||]async Task<object> GetAsync()
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

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAsyncAwait)]
        public async Task Test_Method_Body_ReturnAwait_ConfigureAwait()
        {
            await VerifyRefactoringAsync(@"
using System.Threading.Tasks;

class C
{
    [||]async Task<object> GetAsync()
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

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAsyncAwait)]
        public async Task Test_Method_ExpressionBody()
        {
            await VerifyRefactoringAsync(@"
using System.Threading.Tasks;

class C
{
    [||]async Task<object> GetAsync() => await GetAsync();
}
", @"
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync() => GetAsync();
}
");
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAsyncAwait)]
        public async Task Test_LocalFunction_Body_ReturnAwait()
        {
            await VerifyRefactoringAsync(@"
using System.Threading.Tasks;

class C
{
    void M()
    {
        [||]async Task<object> GetAsync()
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

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAsyncAwait)]
        public async Task Test_LocalFunction_ExpressionBody()
        {
            await VerifyRefactoringAsync(@"
using System.Threading.Tasks;

class C
{
    void M()
    {
        [||]async Task<object> GetAsync() => await GetAsync();
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

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAsyncAwait)]
        public async Task Test_SimpleLambda_Body()
        {
            await VerifyRefactoringAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = [||]async f =>
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
", options: Options.AddAllowedCompilerDiagnosticId("CS1998"), equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAsyncAwait)]
        public async Task Test_SimpleLambda_ExpressionBody()
        {
            await VerifyRefactoringAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = [||]async f => await GetAsync();

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

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAsyncAwait)]
        public async Task Test_ParenthesizedLambda_Body()
        {
            await VerifyRefactoringAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = [||]async (f) =>
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
", options: Options.AddAllowedCompilerDiagnosticId("CS1998"), equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAsyncAwait)]
        public async Task Test_ParenthesizedLambda_ExpressionBody()
        {
            await VerifyRefactoringAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = [||]async (f) => await GetAsync();

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

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAsyncAwait)]
        public async Task Test_AnonymousMethod()
        {
            await VerifyRefactoringAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    Task<object> GetAsync()
    {
        Func<object, Task<object>> func = [||]async delegate (object f)
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
", options: Options.AddAllowedCompilerDiagnosticId("CS1998"), equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAsyncAwait)]
        public async Task Test_IfElseIfReturn()
        {
            await VerifyRefactoringAsync(@"
using System.Threading.Tasks;

class C
{
    [||]async Task<object> GetAsync()
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

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAsyncAwait)]
        public async Task Test_IfElse()
        {
            await VerifyRefactoringAsync(@"
using System.Threading.Tasks;

class C
{
    [||]async Task<object> GetAsync()
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

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAsyncAwait)]
        public async Task Test_SwitchWithoutDefaultSection()
        {
            await VerifyRefactoringAsync(@"
using System.Threading.Tasks;

class C
{
    [||]async Task<object> GetAsync()
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
        }

        return GetAsync();
    }
}
");
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAsyncAwait)]
        public async Task Test_SwitchWithDefaultSection()
        {
            await VerifyRefactoringAsync(@"
using System.Threading.Tasks;

class C
{
    [||]async Task<object> GetAsync()
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
    }
}
