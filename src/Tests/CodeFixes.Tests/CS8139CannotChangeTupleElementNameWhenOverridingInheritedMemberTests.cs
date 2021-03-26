// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS8139CannotChangeTupleElementNameWhenOverridingInheritedMemberTests : AbstractCSharpCompilerDiagnosticFixVerifier<MemberDeclarationCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.CannotChangeTupleElementNameWhenOverridingInheritedMember;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.CannotChangeTupleElementNameWhenOverridingInheritedMember)]
        public async Task Test_Method()
        {
            await VerifyFixAsync(@"
class C : B
{
    public override (string x, string yy, string z) M()
    {
        return (x: null, yy: null, z: null);
    }
}

class B
{
    public virtual (string x, string y, string z) M()
    {
        return default;
    }
}
", @"
class C : B
{
    public override (string x, string y, string z) M()
    {
        return (x: null, y: null, z: null);
    }
}

class B
{
    public virtual (string x, string y, string z) M()
    {
        return default;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.CannotChangeTupleElementNameWhenOverridingInheritedMember)]
        public async Task Test_Property()
        {
            await VerifyFixAsync(@"
class C : B
{
    public override (string x, string yy, string z) P
    {
        get { return (x: null, yy: null, z: null); }
    }
}

class B
{
    public virtual (string x, string y, string z) P
    {
        get { return default; }
    }
}", @"
class C : B
{
    public override (string x, string y, string z) P
    {
        get { return (x: null, y: null, z: null); }
    }
}

class B
{
    public virtual (string x, string y, string z) P
    {
        get { return default; }
    }
}", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
