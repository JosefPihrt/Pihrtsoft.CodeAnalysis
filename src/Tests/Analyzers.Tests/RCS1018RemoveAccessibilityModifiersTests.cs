// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1018RemoveAccessibilityModifiersTests : AbstractCSharpDiagnosticVerifier<AddAccessibilityModifiersOrViceVersaAnalyzer, MemberDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddAccessibilityModifiersOrViceVersa;

        public override CSharpTestOptions Options
        {
            get { return base.Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.RemoveAccessibilityModifiers); }
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task Test_NonNestedType()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary>x</summary>
    [|internal|] class C { }

    /// <summary>x</summary>
    [|internal|] interface I { }

    /// <summary>x</summary>
    [|internal|] enum E { }

    /// <summary>x</summary>
    [|internal|] struct S { }

    /// <summary>x</summary>
    [|internal|] delegate void D();
}
", @"
namespace N
{
    /// <summary>x</summary>
    class C { }

    /// <summary>x</summary>
    interface I { }

    /// <summary>x</summary>
    enum E { }

    /// <summary>x</summary>
    struct S { }

    /// <summary>x</summary>
    delegate void D();
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task Test_MemberDeclaration()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Collections.Generic;

namespace N
{
    /// <summary>x</summary>
    class C
    {
        /// <summary>x</summary>
        [|private|] C()
        {
        }

        /// <summary>x</summary>
        [|private|] delegate void D();

        /// <summary>x</summary>
        [|private|] event EventHandler E;

        /// <summary>x</summary>
        [|private|] event EventHandler E2
        {
            add { }
            remove { }
        }

        /// <summary>x</summary>
        [|private|] string _fieldName;

        /// <summary>x</summary>
        [|private|] object this[int index]
        {
            get { return Items[index]; }
            set { Items[index] = value; }
        }

        /// <summary>x</summary>
        [|private|] List<object> Items { get; } = new List<object>();
    }
}
", @"
using System;
using System.Collections.Generic;

namespace N
{
    /// <summary>x</summary>
    class C
    {
        /// <summary>x</summary>
        C()
        {
        }

        /// <summary>x</summary>
        delegate void D();

        /// <summary>x</summary>
        event EventHandler E;

        /// <summary>x</summary>
        event EventHandler E2
        {
            add { }
            remove { }
        }

        /// <summary>x</summary>
        string _fieldName;

        /// <summary>x</summary>
        object this[int index]
        {
            get { return Items[index]; }
            set { Items[index] = value; }
        }

        /// <summary>x</summary>
        List<object> Items { get; } = new List<object>();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task Test_PartialClass()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    [|internal|] partial class C
    {
    }

    [|internal|] partial class C
    {
    }
}
", @"
namespace N
{
    partial class C
    {
    }

    partial class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task Test_PartialClass2()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    [|internal|] partial class C
    {
    }

    partial class C
    {
    }
}
", @"
namespace N
{
    partial class C
    {
    }

    partial class C
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task TestNoDiagnostic_OperatorDeclaration()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public static explicit operator C(string value)
    {
        return new C();
    }

    public static explicit operator string(C value)
    {
        return string.Empty;
    }

    public static C operator !(C value)
    {
        return new C();
    }
}
");
        }
    }
}
