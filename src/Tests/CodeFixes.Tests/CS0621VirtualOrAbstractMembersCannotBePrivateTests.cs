// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0621VirtualOrAbstractMembersCannotBePrivateTests : AbstractCSharpCompilerDiagnosticFixVerifier<ModifiersCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.VirtualOrAbstractMembersCannotBePrivate;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.VirtualOrAbstractMembersCannotBePrivate)]
        public async Task Test_ChangeAccessibilityToPublic()
        {
            await VerifyFixAsync(@"
class C
{
    private virtual void M()
    {
    }
}
", @"
class C
{
    public virtual void M()
    {
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, nameof(AccessibilityFilter.Public)));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.VirtualOrAbstractMembersCannotBePrivate)]
        public async Task Test_RemoveVirtualModifier()
        {
            await VerifyFixAsync(@"
class C
{
    private virtual void M()
    {
    }
}
", @"
class C
{
    private void M()
    {
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, nameof(SyntaxKind.VirtualKeyword)));
        }
    }
}
