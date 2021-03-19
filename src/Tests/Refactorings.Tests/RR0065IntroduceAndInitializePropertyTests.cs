// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0065IntroduceAndInitializePropertyTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.IntroduceAndInitializeProperty;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.IntroduceAndInitializeProperty)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public C(string [||]p = null)
    {
    }
}
", @"
class C
{
    public C(string p = null)
    {
        P = p;
    }

    public string P { get; }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.IntroduceAndInitializeProperty)]
        public async Task Test_MultipleParameters()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    public C([|object p1, string p2|], int p3)
    {
        if (p1 == null)
            throw new ArgumentNullException(nameof(p1));
    }
}
", @"
using System;

class C
{
    public C(object p1, string p2, int p3)
    {
        if (p1 == null)
            throw new ArgumentNullException(nameof(p1));
        P1 = p1;
        P2 = p2;
    }

    public object P1 { get; }
    public string P2 { get; }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.IntroduceAndInitializeProperty)]
        public async Task TestNoRefactoring_ParameterPassedToInitializer()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    public C(object [||]p1, object p2)
        : this(p1)
    {
    }

    public C(object p1)
    {
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
