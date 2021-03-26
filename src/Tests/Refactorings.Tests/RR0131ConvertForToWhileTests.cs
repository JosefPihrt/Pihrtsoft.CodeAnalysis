// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0131ConvertForToWhileTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ConvertForToWhile;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertForToWhile)]
        public async Task Test_CommonFor()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        [||]for (int i = 0; i < items.Count; i++)
        {
            items[i] = null;
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
        while (i < items.Count)
        {
            items[i] = null;
            i++;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertForToWhile)]
        public async Task Test_ForWithEmbeddedStatement()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        [||]for (int i = 0; i < items.Count; i++)
            items[i] = null;
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
        while (i < items.Count)
        {
            items[i] = null;
            i++;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertForToWhile)]
        public async Task Test_ForWithContinue()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f = false;
        var items = new List<string>();

        [||]for (int i = 0; i < items.Count; i++, i++)
        {
            if (f)
            {
                foreach (var item in items)
                {
                    continue;
                }

                continue;
            }

            if (f)
                continue;

            items[i] = null;
        }
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f = false;
        var items = new List<string>();

        int i = 0;
        while (i < items.Count)
        {
            if (f)
            {
                foreach (var item in items)
                {
                    continue;
                }
                i++;
                i++;
                continue;
            }

            if (f)
            {
                i++;
                i++;
                continue;
            }

            items[i] = null;
            i++;
            i++;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertForToWhile)]
        public async Task Test_ForWithMultipleDeclarationsAndIncrementors()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        string x = null;
        string y = null;

        [||]for (x = """", y = """"; x != null; x = x.ToString(), y = y.ToString())
        {
            x = x.ToString();
        }
    }
}
", @"
class C
{
    void M()
    {
        string x = null;
        string y = null;

        x = """";
        y = """";
        while (x != null)
        {
            x = x.ToString();
            x = x.ToString();
            y = y.ToString();
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertForToWhile)]
        public async Task Test_EmptyFor()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        [||]for (; ; )
        {
        }
    }
}
", @"
class C
{
    void M()
    {
        while (true)
        {
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertForToWhile)]
        public async Task Test_EmbeddedFor()
        {
            await VerifyRefactoringAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        while (true)
            [||]for (int i = 0; i < items.Count; i++)
            {
                items[i] = null;
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

        while (true)
        {
            int i = 0;
            while (i < items.Count)
            {
                items[i] = null;
                i++;
            }
        }
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
