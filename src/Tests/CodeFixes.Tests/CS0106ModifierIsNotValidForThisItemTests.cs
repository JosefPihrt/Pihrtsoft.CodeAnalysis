// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0106ModifierIsNotValidForThisItemTests : AbstractCSharpCompilerDiagnosticFixVerifier<ModifiersCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.ModifierIsNotValidForThisItem;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.ModifierIsNotValidForThisItem)]
        public async Task Test_VirtualModifierInStruct()
        {
            await VerifyFixAsync(@"
using System;

struct S
{
    public virtual string M() => null;

    public virtual string P { get; }

    public virtual string this[int index] => null;

    public virtual event EventHandler E;
}
", @"
using System;

struct S
{
    public string M() => null;

    public string P { get; }

    public string this[int index] => null;

    public event EventHandler E;
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.ModifierIsNotValidForThisItem)]
        public async Task Test_AsyncModifier()
        {
            await VerifyFixAsync(@"
using System;

struct S
{
    public async string P { get; }

    public async string this[int index] => null;

    public async event EventHandler E;
}
", @"
using System;

struct S
{
    public string P { get; }

    public string this[int index] => null;

    public event EventHandler E;
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
