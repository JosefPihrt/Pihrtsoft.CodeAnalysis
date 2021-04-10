// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1171SimplifyLazyInitializationTests : AbstractCSharpDiagnosticVerifier<SimplifyLazyInitializationAnalyzer, BlockCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.SimplifyLazyInitialization;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_IfWithBraces()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object _m;

    object M()
    {
        [|if (_m == null)
        {
            _m = I();
        }

        return _m;|]
    }

    object I() => new object();
}
", @"
class C
{
    object _m;

    object M()
    {
        return _m ?? (_m = I());
    }

    object I() => new object();
}
", options: WellKnownCSharpTestOptions.Default_CSharp7_3);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_IfWithBraces_UseCoalesceAssignment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object _m;

    object M()
    {
        [|if (_m == null)
        {
            _m = I();
        }

        return _m;|]
    }

    object I() => new object();
}
", @"
class C
{
    object _m;

    object M()
    {
        return _m ??= I();
    }

    object I() => new object();
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_IfWithoutBraces()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object _m;

    object M()
    {
        [|if (_m == null)
            _m = I();

        return _m;|]
    }

    object I() => new object();
}
", @"
class C
{
    object _m;

    object M()
    {
        return _m ?? (_m = I());
    }

    object I() => new object();
}
", options: WellKnownCSharpTestOptions.Default_CSharp7_3);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_NullOnLeftSide()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object _m;

    object M()
    {
        [|if (null == _m)
        {
            _m = I();
        }

        return _m;|]
    }

    object I() => new object();
}
", @"
class C
{
    object _m;

    object M()
    {
        return _m ?? (_m = I());
    }

    object I() => new object();
}
", options: WellKnownCSharpTestOptions.Default_CSharp7_3);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_QualifiedWithThis()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object _m;

    object M()
    {
        [|if (this._m == null)
        {
            this._m = I();
        }

        return this._m;|]
    }

    object I() => new object();
}
", @"
class C
{
    object _m;

    object M()
    {
        return this._m ?? (this._m = I());
    }

    object I() => new object();
}
", options: WellKnownCSharpTestOptions.Default_CSharp7_3);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_QualifiedWithThis_UseCoalesceAssignment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object _m;

    object M()
    {
        [|if (this._m == null)
        {
            this._m = I();
        }

        return this._m;|]
    }

    object I() => new object();
}
", @"
class C
{
    object _m;

    object M()
    {
        return this._m ??= I();
    }

    object I() => new object();
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_MemberInitialization()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object _m;
    readonly C _c;

    object M()
    {
        [|if (_c._m == null)
        {
            _c._m = I();
        }

        return _c._m;|]
    }

    object I() => new object();
}
", @"
class C
{
    object _m;
    readonly C _c;

    object M()
    {
        return _c._m ?? (_c._m = I());
    }

    object I() => new object();
}
", options: WellKnownCSharpTestOptions.Default_CSharp7_3);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_MemberInitialization_UseCoalesceAssignment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object _m;
    readonly C _c;

    object M()
    {
        [|if (_c._m == null)
        {
            _c._m = I();
        }

        return _c._m;|]
    }

    object I() => new object();
}
", @"
class C
{
    object _m;
    readonly C _c;

    object M()
    {
        return _c._m ??= I();
    }

    object I() => new object();
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_Property()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object _m;

    object P
    {
        get
        {
            [|if (_m == null)
                _m = I();

            return _m;|]
        }
    }

    object I() => new object();
}
", @"
class C
{
    object _m;

    object P
    {
        get
        {
            return _m ?? (_m = I());
        }
    }

    object I() => new object();
}
", options: WellKnownCSharpTestOptions.Default_CSharp7_3);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_Indexer()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    object _m;

    object this[int index]
    {
        get
        {
            [|if (_m == null)
            {
                _m = I();
            }

            return _m;|]
        }
    }

    object I() => new object();
}
", @"
class C
{
    object _m;

    object this[int index]
    {
        get
        {
            return _m ?? (_m = I());
        }
    }

