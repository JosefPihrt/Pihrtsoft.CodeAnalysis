// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0186ChangeAccessibilityTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ChangeAccessibility;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ChangeAccessibility)]
        public async Task Test_Method()
        {
            await VerifyRefactoringAsync(@"
class C
{
    [||]public string M() => null;
}
", @"
class C
{
    internal string M() => null;
}
", equivalenceKey: EquivalenceKey.Join(RefactoringId, nameof(Accessibility.Internal)));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ChangeAccessibility)]
        public async Task Test_OverrideMethod()
        {
            await VerifyRefactoringAsync(@"
class B
{
    public virtual string M() => null;
}
class C : B
{
    [||]public override string M() => null;
}
", @"
class B
{
    internal virtual string M() => null;
}
class C : B
{
    internal override string M() => null;
}
", equivalenceKey: EquivalenceKey.Join(RefactoringId, nameof(Accessibility.Internal)));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ChangeAccessibility)]
        public async Task Test_MultipleDeclarations()
        {
            await VerifyRefactoringAsync(@"
class C
{
[|    public override string ToString() => null;
    internal string M() => null;|]
}
", @"
class C
{
    public override string ToString() => null;
    public string M() => null;
}
", equivalenceKey: EquivalenceKey.Join(RefactoringId, nameof(Accessibility.Public)));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ChangeAccessibility)]
        public async Task Test_MultipleDeclarations_AllImplicit()
        {
            await VerifyRefactoringAsync(@"
class C
{
[|    object M1() => null;
    object M2() => null;|]
}
", @"
class C
{
    private object M1() => null;
    private object M2() => null;
}
", equivalenceKey: EquivalenceKey.Join(RefactoringId, nameof(Accessibility.Private)));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ChangeAccessibility)]
        public async Task Test_MultipleDeclarations_AnyImplicit()
        {
            await VerifyRefactoringAsync(@"
class C
{
[|    private object M1() => null;
    object M2() => null;|]
}
", @"
class C
{
    private object M1() => null;
    private object M2() => null;
}
", equivalenceKey: EquivalenceKey.Join(RefactoringId, nameof(Accessibility.Private)));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ChangeAccessibility)]
        public async Task TestNoRefactoring_OverrideDeclarationWithoutBaseSource()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    [||]public override string ToString() => null;
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ChangeAccessibility)]
        public async Task TestNoRefactoring_OverrideDeclarationsWithoutBaseSource()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
[|    public override string ToString() => null;
    public override int GetHashCode() => 0;|]
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ChangeAccessibility)]
        public async Task TestNoRefactoring_AbstractMethodToPrivate()
        {
            await VerifyNoRefactoringAsync(@"
abstract class C
{
    [||]public abstract string M();
}
", equivalenceKey: EquivalenceKey.Join(RefactoringId, nameof(Accessibility.Private)));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ChangeAccessibility)]
        public async Task TestNoRefactoring_VirtualMethodToPrivate()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    [||]public virtual string M() => null;
}
", equivalenceKey: EquivalenceKey.Join(RefactoringId, nameof(Accessibility.Private)));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ChangeAccessibility)]
        public async Task TestNoRefactoring_OverrideMethodToPrivate()
        {
            await VerifyNoRefactoringAsync(@"
class B
{
    public virtual string M() => null;
}
class C : B
{
    [||]public override string M() => null;
}", equivalenceKey: EquivalenceKey.Join(RefactoringId, nameof(Accessibility.Private)));
        }
    }
}
