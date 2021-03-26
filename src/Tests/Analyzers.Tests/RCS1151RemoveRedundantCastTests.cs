// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1151RemoveRedundantCastTests : AbstractCSharpDiagnosticVerifier<RemoveRedundantCastAnalyzer, RemoveRedundantCastCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.RemoveRedundantCast;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        public async Task Test_CastToDerivedType()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        var c = new C();

        var s = ([|(B)|]c).P;

    }

    public string P { get; set; }
}

class B : C
{
}
", @"
class C
{
    void M()
    {
        var c = new C();

        var s = c.P;

    }

    public string P { get; set; }
}

class B : C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        public async Task Test_CastToDerivedType_ConditionalAccess()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        var c = new C();

        var s = ([|(B)|]c)?.P;

    }

    public string P { get; set; }
}

class B : C
{
}
", @"
class C
{
    void M()
    {
        var c = new C();

        var s = c?.P;

    }

    public string P { get; set; }
}

class B : C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        public async Task Test_CastToImplementedInterface()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        Dictionary<int, string> dic = null;

        string s = ([|(IDictionary<int, string>)|]dic)[0];
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        Dictionary<int, string> dic = null;

        string s = dic[0];
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        public async Task Test_CastToImplementedInterface_ConditionalAccess()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        Dictionary<int, string> dic = null;

        string s = ([|(IDictionary<int, string>)|]dic)?[0];
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        Dictionary<int, string> dic = null;

        string s = dic?[0];
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        public async Task Test_CastToIDisposable()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        ([|(IDisposable)|]new Disposable()).Dispose();
    }
}

class Disposable : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
", @"
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        new Disposable().Dispose();
    }
}

class Disposable : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        public async Task Test_Accessibility()
        {
            await VerifyDiagnosticAndFixAsync(@"
class B
{
    private void M(B b)
    {
        ([|(C)|]b).Protected();

        ([|(C)|]b).PrivateProtected();

        ([|(C)|]b).ProtectedInternal();
    }

    private class C : B
    {
        private void M2(B b)
        {
            ([|(C)|]b).Protected();

            ([|(C)|]b).PrivateProtected();

            ([|(C)|]b).ProtectedInternal();
        }
    }

    protected void Protected() { }

    private protected void PrivateProtected() { }

    protected internal void ProtectedInternal() { }
}
", @"
class B
{
    private void M(B b)
    {
        b.Protected();

        b.PrivateProtected();

        b.ProtectedInternal();
    }

    private class C : B
    {
        private void M2(B b)
        {
            b.Protected();

            b.PrivateProtected();

            b.ProtectedInternal();
        }
    }

    protected void Protected() { }

    private protected void PrivateProtected() { }

    protected internal void ProtectedInternal() { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        public async Task Test_Accessibility_ProtectedInternal()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C : B
{
    public static void M()
    {
        var b = default(B);

        ([|(C)|]b).ProtectedInternal();
    }
}

class B
{
    protected internal void ProtectedInternal() { }
}
", @"
class C : B
{
    public static void M()
    {
        var b = default(B);

        b.ProtectedInternal();
    }
}

class B
{
    protected internal void ProtectedInternal() { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        public async Task TestNoDiagnostic_NotAccessible()
        {
            await VerifyNoDiagnosticAsync(@"
class B
{
    protected void Protected() { }

    private protected void PrivateProtected() { }
}

class C : B
{
    void M(B b)
    {
        ((C)b).Protected();

        ((C)b).PrivateProtected();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        public async Task TestNoDiagnostic_ExplicitImplementation()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Collections;
using System.Collections.Generic;

class C
{
    static void M()
    {
        var e1 = ((IEnumerable<string>)new EnumerableOfString()).GetEnumerator();
    }
}

class EnumerableOfString : IEnumerable<string>
{
    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

class DerivedEnumerableOfString : EnumerableOfString
{
}

class ExplicitDisposable : IDisposable
{
    void IDisposable.Dispose()
    {
        throw new NotImplementedException();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        public async Task TestNoDiagnostic_ExplicitImplementationOfGenericMethod()
        {
            await VerifyNoDiagnosticAsync(@"
interface IC
{
    void M<T>(T t);
}

class C : IC
{
    void IC.M<T>(T t)
    {
        var c = new C();

        ((IC)c).M(c);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        public async Task TestNoDiagnostic_CastToDerivedType()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        var c = new C();

        var s = ((B)c).P;

    }
}

class B : C
{
    public string P { get; set; }
}
");
        }

        //TODO: Add test for default interface implementation (RCS1151)
        internal async Task TestNoDiagnostic_DefaultInterfaceImplementation()
        {
            await VerifyNoDiagnosticAsync(@"
interface IC
{
    void M()
    {
    }
}

class C : IC
{
    void M2()
    {
        var c = new C();

        ((IC)c).M();
    }
}
");
        }
    }
}
