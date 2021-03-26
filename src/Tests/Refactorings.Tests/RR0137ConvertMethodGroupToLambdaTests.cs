// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0137ConvertMethodGroupToLambdaTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ConvertMethodGroupToLambda;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_VariableDeclaration_Action()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M()
    {
        Action action = [||]M;
    }
}
", @"
using System;

class C
{
    void M()
    {
        Action action = () => M();
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_VariableDeclaration_Action1()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M(string s)
    {
        Action<string> action = [||]M;
    }
}
", @"
using System;

class C
{
    void M(string s)
    {
        Action<string> action = f => M(f);
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_VariableDeclaration_Action2()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M(string s1, string s2)
    {
        Action<string, string> action = [||]M;
    }
}
", @"
using System;

class C
{
    void M(string s1, string s2)
    {
        Action<string, string> action = (f, f2) => M(f, f2);
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_VariableDeclaration_Func()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    string M()
    {
        Func<string> func = [||]M;
        return null;
    }
}
", @"
using System;

class C
{
    string M()
    {
        Func<string> func = () => M();
        return null;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_VariableDeclaration_Func2()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    string M(string s)
    {
        Func<string, string> func = [||]M;
        return null;
    }
}
", @"
using System;

class C
{
    string M(string s)
    {
        Func<string, string> func = f => M(f);
        return null;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_VariableDeclaration_Func3()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    string M(string s1, string s2)
    {
        Func<string, string, string> func = [||]M;
        return null;
    }
}
", @"
using System;

class C
{
    string M(string s1, string s2)
    {
        Func<string, string, string> func = (f, f2) => M(f, f2);
        return null;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_SimpleAssignment_Action()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M()
    {
        Action action = null;

        action = [||]M;
    }
}
", @"
using System;

class C
{
    void M()
    {
        Action action = null;

        action = () => M();
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_SimpleAssignment_Action1()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M(string s)
    {
        Action<string> action = null;

        action = [||]M;
    }
}
", @"
using System;

class C
{
    void M(string s)
    {
        Action<string> action = null;

        action = f => M(f);
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_SimpleAssignment_Action2()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M(string s1, string s2)
    {
        Action<string, string> action = null;

        action = [||]M;
    }
}
", @"
using System;

class C
{
    void M(string s1, string s2)
    {
        Action<string, string> action = null;

        action = (f, f2) => M(f, f2);
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_SimpleAssignment_Func()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    string M()
    {
        Func<string> func = null;

        func = [||]M;

        return null;
    }
}
", @"
using System;

class C
{
    string M()
    {
        Func<string> func = null;

        func = () => M();

        return null;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_SimpleAssignment_Func1()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    string M(string s)
    {
        Func<string, string> func = null;

        func = [||]M;

        return null;
    }
}
", @"
using System;

class C
{
    string M(string s)
    {
        Func<string, string> func = null;

        func = f => M(f);

        return null;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_SimpleAssignment_Func2()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    string M(string s1, string s2)
    {
        Func<string, string, string> func = null;

        func = [||]M;

        return null;
    }
}
", @"
using System;

class C
{
    string M(string s1, string s2)
    {
        Func<string, string, string> func = null;

        func = (f, f2) => M(f, f2);

        return null;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_Argument_Action()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M2(Action action)
    {
        M2([||]M);
    }

    void M() { }
}
", @"
using System;

class C
{
    void M2(Action action)
    {
        M2(() => M());
    }

    void M() { }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_Argument_Action1()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M(Action<string> action)
    {
        M([||]M2);
    }

    void M2(string s) { }
}
", @"
using System;

class C
{
    void M(Action<string> action)
    {
        M(f => M2(f));
    }

    void M2(string s) { }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_Argument_Action2()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M(Action<string, string> action)
    {
        M([||]M2);
    }

    void M2(string s1, string s2) { }
}
", @"
using System;

class C
{
    void M(Action<string, string> action)
    {
        M((f, f2) => M2(f, f2));
    }

    void M2(string s1, string s2) { }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_Argument_Func()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M(Func<string> func)
    {
        M([||]M2);
    }

    string M2() => null;
}
", @"
using System;

class C
{
    void M(Func<string> func)
    {
        M(() => M2());
    }

    string M2() => null;
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_Argument_Func1()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M(Func<string, string> func)
    {
        M([||]M2);
    }

    string M2(string s) => null;
}
", @"
using System;

class C
{
    void M(Func<string, string> func)
    {
        M(f => M2(f));
    }

    string M2(string s) => null;
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertMethodGroupToLambda)]
        public async Task Test_Argument_Func2()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M(Func<string, string, string> func)
    {
        M([||]M2);
    }

    string M2(string s1, string s2) => null;
}
", @"
using System;

class C
{
    void M(Func<string, string, string> func)
    {
        M((f, f2) => M2(f, f2));
    }

    string M2(string s1, string s2) => null;
}
", equivalenceKey: RefactoringId);
        }
    }
}
