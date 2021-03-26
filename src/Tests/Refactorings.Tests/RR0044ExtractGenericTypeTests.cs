// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0044ExtractGenericTypeTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ExtractGenericType;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ExtractGenericType)]
        public async Task Test_Parameter()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M(IEnumerable<[|string|]> p)
    {
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M(string p)
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ExtractGenericType)]
        public async Task TestNoRefactoring()
        {
            await VerifyNoRefactoringAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M(IEnumerable<string> p)
    {
        var x = p.ToList<[|string|]>();
    }
    }
", equivalenceKey: RefactoringId);
        }
    }
}