    object I() => new object();
}
", options: WellKnownCSharpTestOptions.Default_CSharp7_3);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_Nullable()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private int? _m;

    int? M()
    {
        [|if (_m == null)
        {
            _m = I();
        }

        return _m;|]
    }

    int I() => 0;
}
", @"
class C
{
    private int? _m;

    int? M()
    {
        return _m ?? (_m = I());
    }

    int I() => 0;
}
", options: WellKnownCSharpTestOptions.Default_CSharp7_3);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_Nullable_Value()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private int? _m;

    int M()
    {
        [|if (_m == null)
        {
            _m = I();
        }

        return _m.Value;|]
    }

    int I() => 0;
}
", @"
class C
{
    private int? _m;

    int M()
    {
        return _m ?? (_m = I()).Value;
    }

    int I() => 0;
}
", options: WellKnownCSharpTestOptions.Default_CSharp7_3);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_Nullable_Value_UseCoalesceAssignment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private int? _m;

    int M()
    {
        [|if (_m == null)
        {
            _m = I();
        }

        return _m.Value;|]
    }

    int I() => 0;
}
", @"
class C
{
    private int? _m;

    int M()
    {
        return _m ??= I();
    }

    int I() => 0;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_Nullable_Value_QualifiedWithThis()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private int? _m;

    int M()
    {
        [|if (this._m == null)
        {
            this._m = I();
        }

        return this._m.Value;|]
    }

    int I() => 0;
}
", @"
class C
{
    private int? _m;

    int M()
    {
        return this._m ?? (this._m = I()).Value;
    }

    int I() => 0;
}
", options: WellKnownCSharpTestOptions.Default_CSharp7_3);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_Nullable_Value_QualifiedWithThis_UseCoalesceAssignment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private int? _m;

    int M()
    {
        [|if (this._m == null)
        {
            this._m = I();
        }

        return this._m.Value;|]
    }

    int I() => 0;
}
", @"
class C
{
    private int? _m;

    int M()
    {
        return this._m ??= I();
    }

    int I() => 0;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_Nullable_Value_MemberInitialization()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private int? _m;
    private C _c;

    int M()
    {
        [|if (_c._m == null)
        {
            _c._m = I();
        }

        return _c._m.Value;|]
    }

    int I() => 0;
}
", @"
class C
{
    private int? _m;
    private C _c;

    int M()
    {
        return _c._m ?? (_c._m = I()).Value;
    }

    int I() => 0;
}
", options: WellKnownCSharpTestOptions.Default_CSharp7_3);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_Nullable_Value_MemberInitialization_UseCoalesceAssignment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private int? _m;
    private C _c;

    int M()
    {
        [|if (_c._m == null)
        {
            _c._m = I();
        }

        return _c._m.Value;|]
    }

    int I() => 0;
}
", @"
class C
{
    private int? _m;
    private C _c;

    int M()
    {
        return _c._m ??= I();
    }

    int I() => 0;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_Nullable_HasValue()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private int? _m;

    int? M()
    {
        [|if (!_m.HasValue)
        {
            _m = I();
        }

        return _m;|]
    }

    int I() => 0;
}
", @"
class C
{
    private int? _m;

    int? M()
    {
        return _m ?? (_m = I());
    }

    int I() => 0;
}
", options: WellKnownCSharpTestOptions.Default_CSharp7_3);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task Test_Nullable_Property()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private int? _m;

    int? M
    {
        get
        {
            [|if (!_m.HasValue)
                _m = I();

            return _m;|]
        }
    }

    int I() => 0;
}
", @"
class C
{
    private int? _m;

    int? M
    {
        get
        {
            return _m ?? (_m = I());
        }
    }

    int I() => 0;
}
", options: WellKnownCSharpTestOptions.Default_CSharp7_3);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task TestNoDiagnostic_IfElse()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    object _m;

    object M()
    {
        if (_m == null)
        {
            _m = I();
        }
        else
        {
        }

        return _m;
    }

    object I() => new object();
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLazyInitialization)]
        public async Task TestNoDiagnostic_Property_IfElse()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    object _m;

    object P
    {
        get
        {
            if (_m == null)
            {
                _m = I();
            }
            else
            {
            }

            return _m;
        }
    }

    object I() => new object();
}
");
        }
    }
}
