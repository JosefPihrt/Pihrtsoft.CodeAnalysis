// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.Analysis.MakeMemberReadOnly;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1170UseReadOnlyAutoPropertyTests : AbstractCSharpDiagnosticVerifier<MakeMemberReadOnlyAnalyzer, MemberDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseReadOnlyAutoProperty;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task Test_InstanceProperty()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    public string P1 { get; [|private set;|] }
    public string P2 { get; [|private set;|] }

    public C()
    {
        P1 = null;
    }
}
", @"
class C
{
    public string P1 { get; }
    public string P2 { get; }

    public C()
    {
        P1 = null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task Test_InstanceProperty_Int32()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    public int P { get; [|private set;|] }

    public C()
    {
        P = 0;
    }
}
", @"
class C
{
    public int P { get; }

    public C()
    {
        P = 0;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task Test_InstanceProperty_Enum()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    public StringSplitOptions P { get; [|private set;|] }

    public C()
    {
        P = 0;
    }
}
", @"
using System;

class C
{
    public StringSplitOptions P { get; }

    public C()
    {
        P = 0;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task Test_InstanceProperty_ReadOnlyStruct()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    public B P { get; [|private set;|] }

    public C()
    {
        P = default;
    }
}

readonly struct B
{
}
", @"
class C
{
    public B P { get; }

    public C()
    {
        P = default;
    }
}

readonly struct B
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task Test_StaticProperty()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    public static string P { get; [|private set;|] }

static C()
    {
        P = null;
    }
}
", @"
using System;

class C
{
    public static string P { get; }

    static C()
    {
        P = null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_ReadOnlyProperty()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public int P { get; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_FullProperty()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    private int _p;

    public int P
    {
        get { return _p; }
        private set { _p = value; }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_Assigned()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public int P { get; private set; }

    void M()
    {
        P = 0;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_Struct()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public B P { get; private set; }
}

struct B
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_Tuple()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public int P1 { get; private set; }
    public int P2 { get; private set; }

    void M()
    {
        (P1, P2) = default((int, int));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_AssignedInConstructor_LocalFunction()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public int P { get; private set; }

    public C()
    {
        void LF()
        {
            P = 0;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_AssignedInConstructor_SimpleLambda()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    public string P { get; private set; }

    public C()
    {
        var items = new List<string>();

        IEnumerable<string> q = items.Select(f =>
        {
            P = null;
            return f;
        });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_AssignedInConstructor_ParenthesizedLambda()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    public string P { get; private set; }

    public C()
    {
        var items = new List<string>();

        IEnumerable<string> q = items.Select((f) =>
        {
            P = null;
            return f;
        });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_AssignedInConstructor_AnonymousMethod()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    public string P { get; private set; }

    public C()
    {
        var items = new List<string>();

        IEnumerable<string> q = items.Select(delegate (string f)
        {
            P = null;
            return f;
        });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_StaticPropertyAssignedInInstanceConstructor()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public static int P { get; private set; }

    public C()
    {
        P = 0;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_Generic()
        {
            await VerifyNoDiagnosticAsync(@"
class B
{
}

class C<T> : B
{
    public B P { get; private set; }

    C<TResult> M<TResult>()
    {
        return new C<TResult>() { P = this };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_VariablePropertyAssignedInConstructor()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public C P1 { get; private set; }
    public C P2 { get; private set; }

    public C(C c)
    {
        c.P1 = this;
        c.P1.P2 = this;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_DateMemberAttribute()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Runtime.Serialization;

class C
{
    [DataMember]
    public string P { get; private set; }
}

namespace System.Runtime.Serialization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class DataMemberAttribute : Attribute
    {
    }
}", options: Options.AddAllowedCompilerDiagnosticId("CS0436"));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_SetterHasAttribute()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Diagnostics;

class C
{
    public string P { get; [DebuggerStepThrough]private set; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_AssignedSymbolIsNull()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public string P { get; private set; }

    void M()
    {
        P = null;

        var c2 = new C2();
        c2.P = null;
    }
}

class C2
{
    public string P2 { get; }
}
", options: Options.AddAllowedCompilerDiagnosticId("CS1061"));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_ReadOnlyAutoPropertyNotAvailableInCSharp5()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public string P { get; private set; }
}
", options: WellKnownCSharpTestOptions.Default_CSharp5);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_InitSetter()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public string P { get; private init; }
}
", options: Options.AddAllowedCompilerDiagnosticId("CS0518"));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseReadOnlyAutoProperty)]
        public async Task TestNoDiagnostic_DependencyAttribute()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    [Dependency]
    private string P { get; set; }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
class DependencyAttribute : Attribute
{
}
");
        }
    }
}
