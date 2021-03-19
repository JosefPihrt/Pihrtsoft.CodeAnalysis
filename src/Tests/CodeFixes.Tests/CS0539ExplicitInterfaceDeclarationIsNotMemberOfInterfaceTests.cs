// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0539ExplicitInterfaceDeclarationIsNotMemberOfInterfaceTests : AbstractCSharpCompilerDiagnosticFixVerifier<MemberDeclarationCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.ExplicitInterfaceDeclarationIsNotMemberOfInterface;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.ExplicitInterfaceDeclarationIsNotMemberOfInterface)]
        public async Task Test_Method_ExplicitlyImplemented()
        {
            await VerifyFixAsync(@"
interface IFoo
{
    void M(object p);
}

class C : IFoo
{
    void IFoo.M(object p, object p2)
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
    void IFoo.M(object p, object p2)
    {
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, "M:IFoo.M(System.Object)"));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.ExplicitInterfaceDeclarationIsNotMemberOfInterface)]
        public async Task Test_Indexer_ExplicitlyImplemented()
        {
            await VerifyFixAsync(@"
interface IFoo
{
    object this[object p] { get; }
}

class C : IFoo
{
    object IFoo.this[object p, object p2] => null;
}
", @"
interface IFoo
{
    object this[object p, object p2] { get; }
}

class C : IFoo
{
    object IFoo.this[object p, object p2] => null;
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, "P:IFoo.Item(System.Object)"));
        }
    }
}
