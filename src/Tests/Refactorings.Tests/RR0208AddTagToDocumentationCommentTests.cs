// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0208AddTagToDocumentationCommentTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.AddTagToDocumentationComment;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddTagToDocumentationComment)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
class C
{
    /// <summary>
    /// x [|null|] x
    /// </summary>
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// x <c>null</c> x
    /// </summary>
    void M()
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddTagToDocumentationComment)]
        public async Task Test_StartOfText()
        {
            await VerifyRefactoringAsync(@"
class C
{
    /// <summary>
    /// [|null|] x
    /// </summary>
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// <c>null</c> x
    /// </summary>
    void M()
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddTagToDocumentationComment)]
        public async Task Test_EndOfText()
        {
            await VerifyRefactoringAsync(@"
class C
{
    /// <summary>
    /// x [|null|]
    /// </summary>
    void M()
    {
    }
}
", @"
class C
{
    /// <summary>
    /// x <c>null</c>
    /// </summary>
    void M()
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddTagToDocumentationComment)]
        public async Task TestNoRefactoring_EmptySpan()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    /// <summary>
    /// x [||]null
    /// </summary>
    void M()
    {
    }
}", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.AddTagToDocumentationComment)]
        public async Task TestNoRefactoring_InvalidSpan()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    /// <summary>
    /// x [|nul|]l
    /// </summary>
    void M()
    {
    }

    /// <summary>
    /// n[|ull|] x
    /// </summary>
    void M2()
    {
    }
}", equivalenceKey: RefactoringId);
        }
    }
}
