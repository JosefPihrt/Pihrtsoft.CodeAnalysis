// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0062InlineMethodTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.InlineMethod;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineMethod)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
namespace N
{
    class C
    {
        void M()
        {
            [||]M2();
        }

        void M2()
        {
            var x = typeof(N.B);
        }

        object B => null;
    }

    static class B
    {
    }
}", @"
namespace N
{
    class C
    {
        void M()
        {
            var x = typeof(N.B);
        }

        void M2()
        {
            var x = typeof(N.B);
        }

        object B => null;
    }

    static class B
    {
    }
}", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineMethod)]
        public async Task Test_ReplaceTypeArgumentWithPredefinedType()
        {
            await VerifyRefactoringAsync(@"
namespace N
{
    class C
    {
        void M()
        {
            new object().[||]EM();
        }
    }

    class C<T>
    {
    }

    static class E
    {
        public static C<T> EM<T>(this T _) => new C<T>();
    }
}
", @"
namespace N
{
    class C
    {
        void M()
        {
            new C<object>();
        }
    }

    class C<T>
    {
    }

    static class E
    {
        public static C<T> EM<T>(this T _) => new C<T>();
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineMethod)]
        public async Task Test_ReplaceTypeArgumentWithQualifiedName()
        {
            await VerifyRefactoringAsync(@"
namespace N
{
    class C
    {
        void M()
        {
            new N2.C().[||]EM();
        }
    }

    class C<T>
    {
        public C(T parameter)
        {
        }
    }

    static class E
    {
        public static C<T> EM<T>(this T t) => new C<T>(t);
    }
}

namespace N2
{
    class C
    {
    }
}
", @"
namespace N
{
    class C
    {
        void M()
        {
            new C<N2.C>(new N2.C());
        }
    }

    class C<T>
    {
        public C(T parameter)
        {
        }
    }

    static class E
    {
        public static C<T> EM<T>(this T t) => new C<T>(t);
    }
}

namespace N2
{
    class C
    {
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InlineMethod)]
        public async Task TestNoRefactoring()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M()
    {
        var c = this;

        C a = c?.[||]A();
    }

    C A()
    {
        return B;
    }

    C B
    {
        get { return null; }
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
