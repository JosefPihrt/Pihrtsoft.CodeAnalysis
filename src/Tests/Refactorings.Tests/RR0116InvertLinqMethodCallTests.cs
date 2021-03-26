// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0116InvertLinqMethodCallTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.InvertLinqMethodCall;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertLinqMethodCall)]
        public async Task Test_InvertAny()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        if (items.[||]Any(f => f.Contains("""")))
        {
        }
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        if (items.All(f => !f.Contains("""")))
        {
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertLinqMethodCall)]
        public async Task Test_InvertAny_ParenthesizedLambda()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        if (items.[||]Any((f) => f.Contains("""")))
        {
        }
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        if (items.All((f) => !f.Contains("""")))
        {
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertLinqMethodCall)]
        public async Task Test_InvertAll()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        if (items.[||]All(f => f.Contains("""")))
        {
        }
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        if (items.Any(f => !f.Contains("""")))
        {
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InvertLinqMethodCall)]
        public async Task Test_InvertAll_ParenthesizedLambda()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        if (items.[||]All((f) => f.Contains("""")))
        {
        }
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        if (items.Any((f) => !f.Contains("""")))
        {
        }
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
