// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0549NewVirtualMemberInSealedClassTests : AbstractCSharpCompilerDiagnosticFixVerifier<ModifiersCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.NewVirtualMemberInSealedClass;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.NewVirtualMemberInSealedClass)]
        public async Task Test_ReadOnlyPropertyInSealedClass()
        {
            await VerifyFixAsync(@"
sealed class C
{
    public virtual object P
    {
        get { return null; }
    }
}
", @"
sealed class C
{
    public object P
    {
        get { return null; }
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.RemoveVirtualModifier));
        }
    }
}
