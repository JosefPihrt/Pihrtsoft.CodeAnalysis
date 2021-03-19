// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0177OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethodTests : AbstractCSharpCompilerDiagnosticFixVerifier<AssignDefaultValueToOutParameterCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod)]
        public async Task Test_VoidMethodWithTwoOutParameters()
        {
            await VerifyFixAsync(@"
class C
{
    void M(object p1, out object p2, out object p3)
    {
    }
}
", @"
class C
{
    void M(object p1, out object p2, out object p3)
    {
        p2 = null;
        p3 = null;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod)]
        public async Task Test_VoidMethodWithStatement()
        {
            await VerifyFixAsync(@"
class C
{
    void M(object p1, out object p2, out object p3)
    {
        p1 = null;
    }
}
", @"
class C
{
    void M(object p1, out object p2, out object p3)
    {
        p1 = null;
        p2 = null;
        p3 = null;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod)]
        public async Task Test_BoolMethodWithReturnStatement()
        {
            await VerifyFixAsync(@"
class C
{
    bool M(object p1, out object p2, out object p3)
    {
        return false;
    }
}
", @"
class C
{
    bool M(object p1, out object p2, out object p3)
    {
        p2 = null;
        p3 = null;
        return false;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod)]
        public async Task Test_BoolMethodWithStatements()
        {
            await VerifyFixAsync(@"
class C
{
    bool M(object p1, out object p2, out object p3)
    {
        p1 = null;
        p2 = null;
        return false;
    }
}
", @"
class C
{
    bool M(object p1, out object p2, out object p3)
    {
        p1 = null;
        p2 = null;
        p3 = null;
        return false;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod)]
        public async Task Test_BoolMethodWithReturnStatements()
        {
            await VerifyFixAsync(@"
class C
{
    bool M(bool f, out object p2, out object p3)
    {
        if (f)
            return false;

        return false;
    }
}
", @"
class C
{
    bool M(bool f, out object p2, out object p3)
    {
        if (f)
        {
            p2 = null;
            p3 = null;
            return false;
        }

        p2 = null;
        p3 = null;
        return false;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod)]
        public async Task Test_MethodWithExpressionBody()
        {
            await VerifyFixAsync(@"
class C
{
    object M(object p1, out object p2, out object p3, out object p4) => p1 = p2 = null;
}
", @"
class C
{
    object M(object p1, out object p2, out object p3, out object p4)
    {
        p3 = null;
        p4 = null;
        return p1 = p2 = null;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        // Flow analysis APIs do not work with local functions: https://github.com/dotnet/roslyn/issues/14214
#pragma warning disable xUnit1013
        //[Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod)]
        public async Task Test_VoidLocalFunctionWithTwoOutParameters()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        void LF(object p1, out object p2, out object p3)
        {
        }
    }
}
", @"
class C
{
    void M()
    {
        void LF(object p1, out object p2, out object p3)
        {
            p2 = null;
            p3 = null;
        }
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        //[Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod)]
        public async Task Test_VoidLocalFunctionWithStatement()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        void LF(object p1, out object p2, out object p3)
        {
            p1 = null;
        }
    }
}
", @"
class C
{
    void M()
    {
        void LF(object p1, out object p2, out object p3)
        {
            p1 = null;
            p2 = null;
            p3 = null;
        }
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        //[Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod)]
        public async Task Test_BoolLocalFunctionWithReturnStatement()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        bool LF(object p1, out object p2, out object p3)
        {
            return false;
        }
    }
}
", @"
class C
{
    void M()
    {
        bool LF(object p1, out object p2, out object p3)
        {
            p2 = null;
            p3 = null;
            return false;
        }
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        //[Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod)]
        public async Task Test_BoolLocalFunctionWithStatements()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        bool LF(object p1, out object p2, out object p3)
        {
            p1 = null;
            p2 = null;
            return false;
        }
    }
}
", @"
class C
{
    void M()
    {
        bool LF(object p1, out object p2, out object p3)
        {
            p1 = null;
            p2 = null;
            p3 = null;
            return false;
        }
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        //[Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod)]
        public async Task Test_BoolLocalFunctionWithReturnStatements()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        bool LF(bool f, out object p2, out object p3)
        {
            if (f)
                return false;

            return false;
        }
    }
}
", @"
class C
{
    void M()
    {
        bool LF(bool f, out object p2, out object p3)
        {
            if (f)
            {
                p2 = null;
                p3 = null;
                return false;
            }

            p2 = null;
            p3 = null;
            return false;
        }
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        //[Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod)]
        public async Task Test_LocalFunctionWithExpressionBody()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        object LF(object p1, out object p2, out object p3, out object p4) => p1 = p2 = null;
    }
}
", @"
class C
{
    void M()
    {
        object LF(object p1, out object p2, out object p3, out object p4)
        {
            p3 = null;
            p4 = null;
            return p1 = p2 = null;
        }
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
#pragma warning restore xUnit1013

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod)]
        public async Task TestNoFix_MethodWithoutBody()
        {
            await VerifyNoFixAsync(@"
class C
{
    void M(object p1, out object p2, out object p3)
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OutParameterMustBeAssignedToBeforeControlLeavesCurrentMethod)]
        public async Task TestNoFix_LocalFunctionWithoutBody()
        {
            await VerifyNoFixAsync(@"
class C
{
    void M()
    {
        void LF(object p1, out object p2, out object p3)
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
