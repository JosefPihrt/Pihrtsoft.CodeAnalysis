// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0507CannotChangeAccessModifiersWhenOverridingInheritedMemberTests : AbstractCSharpCompilerDiagnosticFixVerifier<ChangeOverridingMemberAccessibilityCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.CannotChangeAccessModifiersWhenOverridingInheritedMember;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.CannotChangeAccessModifiersWhenOverridingInheritedMember)]
        public async Task Test_AccessorWithPublicAccessibility()
        {
            await VerifyFixAsync(@"
class B
{
    public virtual object P { get; set; }
}

class C : B
{
    public override object P { get; private set; }
}
", @"
class B
{
    public virtual object P { get; set; }
}

class C : B
{
    public override object P { get; set; }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
