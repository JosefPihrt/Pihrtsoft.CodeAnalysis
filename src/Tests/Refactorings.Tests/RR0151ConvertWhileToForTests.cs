// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0151ConvertWhileToForTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ConvertWhileToFor;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertWhileToFor)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        int i = 0;
        [||]while (i < items.Count)
        {
            items[i] = null;
            i++;
        }
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        for (int i = 0; i < items.Count; i++)
        {
            items[i] = null;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertWhileToFor)]
        public async Task Test2()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        string s1 = null;
        string s2 = null;
        var items = new List<string>();

        int i = 0;
        [||]while (i < items.Count)
        {
            items[i] = null;
            i++;
        }
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        string s1 = null;
        string s2 = null;
        var items = new List<string>();

        for (int i = 0; i < items.Count; i++)
        {
            items[i] = null;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertWhileToFor)]
        public async Task Test_WithContinue()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        int i = 0;
        [||]while (i < items.Count)
        {
            items[i] = null;
            i++;
            continue;
        }
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        for (int i = 0; i < items.Count; i++)
        {
            items[i] = null;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertWhileToFor)]
        public async Task Test_OnlyContinue()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        int i = 0;
        [||]while (i < items.Count)
        {
            items[i] = null;
            continue;
        }
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        for (int i = 0; i < items.Count;)
        {
            items[i] = null;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertWhileToFor)]
        public async Task Test_TwoVariables()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        int i = 0;
        int j = 0;
        [||]while (i < items.Count && j < items.Count)
        {
            items[i] = null;
            i++;
            j++;
        }
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        for (int i = 0, j = 0; i < items.Count && j < items.Count; i++, j++)
        {
            items[i] = null;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertWhileToFor)]
        public async Task Test_VariableReferencedInsideWhileStatement()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        int i = 0;
        [||]while (true)
        {
        }
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        int i = 0;
        for (; ; )
        {
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertWhileToFor)]
        public async Task Test_VariableReferencedAfterWhileStatement()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        int i = 0;
        [||]while (i < items.Count)
        {
            items[i] = null;
            i++;
        }

        i = 0;
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        int i = 0;
        for (; i < items.Count; i++)
        {
            items[i] = null;
        }

        i = 0;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertWhileToFor)]
        public async Task Test_SelectedStatements()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();
        int i = 0;

        [|M();
        M();
        while (i < items.Count)
        {
            items[i] = null;
            i++;
        }|]
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();
        int i = 0;

        for (M(), M(); i < items.Count; i++)
        {
            items[i] = null;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertWhileToFor)]
        public async Task TestNoRefactoring_SelectedStatements_MixedStatements()
        {
            await VerifyNoRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        [|int i = 0;
        M();
        while (i < items.Count)
        {
            items[i] = null;
            i++;
        }|]
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertWhileToFor)]
        public async Task TestNoRefactoring_SelectedStatements_MixedStatements2()
        {
            await VerifyNoRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        [|M();
        int i = 0;
        while (i < items.Count)
        {
            items[i] = null;
            i++;
        }|]
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertWhileToFor)]
        public async Task TestNoRefactoring_SelectedStatements_TypesAreNotEqual()
        {
            await VerifyNoRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        [|int i = 0;
        long j = 0;
        while (i < items.Count)
        {
            items[i] = null;
            i++;
        }|]
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertWhileToFor)]
        public async Task TestNoRefactoring_SelectedStatements_VariableReferencedAfterWhileStatement()
        {
            await VerifyNoRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        [|int i = 0;
        while (i < items.Count)
        {
            items[i] = null;
            i++;
        }|]

        i = 0;
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
