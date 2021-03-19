// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0213AddParameterToInterfaceMemberTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.AddParameterToInterfaceMember;

        public override CSharpTestOptions Options
        {
            get { return base.Options.AddAllowedCompilerDiagnosticIds(new[] { "CS0535", "CS0539" }); }
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddParameterToInterfaceMember)]
        public async Task Test_Method()
        {
            await VerifyRefactoringAsync(@"
interface IFoo
{
    void M(object p);
}

class C : IFoo
{
    public void [||]M(object p, object p2)
    {
    }
}
", @"
interface IFoo
{
    void M(object p, object p2);
}

class C : IFoo
{
    public void M(object p, object p2)
    {
    }
}
", equivalenceKey: EquivalenceKey.Join(RefactoringId, "M:IFoo.M(System.Object)"));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddParameterToInterfaceMember)]
        public async Task Test_Method_OutParameter()
        {
            await VerifyRefactoringAsync(@"
interface IFoo
{
    void M(object p);
}

class C : IFoo
{
    public void [||]M(object p, out object p2)
    {
        p2 = null;
    }
}
", @"
interface IFoo
{
    void M(object p, out object p2);
}

class C : IFoo
{
    public void M(object p, out object p2)
    {
        p2 = null;
    }
}
", equivalenceKey: EquivalenceKey.Join(RefactoringId, "M:IFoo.M(System.Object)"));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddParameterToInterfaceMember)]
        public async Task Test_Method_Parameter_WithDefaultValue()
        {
            await VerifyRefactoringAsync(@"
interface IFoo
{
    void M(object p);
}

class C : IFoo
{
    public void [||]M(object p, int p2 = 1)
    {
        p2 = 0;
    }
}
", @"
interface IFoo
{
    void M(object p, int p2 = 1);
}

class C : IFoo
{
    public void M(object p, int p2 = 1)
    {
        p2 = 0;
    }
}
", equivalenceKey: EquivalenceKey.Join(RefactoringId, "M:IFoo.M(System.Object)"));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddParameterToInterfaceMember)]
        public async Task Test_Method_Generic()
        {
            await VerifyRefactoringAsync(@"
interface IFoo<T>
{
    void M(T p);
}

class C : IFoo<string>
{
    public void [||]M(string p, object p2)
    {
    }
}
", @"
interface IFoo<T>
{
    void M(T p, object p2);
}

class C : IFoo<string>
{
    public void M(string p, object p2)
    {
    }
}
", equivalenceKey: EquivalenceKey.Join(RefactoringId, "M:IFoo`1.M(`0)"));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddParameterToInterfaceMember)]
        public async Task Test_Indexer()
        {
            await VerifyRefactoringAsync(@"
interface IFoo
{
    object this[object p] { get; }
}

class C : IFoo
{
    public object [||]this[object p, object p2] => null;
}
", @"
interface IFoo
{
    object this[object p, object p2] { get; }
}

class C : IFoo
{
    public object this[object p, object p2] => null;
}
", equivalenceKey: EquivalenceKey.Join(RefactoringId, "P:IFoo.Item(System.Object)"));
        }
    }
}
