// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0021AddNewLineAfterOpeningBraceOfBlockTests : AbstractCSharpDiagnosticVerifier<AddNewLineAfterOpeningBraceOfBlockAnalyzer, BlockCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddNewLineAfterOpeningBraceOfBlock;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfBlock)]
        public async Task Test_Constructor()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    public C() {[||] M(); }

    public void M()
    {
    }
}
", @"
class C
{
    public C()
    {
        M();
    }

    public void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfBlock)]
        public async Task Test_Destructor()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    ~C() {[||] M(); }

    public void M()
    {
    }
}
", @"
class C
{
    ~C()
    {
        M();
    }

    public void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfBlock)]
        public async Task Test_Method()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    public void M(object p = null) {[||] if (p == null) {[||] throw new ArgumentNullException(nameof(p)); } }
}
", @"
using System;

class C
{
    public void M(object p = null)
    {
        if (p == null)
        {
            throw new ArgumentNullException(nameof(p));
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfBlock)]
        public async Task Test_ExplicitOperator()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    public static explicit operator C(string value) {[||] return new C(); }
}
", @"
class C
{
    public static explicit operator C(string value)
    {
        return new C();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfBlock)]
        public async Task Test_Operator()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    public static C operator !(C value) {[||] return new C(); }
}
", @"
class C
{
    public static C operator !(C value)
    {
        return new C();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfBlock)]
        public async Task TestNoDiagnostic_SingleLineAccessorList()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Collections.Generic;

class C
{
    private string _p;
    private readonly List<string> _items;

    public event EventHandler E { add { M(); } remove { M(); } }

    public string P { get { return _p; } set { _p = value; } }

    public string this[int index] { get { return _items[index]; } set { _items[index] = value; } }

    public void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfBlock)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Collections.Generic;

class C
{
    private string _p;
    private readonly List<string> _items;

    public C() { }

    ~C() { }

    public event EventHandler E
    {
        add { M(); }
        remove { M(); }
    }

    public string P
    {
        get { return _p; }
        set { _p = value; }
    }

    public string this[int index]
    {
        get { return _items[index]; }
        set { _items[index] = value; }
    }

    public void M()
    {
        Action<object> action1 = f => { };
        Action<object> action2 = (f) => { };
        Action<object> action3 = delegate { };
    }

    public static explicit operator C(string value)
    {
        return new C();
    }

    public static explicit operator string(C value)
    {
        return null;
    }

    public static C operator !(C value)
    {
        return new C();
    }

    public enum EM { }

    public interface I { }

    public struct S { }

    public class C2 { }
}
");
        }
    }
}
