// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0130ConvertForToForEachTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ConvertForToForEach;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertForToForEach)]
        public async Task Test_UseExplicitType()
        {
            await VerifyRefactoringAsync(@"
using System;
using System.Collections;

class C
{
    void M(RowCollection rows, string columnName)
    {
        [||]for (int i = 0; i < rows.Count; i++)
        {
            string s = rows[i][columnName].ToString();
        }
    }
}

abstract class Row
{
    public abstract object this[string columnName] { get; }
}

abstract class RowCollection : ICollection
{
    public abstract Row this[int index] { get; }

    public abstract int Count { get; }
    public abstract bool IsSynchronized { get; }
    public abstract object SyncRoot { get; }

    public abstract void CopyTo(Array array, int index);
    public abstract IEnumerator GetEnumerator();
}
", @"
using System;
using System.Collections;

class C
{
    void M(RowCollection rows, string columnName)
    {
        foreach (Row item in rows)
        {
            string s = item[columnName].ToString();
        }
    }
}

abstract class Row
{
    public abstract object this[string columnName] { get; }
}

abstract class RowCollection : ICollection
{
    public abstract Row this[int index] { get; }

    public abstract int Count { get; }
    public abstract bool IsSynchronized { get; }
    public abstract object SyncRoot { get; }

    public abstract void CopyTo(Array array, int index);
    public abstract IEnumerator GetEnumerator();
}
", equivalenceKey: RefactoringId);
        }
    }
}
