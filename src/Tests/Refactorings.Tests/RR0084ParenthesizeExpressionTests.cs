// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0084ParenthesizeExpressionTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ParenthesizeExpression;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ParenthesizeExpression)]
        public async Task TestNoRefactoring_ReturnType_TupleType()
        {
            await VerifyNoRefactoringAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class C
{
        [|(object x, object y)|] M()
        {
            return default;
        }
}
", equivalenceKey: RefactoringId);
        }
    }
}
