// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0086RemoveAllCommentsTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.RemoveAllComments;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAllComments)]
        public async Task Test_SingleLineComment()
        {
            await VerifyRefactoringAsync(@"
[|class C// x
|]{
}
", @"
class C
{
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAllComments)]
        public async Task Test_SingleLineComment_LeadingWhitespace()
        {
            await VerifyRefactoringAsync(@"
[|class C // x
|]{
}
", @"
class C
{
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAllComments)]
        public async Task Test_SingleLineComment_NoTrailingEndOfLine()
        {
            await VerifyRefactoringAsync(@"
[|class C { } // x|]",
@"
class C { }", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAllComments)]
        public async Task Test_SingleLineComment_PragmaDirective()
        {
            await VerifyRefactoringAsync(@"
[|#pragma warning disable 1 // x
class C
#pragma warning restore 1 // x
|]{
}
", @"
#pragma warning disable 1
class C
#pragma warning restore 1
{
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAllComments)]
        public async Task Test_MultiLineComment()
        {
            await VerifyRefactoringAsync(@"
class C
{
[|    /* x
     */ 
|]    void M()
    {
    }
}
", @"
class C
{
    void M()
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAllComments)]
        public async Task Test_MultiLineComment_NoLeadingTrailingTrivia()
        {
            await VerifyRefactoringAsync(@"
class C
{
[|/* x
     */
|]    void M()
    {
    }
}
", @"
class C
{
    void M()
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAllComments)]
        public async Task Test_MultiLineComment_NoEndOfLine()
        {
            await VerifyRefactoringAsync(@"
class C
{
[|     /* x
     */ void M()
|]    {
    }
}
", @"
class C
{
    void M()
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAllComments)]
        public async Task Test_SingleLineDocumentationComment()
        {
            await VerifyRefactoringAsync(@"
class C
{
[|    /// <summary>
    /// x
    /// </summary>
|]    void M()
    {
    }
}
", @"
class C
{
    void M()
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAllComments)]
        public async Task Test_SingleLineDocumentationComment_NoLeadingWhitespace()
        {
            await VerifyRefactoringAsync(@"
[|/// <summary>
/// x
/// </summary>
|]class C
{
}
", @"
class C
{
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAllComments)]
        public async Task Test_MultiLineDocumentationComment()
        {
            await VerifyRefactoringAsync(@"
class C
{
[|    /** <summary>
     *  x
     *  </summary> */ 
|]    void M()
    {
    }
}
", @"
class C
{
    void M()
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAllComments)]
        public async Task Test_MultiLineDocumentationComment_NoLeadingTrivia()
        {
            await VerifyRefactoringAsync(@"
class C
{
[|/** <summary>
 *  x
 *  </summary> */
|]    void M()
    {
    }
}
", @"
class C
{
    void M()
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAllComments)]
        public async Task Test_MultiLineDocumentationComment_NoTrailingEndOfLine()
        {
            await VerifyRefactoringAsync(@"
class C
{
[|    /** <summary>
     *  x
     *  </summary> */ void M()
|]    {
    }
}
", @"
class C
{
    void M()
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAllComments)]
        public async Task Test_AllComments()
        {
            await VerifyRefactoringAsync(@"
[|/// <summary>
/// x
/// </summary>
class C// x
{ /* x
   */
    /// <summary>
    /// x
    /// </summary>
     void M() // x
    {
    }

    /** <summary>
     *  x
     *  </summary> */
     void M2() // x
    {
    }

|]    // x
}
", @"
class C
{
    void M()
    {
    }

    void M2()
    {
    }

    // x
}
", equivalenceKey: RefactoringId);
        }
    }
}
