// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0025CheckParameterForNullTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.CheckParameterForNull;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.CheckParameterForNull)]
        public async Task Test_ReferenceType()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M(object [||]p)
    {
    }
}
", @"
using System;

class C
{
    void M(object p)
    {
        if (p == null)
            throw new ArgumentNullException(nameof(p));
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.CheckParameterForNull)]
        public async Task Test_NullableType()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M(int? [||]p)
    {
    }
}
", @"
using System;

class C
{
    void M(int? p)
    {
        if (p == null)
            throw new ArgumentNullException(nameof(p));
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.CheckParameterForNull)]
        public async Task Test_MultipleParametersSelected_OneNullCheckAdded()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M([|object p, object p2|])
    {
        if (p == null)
            throw new ArgumentNullException(nameof(p));
    }
}
", @"
using System;

class C
{
    void M(object p, object p2)
    {
        if (p == null)
            throw new ArgumentNullException(nameof(p));

        if (p2 == null)
            throw new ArgumentNullException(nameof(p2));
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.CheckParameterForNull)]
        public async Task Test_MultipleParametersSelected_TwoNullChecksAdded()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M(object p, [|object p2, object p3, object p4, int? pi, int i, object p5 = null, object p6 = default, object p7 = default(object)|])
    {
        if (p2 == null)
            throw new ArgumentNullException(nameof(p2));

        if (p3 is null)
            throw new ArgumentNullException(nameof(p3));
    }
}
", @"
using System;

class C
{
    void M(object p, object p2, object p3, object p4, int? pi, int i, object p5 = null, object p6 = default, object p7 = default(object))
    {
        if (p2 == null)
            throw new ArgumentNullException(nameof(p2));

        if (p3 is null)
            throw new ArgumentNullException(nameof(p3));

        if (p4 == null)
            throw new ArgumentNullException(nameof(p4));

        if (pi == null)
            throw new ArgumentNullException(nameof(pi));
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.CheckParameterForNull)]
        public async Task TestNoRefactoring_NullCheckAlreadyExists()
        {
            await VerifyNoRefactoringAsync(@"
using System;

class C
{
    void M(object [||]p)
    {
        if (p == null)
            throw new ArgumentNullException(nameof(p));
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.CheckParameterForNull)]
        public async Task TestNoRefactoring_NullCheckAlreadyExists_MultipleParametersSelected()
        {
            await VerifyNoRefactoringAsync(@"
using System;

class C
{
    void M([|object p, object p2|])
    {
        if (p == null)
            throw new ArgumentNullException(nameof(p));

        if (p2 == null)
            throw new ArgumentNullException(nameof(p2));
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.CheckParameterForNull)]
        public async Task TestNoRefactoring_ValueType()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M(int [||]p)
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.CheckParameterForNull)]
        public async Task TestNoRefactoring_NullLiteral()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M(object [||]p = null)
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.CheckParameterForNull)]
        public async Task TestNoRefactoring_DefaultLiteral()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M(object [||]p = default)
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.CheckParameterForNull)]
        public async Task TestNoRefactoring_DefaultExpression()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M(object [||]p = default(object))
    {
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
