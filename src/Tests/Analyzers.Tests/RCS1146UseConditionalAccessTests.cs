// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1146UseConditionalAccessTests : AbstractCSharpDiagnosticVerifier<UseConditionalAccessAnalyzer, UseConditionalAccessCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseConditionalAccess;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task Test_IfStatement_ReferenceType()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        C x = null;

        [|if (x != null)
            x.M();|]

        [|if (x != null)
        {
            x.M();
        }|]
    }
}
", @"
class C
{
    void M()
    {
        C x = null;

        x?.M();

        x?.M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task Test_IfStatement_ValueType()
        {
            await VerifyDiagnosticAndFixAsync(@"
struct S
{
    void M()
    {
        S? x = null;

        [|if (x != null)
            x.Value.M();|]

        [|if (x != null)
        {
            x.Value.M();
        }|]
    }
}
", @"
struct S
{
    void M()
    {
        S? x = null;

        x?.M();

        x?.M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task Test_LogicalAnd_ReferenceType()
        {
            await VerifyDiagnosticAndFixAsync(@"
class Foo
{
    const string NonNullConst = ""x"";

    string P { get; }

    void M()
    {
        bool f = false;

        Foo x = null;

        if ([|x != null && x.Equals(x)|]) { }

        if ([|x != default(Foo) && x.Equals(x)|]) { }

        if ([|x != default && x.Equals(x)|]) { }

        if ([|null != x && x.Equals(x)|]) { }

        if ([|x != null && (x.Equals(x)|])) { }

        if ([|x != null && x.Equals(x)|] && f) { }

        if (f && [|x != null && x.Equals(x)|]) { }

        if ([|x != null && x.P.Length > 1|]) { }

        if ([|x != null && !x.Equals(x)|]) { }

        if ([|x != null && (!x.Equals(x)|])) { }

        if ([|x != null && x.P == ""x""|]) { }

        if ([|x != null && x.P == NonNullConst|]) { }

        if ([|x != null && x.P != null|]) { }

        if ([|x != null && x.P is object|]) { }

        if ([|x != null && x.P is object _|]) { }

        if (f &&
     /*lt*/ [|x != null &&
            x.Equals(""x"")|] /*tt*/
            && f) { }
    }
}
", @"
class Foo
{
    const string NonNullConst = ""x"";

    string P { get; }

    void M()
    {
        bool f = false;

        Foo x = null;

        if (x?.Equals(x) == true) { }

        if (x?.Equals(x) == true) { }

        if (x?.Equals(x) == true) { }

        if (x?.Equals(x) == true) { }

        if ((x?.Equals(x) == true)) { }

        if (x?.Equals(x) == true && f) { }

        if (f && x?.Equals(x) == true) { }

        if (x?.P.Length > 1) { }

        if (x?.Equals(x) == false) { }

        if ((x?.Equals(x) == false)) { }

        if (x?.P == ""x"") { }

        if (x?.P == NonNullConst) { }

        if (x?.P != null) { }

        if (x?.P is object) { }

        if (x?.P is object _) { }

        if (f &&
     /*lt*/ x?.Equals(""x"") == true /*tt*/
            && f) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task Test_LogicalOr_ReferenceType()
        {
            await VerifyDiagnosticAndFixAsync(@"
class Foo
{
    void M()
    {
        Foo x = null;

        if ([|x == null || x.Equals(x)|]) { }

        if ([|x == default(Foo) || x.Equals(x)|]) { }

        if ([|x == default || x.Equals(x)|]) { }

        if ([|x == null || (x.Equals(x)|])) { }

        if ([|x == null || !x.Equals(x)|]) { }

        if ([|x == null || (!x.Equals(x)|])) { }
    }
}
", @"
class Foo
{
    void M()
    {
        Foo x = null;

        if (x?.Equals(x) != false) { }

        if (x?.Equals(x) != false) { }

        if (x?.Equals(x) != false) { }

        if ((x?.Equals(x) != false)) { }

        if (x?.Equals(x) != true) { }

        if ((x?.Equals(x) != true)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task Test_LogicalAnd_ElementAccess()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class Foo
{
    void M()
    {
        Dictionary<int, string> dic = null;

        if ([|dic != null && dic[0].Equals(""x"")|]) { }

        if ([|dic != null && dic[0].Length > 1|]) { }

        if ([|dic != null && !dic[0].Equals(""x"")|]) { }
    }
}
", @"
using System.Collections.Generic;

class Foo
{
    void M()
    {
        Dictionary<int, string> dic = null;

        if (dic?[0].Equals(""x"") == true) { }

        if (dic?[0].Length > 1) { }

        if (dic?[0].Equals(""x"") == false) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task Test_LogicalAnd_Nested()
        {
            await VerifyDiagnosticAndFixAsync(@"
class Foo
{
    Foo M()
    {
        Foo x = null;

        if (x != null && x.M() != null && [|x.M().M2() != null && x.M().M2().M3() != null|]) { }

        if (x != null && (x.M() != null && ([|x.M().M2() != null && x.M().M2().M3() != null|]))) { }

        if (((x != null) && (x.M() != null)) && ((([|x.M().M2() != null)) && (x.M().M2().M3() != null|]))) { }

        return null;
    }

    Foo M2() => null;
    Foo M3() => null;
}
", @"
class Foo
{
    Foo M()
    {
        Foo x = null;

        if (x?.M()?.M2()?.M3() != null) { }

        if (((x?.M()?.M2()?.M3() != null))) { }

        if ((((x?.M()?.M2()?.M3() != null)))) { }

        return null;
    }

    Foo M2() => null;
    Foo M3() => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task Test_LogicalAnd_NullableType()
        {
            await VerifyDiagnosticAndFixAsync(@"
struct Foo
{
    const string NonNullConst = ""x"";

    string P { get; }

    void M()
    {
        Foo? x = null;

        if ([|x != null && x.Value.Equals(x)|]) { }

        if ([|x != null && x.Value.P.Length > 1|]) { }

        if ([|x != null && !x.Value.Equals(x)|]) { }

        if ([|x != null && x.Value.P == ""x""|]) { }

        if ([|x != null && x.Value.P == NonNullConst|]) { }

        if ([|x != null && x.Value.P != null|]) { }

        if ([|x != null && x.Value.P is object|]) { }

        if ([|x != null && x.Value.P is object _|]) { }

        if (x != null && [|x.Value.ToString() != null && x.Value.ToString().ToString() != null|]) { }
    }
}
", @"
struct Foo
{
    const string NonNullConst = ""x"";

    string P { get; }

    void M()
    {
        Foo? x = null;

        if (x?.Equals(x) == true) { }

        if (x?.P.Length > 1) { }

        if (x?.Equals(x) == false) { }

        if (x?.P == ""x"") { }

        if (x?.P == NonNullConst) { }

        if (x?.P != null) { }

        if (x?.P is object) { }

        if (x?.P is object _) { }

        if (x?.ToString()?.ToString() != null) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task Test_LogicalOr_NullableType()
        {
            await VerifyDiagnosticAndFixAsync(@"
struct Foo
{
    void M()
    {
        Foo? x = null;

        if ([|x == null || x.Value.Equals(x)|]) { }

        if ([|x == null || !x.Value.Equals(x)|]) { }
    }
}
", @"
struct Foo
{
    void M()
    {
        Foo? x = null;

        if (x?.Equals(x) != false) { }

        if (x?.Equals(x) != true) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task Test_LogicalAnd_Nested_NullableType()
        {
            await VerifyDiagnosticAndFixAsync(@"
struct Foo
{
    void M()
    {
        Foo? x = null;

        if (x != null
            && x.Value.ToString() != null
            && [|x.Value.ToString().ToString() != null
            && x.Value.ToString().ToString().ToString() != null|]) { }
    }
}
", @"
struct Foo
{
    void M()
    {
        Foo? x = null;

        if (x?.ToString()?.ToString()?.ToString() != null) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task TestNoDiagnostic_LogicalAnd_ReferenceType()
        {
            await VerifyNoDiagnosticAsync(@"
class Foo
{
    const string NullConst = null;
    const string NonNullConst = ""x"";

    string P { get; }

    void M()
    {
        bool f = false;

        string s = null;

        Foo x = null;

        if (x != null && x.P == null && f) { }

        if (x != null && x.P is null && f) { }

        if (x != null && x.P == NullConst && f) { }

        if (x != null && x.P == s && f) { }

        if (x != null && x.P != ""x"" && f) { }

        if (x != null && x.P != NonNullConst && f) { }

        if (x != null && x.P != s && f) { }

        if (x != null && (x.P != null) is object _) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task TestNoDiagnostic_LogicalOr_ReferenceType()
        {
            await VerifyNoDiagnosticAsync(@"
class Foo
{
    const string NullConst = null;
    const string NonNullConst = ""x"";

    string P { get; }

    void M()
    {
        bool f = false;

        string s = null;

        Foo x = null;


        if (x == null || x.P.Length > 1) { }

        if (x == null || x.P == ""x"") { }

        if (x == null || x.P == NonNullConst) { }

        if (x == null || x.P != null) { }

        if (x == null || x.P is object) { }

        if (x == null || x.P is object _) { }

        if (x == null || x.P == null && f) { }

        if (x == null || x.P == NullConst && f) { }

        if (x == null || x.P == s && f) { }

        if (x == null || x.P != ""x"" && f) { }

        if (x == null || x.P != NonNullConst && f) { }

        if (x == null || x.P != s && f) { }

        if (x == null || (x.P != null) is object _) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task TestNoDiagnostic_LogicalAnd_ValueType()
        {
            await VerifyNoDiagnosticAsync(@"
struct Foo
{
    public int P { get; }

    void M()
    {
        var x = new Foo();

        if (x != null && x.P > 0) { }
    }

    public static bool operator ==(Foo left, Foo right) => left.Equals(right);
    public static bool operator !=(Foo left, Foo right) => !(left == right);
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task TestNoDiagnostic_LogicalAnd_NullableType()
        {
            await VerifyNoDiagnosticAsync(@"
struct Foo
{
    void M()
    {
        bool? f = null;

        if (f != null && f.Value) { }

        if (f != null && f.Value && f.Value) { }

        if (f != null && (f.Value)) { }

        if (f != null && !f.Value) { }

        if (f != null && !f.Value && !f.Value) { }

        if (f != null && (!f.Value)) { }

        Foo? x = null;

        var value = default(Foo);

        if (x != null && x.Value == null) { }

        if (x != null && x.Value == value) { }

        if (x != null && x.Value != null) { }

        if (x != null && x.Value != null) { }

        if (x != null && x.Value != value) { }

        if (x != null && x.HasValue.Equals(true)) { }

        if (x != null && (x.Value == null) is object _) { }
    }

    public static bool operator ==(Foo left, Foo right) => left.Equals(right);
    public static bool operator !=(Foo left, Foo right) => !(left == right);
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task TestNoDiagnostic_LogicalOr_NullableType()
        {
            await VerifyNoDiagnosticAsync(@"
struct Foo
{
    const string NonNullConst = ""x"";

    string P { get; }

    void M()
    {
        bool? f = null;

        if (f == null || f.Value) { }

        if (f == null || f.Value && f.Value) { }

        if (f == null || (f.Value)) { }

        if (f == null || !f.Value) { }

        if (f == null || !f.Value && !f.Value) { }

        if (f == null || (!f.Value)) { }

        Foo? x = null;

        var value = default(Foo);

        if (x == null || x.Value.P.Length > 1) { }

        if (x == null || x.Value.P == ""x"") { }

        if (x == null || x.Value.P == NonNullConst) { }

        if (x == null || x.Value.P != null) { }

        if (x == null || x.Value.P is object) { }

        if (x == null || x.Value.P is object _) { }

        if (x == null || x.Value.ToString() == null || x.Value.ToString().ToString() == null) { }


        if (x == null || x.Value == null) { }

        if (x == null || x.Value == value) { }

        if (x == null || x.Value != null) { }

        if (x == null || x.Value != null) { }

        if (x == null || x.Value != value) { }

        if (x == null || x.HasValue.Equals(true)) { }

        if (x == null || (x.Value == null) is object _) { }
    }

    public static bool operator ==(Foo left, Foo right) => left.Equals(right);
    public static bool operator !=(Foo left, Foo right) => !(left == right);
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task TestNoDiagnostic_LogicalAnd_OutParameter()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        Dictionary<int, string> dic = null;

        string value;

if (dic != null && dic.TryGetValue(0, out value)) { }

        if (dic != null && dic.TryGetValue(0, out string value2)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task TestNoDiagnostic_LogicalAnd_OutParameter2()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;

class C
{
    Dictionary<string, string> _dic;

    bool TryGetValue(string key, out string value)
    {
        if (this._dic != null && this._dic.TryGetValue(key, out value))
        {
            return true;
        }

        value = null;
        return false;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task TestNoDiagnostic_LogicalOr_OutParameter()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        Dictionary<int, string> dic = null;

        string value;

        if (dic == null || dic.TryGetValue(0, out value)) { }

        if (dic == null || dic.TryGetValue(0, out string value2)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task TestNoDiagnostic_LogicalAnd_ExpressionTree()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Linq.Expressions;

class C
{
    public void M<T>(Expression<Func<T>> expression)
    {
        string s = null;

        M(() => s != null && s.GetHashCode() == 0);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task TestNoDiagnostic_LogicalOr_ExpressionTree()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Linq.Expressions;

class C
{
    public void M<T>(Expression<Func<T>> expression)
    {
        string s = null;

        M(() => s == null || s.Equals(s));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task TestNoDiagnostic_TypeOverloadsOrOperatorAndImplicitConversionToBooleanDoesNotExist()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public SqlBoolean Value { get; set; }

    void M()
    {
        C x = null;

        if (x == null || x.Value)
        {
        }

        if (x == null || !x.Value)
        {
        }
    }
}

struct SqlBoolean
{
    private readonly bool _boolean;

    public SqlBoolean(bool boolean) => _boolean = boolean;

    public static SqlBoolean operator !(SqlBoolean x) => new SqlBoolean(!x._boolean);

    public static SqlBoolean operator |(SqlBoolean x, SqlBoolean y) => new SqlBoolean(x._boolean | y._boolean);

    public static bool operator true(SqlBoolean x) => x._boolean;

    public static bool operator false(SqlBoolean x) => !x._boolean;

    public static explicit operator bool(SqlBoolean x) => x._boolean;

    public static implicit operator SqlBoolean(bool x) => new SqlBoolean(x);

    public static SqlBoolean operator !=(SqlBoolean x, SqlBoolean y) => !(x == y);

    public static SqlBoolean operator ==(SqlBoolean x, SqlBoolean y) => x._boolean == y._boolean;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task TestNoDiagnostic_PreprocessorDirective()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(string s)
    {
        if (s != null

#if X
                && s != s
#endif
                && !s.Equals(s))
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task TestNoDiagnostic_PointerType()
        {
            await VerifyNoDiagnosticAsync(@"
unsafe class C
{
    public int* P { get; }

    void M()
    {
        var c = new C();

        if (c != null && c.P != null)
        {
        }
    }
}
", options: Options.WithAllowUnsafe(true));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccess)]
        public async Task TestNoDiagnostic_LanguageVersion()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        C x = null;

        if (x != null)
        {
            x.M();
        }
    }
}
", options: WellKnownCSharpTestOptions.Default_CSharp5);
        }
    }
}
