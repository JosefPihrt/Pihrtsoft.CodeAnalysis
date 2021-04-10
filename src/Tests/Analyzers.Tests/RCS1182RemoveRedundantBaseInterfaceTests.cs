// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1182RemoveRedundantBaseInterfaceTests : AbstractCSharpDiagnosticVerifier<RemoveRedundantBaseInterfaceAnalyzer, BaseTypeCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.RemoveRedundantBaseInterface;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantBaseInterface)]
        public async Task Test_IEnumerableOfT()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class Foo1<T> : List<T>, [|IEnumerable<T>|] where T : class
{
}
", @"
using System.Collections.Generic;

class Foo1<T> : List<T> where T : class
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantBaseInterface)]
        public async Task TestNoDiagnostic_ExplicitImplementation()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;

class C : List<object>, ICollection<object>
{
    IEnumerator<object> IEnumerable<object>.GetEnumerator()
    {
        return null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantBaseInterface)]
        public async Task TestNoDiagnostic_MethodImplementedWithNewKeyword()
        {
            await VerifyNoDiagnosticAsync(@"
interface IFoo
{
    void Bar();
}

class B : IFoo
{
    public void Bar()
    {
    }
}

class C : B, IFoo
{
    new public void Bar()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantBaseInterface)]
        public async Task TestNoDiagnostic_PropertyImplementedWithNewKeyword()
        {
            await VerifyNoDiagnosticAsync(@"
interface IFoo
{
    object Bar { get; }
}

class B : IFoo
{
    public object Bar { get; }
}

class C : B, IFoo
{
    new public object Bar { get; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantBaseInterface)]
        public async Task TestNoDiagnostic_IndexerImplementedWithNewKeyword()
        {
            await VerifyNoDiagnosticAsync(@"
interface IFoo
{
    object this[int index] { get; }
}

class B : IFoo
{
    public object this[int index]
    {
        get => null;
    }
}

class C : B, IFoo
{
    new public object this[int index]
    {
        get => null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantBaseInterface)]
        public async Task TestNoDiagnostic_EventFieldImplementedWithNewKeyword()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

interface IFoo
{
    event EventHandler Bar;
}

class B : IFoo
{
    public event EventHandler Bar;
}

class C : B, IFoo
{
    new public event EventHandler Bar;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantBaseInterface)]
        public async Task TestNoDiagnostic_EventImplementedWithNewKeyword()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

interface IFoo
{
    event EventHandler Bar;
}

class B : IFoo
{
    public event EventHandler Bar;
}

class C : B, IFoo
{
    new public event EventHandler Bar
    {
        add { }
        remove { }
    }
}
");
        }
    }
}
