// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0090RemoveAllPreprocessorDirectivesTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.RemoveAllPreprocessorDirectives;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.RemoveAllPreprocessorDirectives)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
#nullable disable

class C
{
    #[||]region Methods
    public void M()
    {
#if DEBUG
        string s = ""DEBUG"";
#endif
    }
    #endregion

#pragma warning disable 1
#pragma warning restore 1

    #region Properties
    public string P { get; set; }
    #endregion
}
", @"

class C
{
    public void M()
    {
        string s = ""DEBUG"";
    }


    public string P { get; set; }
}
", equivalenceKey: RefactoringId);
        }
    }
}
