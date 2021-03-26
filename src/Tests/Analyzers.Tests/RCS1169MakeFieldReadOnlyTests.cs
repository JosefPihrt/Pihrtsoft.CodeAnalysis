// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.Analysis.MakeMemberReadOnly;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1169MakeFieldReadOnlyTests : AbstractCSharpDiagnosticVerifier<MakeMemberReadOnlyAnalyzer, MemberDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.MakeFieldReadOnly;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task Test_InstanceField()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    [|private string _f1;|]
    [|private string _f2;|]

    public C()
    {
        _f1 = null;
    }
}
", @"
class C
{
    private readonly string _f1;
    private readonly string _f2;

    public C()
    {
        _f1 = null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task Test_InstanceField_Int32()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    [|private int _f;|]

    public C()
    {
        _f = 0;
    }
}
", @"
class C
{
    private readonly int _f;

    public C()
    {
        _f = 0;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task Test_InstanceField_Enum()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    [|private StringSplitOptions _f;|]

    public C()
    {
        _f = 0;
    }
}
", @"
using System;

class C
{
    private readonly StringSplitOptions _f;

    public C()
    {
        _f = 0;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task Test_InstanceField_ReadOnlyStruct()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    [|private B _f;|]

    public C()
    {
        _f = default;
    }
}

readonly struct B
{
}
", @"
class C
{
    private readonly B _f;

    public C()
    {
        _f = default;
    }
}

readonly struct B
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task Test_StaticField()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    [|private static string _f;|]

    static C()
    {
        _f = null;
    }
}
", @"
class C
{
    private static readonly string _f;

    static C()
    {
        _f = null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task TestNoDiagnostic_Assigned()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    int _f;

    void M()
    {
        _f = 0;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task TestNoDiagnostic_Struct()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    B _f;
}

struct B
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task TestNoDiagnostic_Tuple()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    private int _tuple1;
    private int _tuple2;

    void M()
    {
        (_tuple1, _tuple2) = default((int, int));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task TestNoDiagnostic_AssignedInConstructor_LocalFunction()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    int _f;

    public C()
    {
        void LF()
        {
            _f = 0;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task TestNoDiagnostic_AssignedInConstructor_SimpleLambda()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    private string _f;

    public C()
    {
        var items = new List<string>();

        IEnumerable<string> q = items.Select(f =>
        {
            _f = null;
            return f;
        });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task TestNoDiagnostic_AssignedInConstructor_ParenthesizedLambda()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    private string _f;

    public C()
    {
        var items = new List<string>();

        IEnumerable<string> q = items.Select((f) =>
        {
            _f = null;
            return f;
        });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task TestNoDiagnostic_AssignedInConstructor_AnonymousMethod()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    private string _f;

    public C()
    {
        var items = new List<string>();

        IEnumerable<string> q = items.Select(delegate (string f)
        {
            _f = null;
            return f;
        });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task TestNoDiagnostic_StaticFieldAssignedInInstanceConstructor()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    static int _f;

    public C()
    {
        _f = 0;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task TestNoDiagnostic_Generic()
        {
            await VerifyNoDiagnosticAsync(@"
class B
{
}

class C<T> : B
{
    B _f;

    C<TResult> M<TResult>()
    {
        return new C<TResult>() { _f = this };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeFieldReadOnly)]
        public async Task TestNoDiagnostic_ReturnRef()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    int _f;

    ref int M()
    {
        return ref _f;
    }
}
");
        }
    }
}
