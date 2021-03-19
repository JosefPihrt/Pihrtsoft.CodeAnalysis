// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0016AddNewLineAfterAttributeListTests : AbstractCSharpDiagnosticVerifier<AddNewLineAfterAttributeListAnalyzer, SyntaxTokenCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddNewLineAfterAttributeList;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_Class()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

namespace N
{
    [Obsolete] [||]class C
    {
    }

    [Obsolete] [||]static class SC
    {
    }
}
", @"
using System;

namespace N
{
    [Obsolete]
    class C
    {
    }

    [Obsolete]
    static class SC
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_Constructor()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    [Obsolete] [||]C()
    {
    }

    [Obsolete] [||]public C(object p)
    {
    }
}
", @"
using System;

class C
{
    [Obsolete]
    C()
    {
    }

    [Obsolete]
    public C(object p)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_ConversionOperator()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    [Obsolete] [||]public static explicit operator C(string value) => null;
}
", @"
using System;

class C
{
    [Obsolete]
    public static explicit operator C(string value) => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_Delegate()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    [Obsolete] [||]delegate void D();
}
", @"
using System;

class C
{
    [Obsolete]
    delegate void D();
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_Destructor()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    [Obsolete] [||]~C()
    {
    }
}
", @"
using System;

class C
{
    [Obsolete]
    ~C()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_Enum()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

namespace N
{
    [Obsolete] [||]enum E
    {
    }

    [Obsolete] [||]public enum E2
    {
    }
}
", @"
using System;

namespace N
{
    [Obsolete]
    enum E
    {
    }

    [Obsolete]
    public enum E2
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_EnumMember()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

enum E
{
    [Obsolete] [||]A
}
", @"
using System;

enum E
{
    [Obsolete]
    A
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_Event()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    [Obsolete] [||]event EventHandler E
    {
        add { }
        remove { }
    }

    [Obsolete] [||]public event EventHandler E2
    {
        add { }
        remove { }
    }
}
", @"
using System;

class C
{
    [Obsolete]
    event EventHandler E
    {
        add { }
        remove { }
    }

    [Obsolete]
    public event EventHandler E2
    {
        add { }
        remove { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_EventField()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    [Obsolete] [||]event EventHandler E;

    [Obsolete] [||]public event EventHandler E2;
}
", @"
using System;

class C
{
    [Obsolete]
    event EventHandler E;

    [Obsolete]
    public event EventHandler E2;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_Field()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    [Obsolete] [||]string F;

    [Obsolete] [||]public string F2;
}
", @"
using System;

class C
{
    [Obsolete]
    string F;

    [Obsolete]
    public string F2;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_Indexer()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    [Obsolete] [||]string this[int index]
    {
        get { return null; }
        set { }
    }

    [Obsolete] [||]public string this[int index, int index2]
    {
        get { return null; }
        set { }
    }
}
", @"
using System;

class C
{
    [Obsolete]
    string this[int index]
    {
        get { return null; }
        set { }
    }

    [Obsolete]
    public string this[int index, int index2]
    {
        get { return null; }
        set { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_Interface()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

namespace N
{
    [Obsolete] [||]interface I
    {
    }

    [Obsolete] [||]public interface I2
    {
    }
}
", @"
using System;

namespace N
{
    [Obsolete]
    interface I
    {
    }

    [Obsolete]
    public interface I2
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_Method()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    [Obsolete] [||]void M()
    {
    }

    [Obsolete] [||]static void SM()
    {
    }
}
", @"
using System;

class C
{
    [Obsolete]
    void M()
    {
    }

    [Obsolete]
    static void SM()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_Operator()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    [Obsolete] [||]public static C operator !(C value) => null;
}
", @"
using System;

class C
{
    [Obsolete]
    public static C operator !(C value) => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_Property()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    [Obsolete] [||]string P
    {
        get { return null; }
        set { }
    }

    [Obsolete] [||]public string P2
    {
        get { return null; }
        set { }
    }
}
", @"
using System;

class C
{
    [Obsolete]
    string P
    {
        get { return null; }
        set { }
    }

    [Obsolete]
    public string P2
    {
        get { return null; }
        set { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_Struct()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

namespace N
{
    [Obsolete] [||]struct S
    {
    }

    [Obsolete] [||]public struct S2
    {
    }
}
", @"
using System;

namespace N
{
    [Obsolete]
    struct S
    {
    }

    [Obsolete]
    public struct S2
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_GetSetAccessor()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    string P
    {
        [Foo] [||]get { return null; }
        [Foo] [||]set { }
    }

    public string P2
    {
        [Foo]
        private get { return null; }
        set { }
    }

    public string P3
    {
        get { return null; }
        [Foo]
        private set { }
    }
}

[AttributeUsage(AttributeTargets.All)]
public sealed class FooAttribute : Attribute
{
}
", @"
using System;

class C
{
    string P
    {
        [Foo]
        get { return null; }
        [Foo]
        set { }
    }

    public string P2
    {
        [Foo]
        private get { return null; }
        set { }
    }

    public string P3
    {
        get { return null; }
        [Foo]
        private set { }
    }
}

[AttributeUsage(AttributeTargets.All)]
public sealed class FooAttribute : Attribute
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_InitSetter()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    private string _f;

    string P
    {
        get { return _f; }
        [Foo] [||]init { _f = value; }
    }
}

[AttributeUsage(AttributeTargets.All)]
public sealed class FooAttribute : Attribute
{
}
", @"
using System;

class C
{
    private string _f;

    string P
    {
        get { return _f; }
        [Foo]
        init { _f = value; }
    }
}

[AttributeUsage(AttributeTargets.All)]
public sealed class FooAttribute : Attribute
{
}
", options: Options.AddAllowedCompilerDiagnosticId("CS0518"));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterAttributeList)]
        public async Task Test_AddRemoveAccessor()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    event EventHandler E
    {
        [Foo] [||]add { }
        [Foo] [||]remove { }
    }
}

[AttributeUsage(AttributeTargets.All)]
public sealed class FooAttribute : Attribute
{
}
", @"
using System;

class C
{
    event EventHandler E
    {
        [Foo]
        add { }
        [Foo]
        remove { }
    }
}

[AttributeUsage(AttributeTargets.All)]
public sealed class FooAttribute : Attribute
{
}
");
        }
    }
}
