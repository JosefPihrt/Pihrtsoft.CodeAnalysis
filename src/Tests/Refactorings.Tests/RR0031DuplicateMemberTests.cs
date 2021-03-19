// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0031DuplicateMemberTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.DuplicateMember;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.DuplicateMember)]
        public async Task Test_Class()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        C c = default(C);
    }
[||]}

class C2
{
    void M()
    {
        C c = default(C);
    }
}
", @"
class C
{
    void M()
    {
        C c = default(C);
    }
}

class C3
{
    void M()
    {
        C3 c = default(C3);
    }
}

class C2
{
    void M()
    {
        C c = default(C);
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.DuplicateMember)]
        public async Task Test_Constructor()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public C()
    {
    [||]}
}
", @"
class C
{
    public C()
    {
    }

    public C()
    {
    }
}
", equivalenceKey: RefactoringId, options: Options.AddAllowedCompilerDiagnosticId("CS0111"));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.DuplicateMember)]
        public async Task Test_Indexer()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
    }

    public string this[int index]
    [||]{
        get { return null; }
        set { }
    }
}
", @"
class C
{
    void M()
    {
    }

    public string this[int index]
    {
        get { return null; }
        set { }
    }

    public string this[int index]
    {
        get { return null; }
        set { }
    }
}
", equivalenceKey: RefactoringId, options: Options.AddAllowedCompilerDiagnosticId("CS0111"));
        }
    }
}
