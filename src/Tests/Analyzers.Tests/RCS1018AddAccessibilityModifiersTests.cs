// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1018AddAccessibilityModifiersTests : AbstractCSharpDiagnosticVerifier<AddAccessibilityModifiersOrViceVersaAnalyzer, MemberDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddAccessibilityModifiersOrViceVersa;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task Test_NonNestedType()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary>x</summary>
    class [|C|] { }

    /// <summary>x</summary>
    interface [|I|] { }

    /// <summary>x</summary>
    enum [|E|] { }

    /// <summary>x</summary>
    struct [|S|] { }

    /// <summary>x</summary>
    delegate void [|D|]();
}
", @"
namespace N
{
    /// <summary>x</summary>
    internal class C { }

    /// <summary>x</summary>
    internal interface I { }

    /// <summary>x</summary>
    internal enum E { }

    /// <summary>x</summary>
    internal struct S { }

    /// <summary>x</summary>
    internal delegate void D();
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
    class [|C|]
    {
        /// <summary>x</summary>
        [|C|]()
        {
        }

        /// <summary>x</summary>
        delegate void [|D|]();

        /// <summary>x</summary>
        event EventHandler [|E|];

        /// <summary>x</summary>
        event EventHandler [|E2|]
        {
            add { }
            remove { }
        }

        /// <summary>x</summary>
        string [|_fieldName|];

        /// <summary>x</summary>
        object [|this|][int index]
        {
            get { return Items[index]; }
            set { Items[index] = value; }
        }

        /// <summary>x</summary>
        List<object> [|Items|] { get; } = new List<object>();
    }
}
", @"
using System;
using System.Collections.Generic;

namespace N
{
    /// <summary>x</summary>
    internal class C
    {
        /// <summary>x</summary>
        private C()
        {
        }

        /// <summary>x</summary>
        private delegate void D();

        /// <summary>x</summary>
        private event EventHandler E;

        /// <summary>x</summary>
        private event EventHandler E2
        {
            add { }
            remove { }
        }

        /// <summary>x</summary>
        private string _fieldName;

        /// <summary>x</summary>
        private object this[int index]
        {
            get { return Items[index]; }
            set { Items[index] = value; }
        }

        /// <summary>x</summary>
        private List<object> Items { get; } = new List<object>();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task Test_OperatorDeclaration()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    /// <summary>x</summary>
    class [|C|]
    {
        /// <summary>x</summary>
        static explicit operator [|C|](string value)
        {
            return new C();
        }

        /// <summary>x</summary>
        static explicit operator [|string|](C value)
        {
            return string.Empty;
        }

        /// <summary>x</summary>
        static C operator [|!|](C value)
        {
            return new C();
        }
    }
}
", @"
namespace N
{
    /// <summary>x</summary>
    internal class C
    {
        /// <summary>x</summary>
        public static explicit operator C(string value)
        {
            return new C();
        }

        /// <summary>x</summary>
        public static explicit operator string(C value)
        {
            return string.Empty;
        }

        /// <summary>x</summary>
        public static C operator !(C value)
        {
            return new C();
        }
    }
}
", options: Options.AddAllowedCompilerDiagnosticId(CompilerDiagnosticIdentifiers.UserDefinedOperatorMustBeDeclaredStaticAndPublic));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task Test_PartialClass()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    partial class [|C|]
    {
        partial void M();
    }

    partial class [|C|]
    {
        partial void M()
        {
        }
    }
}
", @"
namespace N
{
    internal partial class C
    {
        partial void M();
    }

    internal partial class C
    {
        partial void M()
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task Test_PartialClass2()
        {
            await VerifyDiagnosticAndFixAsync(@"
public class Foo
{
    partial class [|C|] { }

    protected internal partial class C { }

    partial interface [|I|] { }

    protected internal partial interface I { }

    partial interface [|S|] { }

    protected internal partial interface S { }
}
", @"
public class Foo
{
    protected internal partial class C { }

    protected internal partial class C { }

    protected internal partial interface I { }

    protected internal partial interface I { }

    protected internal partial interface S { }

    protected internal partial interface S { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task Test_PartialClass3()
        {
            await VerifyDiagnosticAndFixAsync(@"
public class Foo
{
    partial class [|C|] { }

    internal protected partial class C { }
}
", @"
public class Foo
{
    protected internal partial class C { }

    internal protected partial class C { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task Test_PartialClass4()
        {
            await VerifyDiagnosticAndFixAsync(@"
public class Foo
{
    partial class [|C|] { }

    protected partial class C { }
}
", @"
public class Foo
{
    protected partial class C { }

    protected partial class C { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task Test_PartialClass5()
        {
            await VerifyDiagnosticAndFixAsync(@"
public class Foo
{
    partial class [|C|] { }

    internal partial class C { }
}
", @"
public class Foo
{
    internal partial class C { }

    internal partial class C { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task Test_PartialClass6()
        {
            await VerifyDiagnosticAndFixAsync(@"
public class Foo
{
    partial class [|C|] { }

    internal partial class C { }
}
", @"
public class Foo
{
    internal partial class C { }

    internal partial class C { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task Test_PartialClass7()
        {
            await VerifyDiagnosticAndFixAsync(@"
public class Foo
{
    partial class [|C|] { }

    partial class [|C|] { }
}
", @"
public class Foo
{
    private partial class C { }

    private partial class C { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task Test_PartialClass8()
        {
            await VerifyDiagnosticAndFixAsync(@"
public class C
{
    protected class C2
    {
    }
}

public class C3 : C
{
    new class [|C2|]
    {
    }
}
", @"
public class C
{
    protected class C2
    {
    }
}

public class C3 : C
{
    new private class C2
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task TestNoDiagnostic_StaticConstructor()
        {
            await VerifyNoDiagnosticAsync(@"
public class C
{
    static C()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task TestNoDiagnostic_Destructor()
        {
            await VerifyNoDiagnosticAsync(@"
public class C
{
    ~C()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddAccessibilityModifiersOrViceVersa)]
        public async Task TestNoDiagnostic_ExplicitInterfaceImplementation()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

public class C
{
    public interface I
    {
        void M();
        string P { get; set; }
        object this[int index] { get; set; }
        event EventHandler EH;
    }

    public class C2 : I
    {
        event EventHandler I.EH
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        void I.M()
        {
            throw new NotImplementedException();
        }

        string I.P
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        object I.this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
");
        }
    }
}
