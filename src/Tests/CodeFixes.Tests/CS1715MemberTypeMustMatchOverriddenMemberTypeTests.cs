// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS1715MemberTypeMustMatchOverriddenMemberTypeTests : AbstractCSharpCompilerDiagnosticFixVerifier<MemberDeclarationCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.MemberTypeMustMatchOverriddenMemberType;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.MemberTypeMustMatchOverriddenMemberType)]
        public async Task TestFix()
        {
            await VerifyFixAsync(@"
using System;

public class Foo : Base
{
    public override object PropertyName => base.PropertyName;

    public override object this[int index] => base[index];

    public override event EventHandler<FooEventArgs> EventName;

    public override event EventHandler<FooEventArgs> EventName2;
}

public class Base
{
    public virtual DateTime PropertyName { get; }

    public virtual string this[int index]
    {
        get { return null; }
    }

    public virtual event EventHandler EventName;

    public virtual event EventHandler EventName2
    {
        add { }
        remove { }
    }
}

public class FooEventArgs : EventArgs
{
}
", @"
using System;

public class Foo : Base
{
    public override DateTime PropertyName => base.PropertyName;

    public override string this[int index] => base[index];

    public override event EventHandler EventName;

    public override event EventHandler EventName2;
}

public class Base
{
    public virtual DateTime PropertyName { get; }

    public virtual string this[int index]
    {
        get { return null; }
    }

    public virtual event EventHandler EventName;

    public virtual event EventHandler EventName2
    {
        add { }
        remove { }
    }
}

public class FooEventArgs : EventArgs
{
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
