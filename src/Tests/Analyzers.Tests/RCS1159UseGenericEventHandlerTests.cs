// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1159UseGenericEventHandlerTests : AbstractCSharpDiagnosticVerifier<UseGenericEventHandlerAnalyzer, TypeCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseGenericEventHandler;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseGenericEventHandler)]
        public async Task Test_EventField()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    public event [|FooEventHandler|] E;
}

class FooEventArgs : EventArgs
{
}

delegate void FooEventHandler(object sender, FooEventArgs args);
", @"
using System;

class C
{
    public event EventHandler<FooEventArgs> E;
}

class FooEventArgs : EventArgs
{
}

delegate void FooEventHandler(object sender, FooEventArgs args);
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseGenericEventHandler)]
        public async Task Test_Event()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    public event [|FooEventHandler|] E
    {
        add { }
        remove { }
    }
}

class FooEventArgs : EventArgs
{
}

delegate void FooEventHandler(object sender, FooEventArgs args);
", @"
using System;

class C
{
    public event EventHandler<FooEventArgs> E
    {
        add { }
        remove { }
    }
}

class FooEventArgs : EventArgs
{
}

delegate void FooEventHandler(object sender, FooEventArgs args);
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseGenericEventHandler)]
        public async Task Test_Interface()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

public interface IC
{
    event [|FooEventHandler|] E;
}

public class FooEventArgs : EventArgs
{
}

public delegate void FooEventHandler(object sender, FooEventArgs args);
", @"
using System;

public interface IC
{
    event EventHandler<FooEventArgs> E;
}

public class FooEventArgs : EventArgs
{
}

public delegate void FooEventHandler(object sender, FooEventArgs args);
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseGenericEventHandler)]
        public async Task Test_InterfaceImplementation()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

interface IC
{
    event [|FooEventHandler|] E;
}

class FooEventArgs : EventArgs
{
}

delegate void FooEventHandler(object sender, FooEventArgs args);
", @"
using System;

interface IC
{
    event EventHandler<FooEventArgs> E;
}

class FooEventArgs : EventArgs
{
}

delegate void FooEventHandler(object sender, FooEventArgs args);
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseGenericEventHandler)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Collections.Generic;
using System.ComponentModel;

class Foo2
{
    public event EventHandler E;
}

class FooEventArgs : EventArgs
{
}

delegate void FooEventHandler(object sender, FooEventArgs args);

public interface INotifyPropertyChangedEx : INotifyPropertyChanged
{
}

class BaseClass : INotifyPropertyChangedEx
{
    public event PropertyChangedEventHandler PropertyChanged;
}

class BaseClass2 : INotifyPropertyChangedEx
{
    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
        add { throw new NotImplementedException(); }
        remove { throw new NotImplementedException(); }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseGenericEventHandler)]
        public async Task TestNoDiagnostic_NonVoidDelegate()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public delegate object FooEventHandler(object sender, FooEventArgs e);

    public event FooEventHandler OnFoo;

    void M()
    {
        object x = OnFoo?.Invoke(this, new FooEventArgs());
    }
}

class FooEventArgs
{
}
");
        }
    }
}
