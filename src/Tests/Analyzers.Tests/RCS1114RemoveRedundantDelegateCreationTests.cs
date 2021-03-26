// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1114RemoveRedundantDelegateCreationTests : AbstractCSharpDiagnosticVerifier<RemoveRedundantDelegateCreationAnalyzer, AssignmentExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.RemoveRedundantDelegateCreation;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantDelegateCreation)]
        public async Task Test_EventHandler()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class Foo
{
    void M()
    {
        Changed += [|new EventHandler(Foo_Changed)|];
        Changed -= [|new EventHandler(Foo_Changed)|];
    }

    protected virtual void Foo_Changed(object sender, EventArgs e) { }

    public event EventHandler Changed;
}
", @"
using System;

class Foo
{
    void M()
    {
        Changed += Foo_Changed;
        Changed -= Foo_Changed;
    }

    protected virtual void Foo_Changed(object sender, EventArgs e) { }

    public event EventHandler Changed;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantDelegateCreation)]
        public async Task Test_EventHandlerOfT()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class Foo
{
    void M()
    {
        Changed += [|new EventHandler<FooEventArgs>(Foo_Changed)|];
        Changed -= [|new EventHandler<FooEventArgs>(Foo_Changed)|];
    }

    protected virtual void Foo_Changed(object sender, FooEventArgs e) { }

    public event EventHandler<FooEventArgs> Changed;

    public class FooEventArgs : EventArgs
    {
    }
}
", @"
using System;

class Foo
{
    void M()
    {
        Changed += Foo_Changed;
        Changed -= Foo_Changed;
    }

    protected virtual void Foo_Changed(object sender, FooEventArgs e) { }

    public event EventHandler<FooEventArgs> Changed;

    public class FooEventArgs : EventArgs
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantDelegateCreation)]
        public async Task Test_CustomEventHandler()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class Foo
{
    void M()
    {
        Changed += [|new FooEventHandler(Foo_Changed)|];
        Changed -= [|new FooEventHandler(Foo_Changed)|];
    }

    protected virtual void Foo_Changed(object sender, FooEventArgs e) { }

    public event FooEventHandler Changed;

    public delegate void FooEventHandler(object sender, FooEventArgs args);

    public class FooEventArgs : EventArgs
    {
    }
}
", @"
using System;

class Foo
{
    void M()
    {
        Changed += Foo_Changed;
        Changed -= Foo_Changed;
    }

    protected virtual void Foo_Changed(object sender, FooEventArgs e) { }

    public event FooEventHandler Changed;

    public delegate void FooEventHandler(object sender, FooEventArgs args);

    public class FooEventArgs : EventArgs
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantDelegateCreation)]
        public async Task Test_TEventArgs()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class Foo<TEventArgs>
{
    void M()
    {
        Changed += [|new EventHandler<TEventArgs>(Foo_Changed)|];
        Changed -= [|new EventHandler<TEventArgs>(Foo_Changed)|];
    }

    protected virtual void Foo_Changed(object sender, TEventArgs e) { }

    public event EventHandler<TEventArgs> Changed;
}
", @"
using System;

class Foo<TEventArgs>
{
    void M()
    {
        Changed += Foo_Changed;
        Changed -= Foo_Changed;
    }

    protected virtual void Foo_Changed(object sender, TEventArgs e) { }

    public event EventHandler<TEventArgs> Changed;
}
");
        }
    }
}
