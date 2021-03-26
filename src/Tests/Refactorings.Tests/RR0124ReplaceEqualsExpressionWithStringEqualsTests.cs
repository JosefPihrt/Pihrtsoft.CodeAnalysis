// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0124ReplaceEqualsExpressionWithStringEqualsTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ReplaceEqualsExpressionWithStringEquals;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ReplaceEqualsExpressionWithStringEquals)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M()
    {
        string s1 = null;
        string s2 = null;

        if (s1 =[||]= s2) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        string s1 = null;
        string s2 = null;

        if (string.Equals(s1, s2, StringComparison.CurrentCulture)) { }
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
