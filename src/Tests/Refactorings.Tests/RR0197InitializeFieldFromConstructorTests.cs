// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0197InitializeFieldFromConstructorTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.InitializeFieldFromConstructor;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.InitializeFieldFromConstructor)]
        public async Task TestRefactoring()
        {
            await VerifyRefactoringAsync(@"
class Foo : FooBase
{
[|    private string bar;

    private string _bar2, _bar3;|]

    public Foo()
    {
    }

    public Foo(object parameter)
    {
        M(parameter);
    }

    public Foo(object parameter1, object parameter2)
        : this(parameter1, parameter2, null)
    {
        M(bar);
    }

    public Foo(object parameter1, object parameter2, object bar)
        : base(parameter1, parameter2)
    {
        M(bar);
    }

    public void M(object parameter) { }
}

class FooBase
{
    public FooBase() { }
    public FooBase(object parameter1, object bar) { }
}
", @"
class Foo : FooBase
{
    private string bar;

    private string _bar2, _bar3;

    public Foo(string bar, string bar2, string bar3)
    {
        this.bar = bar;
        _bar2 = bar2;
        _bar3 = bar3;
    }

    public Foo(object parameter, string bar, string bar2, string bar3)
    {
        M(parameter);
        this.bar = bar;
        _bar2 = bar2;
        _bar3 = bar3;
    }

    public Foo(object parameter1, object parameter2, string bar, string bar2, string bar3)
        : this(parameter1, parameter2, null, bar, bar2, bar3)
    {
        M(bar);
        this.bar = bar;
        _bar2 = bar2;
        _bar3 = bar3;
    }

    public Foo(object parameter1, object parameter2, object bar, string bar2, string bar22, string bar3)
        : base(parameter1, parameter2)
    {
        M(bar);
        this.bar = bar2;
        _bar2 = bar22;
        _bar3 = bar3;
    }

    public void M(object parameter) { }
}

class FooBase
{
    public FooBase() { }
    public FooBase(object parameter1, object bar) { }
}
", equivalenceKey: RefactoringId);
        }
    }
}
