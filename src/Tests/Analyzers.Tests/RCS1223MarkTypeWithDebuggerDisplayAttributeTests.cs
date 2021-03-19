// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1223MarkTypeWithDebuggerDisplayAttributeTests : AbstractCSharpDiagnosticVerifier<MarkTypeWithDebuggerDisplayAttributeAnalyzer, MarkTypeWithDebuggerDisplayAttributeCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.MarkTypeWithDebuggerDisplayAttribute;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MarkTypeWithDebuggerDisplayAttribute)]
        public async Task Test_PublicClass()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Diagnostics;

public class [|C|]
{
}
", @"
using System.Diagnostics;

[DebuggerDisplay(""{DebuggerDisplay,nq}"")]
public class C
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MarkTypeWithDebuggerDisplayAttribute)]
        public async Task Test_PublicClassWithDocComment()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Diagnostics;

/// <summary></summary>
public class [|C|]
{
}
", @"
using System.Diagnostics;

/// <summary></summary>
[DebuggerDisplay(""{DebuggerDisplay,nq}"")]
public class C
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MarkTypeWithDebuggerDisplayAttribute)]
        public async Task Test_PublicClassWithAttribute()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Diagnostics;

[Obsolete]
public class [|C|]
{
}
", @"
using System;
using System.Diagnostics;

[Obsolete]
[DebuggerDisplay(""{DebuggerDisplay,nq}"")]
public class C
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MarkTypeWithDebuggerDisplayAttribute)]
        public async Task Test_PublicClassWithDocCommentAndAttribute()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Diagnostics;

/// <summary>
/// 
/// </summary>
[Obsolete]
public class [|C|]
{
}
", @"
using System;
using System.Diagnostics;

/// <summary>
/// 
/// </summary>
[Obsolete]
[DebuggerDisplay(""{DebuggerDisplay,nq}"")]
public class C
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MarkTypeWithDebuggerDisplayAttribute)]
        public async Task Test_PublicStructWithDocCommentAndAttribute()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Diagnostics;

/// <summary>
/// 
/// </summary>
[Obsolete]
public struct [|C|]
{
}
", @"
using System;
using System.Diagnostics;

/// <summary>
/// 
/// </summary>
[Obsolete]
[DebuggerDisplay(""{DebuggerDisplay,nq}"")]
public struct C
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MarkTypeWithDebuggerDisplayAttribute)]
        public async Task TestNoDiagnostic_ClassWithDebuggerDisplayAttribute()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Diagnostics;

[DebuggerDisplay("""")]
public class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MarkTypeWithDebuggerDisplayAttribute)]
        public async Task TestNoDiagnostic_ClassWithDebuggerDisplayAttributeOnBaseClass()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Diagnostics;

[DebuggerDisplay("""")]
public class B
{
}

public class C : B
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MarkTypeWithDebuggerDisplayAttribute)]
        public async Task TestNoDiagnostic_StaticClass()
        {
            await VerifyNoDiagnosticAsync(@"
static class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MarkTypeWithDebuggerDisplayAttribute)]
        public async Task TestNoDiagnostic_Interface()
        {
            await VerifyNoDiagnosticAsync(@"
public interface IC
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MarkTypeWithDebuggerDisplayAttribute)]
        public async Task TestNoDiagnostic_NonPubliclyVisibleType()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Diagnostics;

[DebuggerDisplay("""")]
public class C
{
    internal class IC
    {
        public class C { }
        protected internal class PIC { }
        protected class DC { }
    }

    [DebuggerDisplay("""")]
    private class PC
    {
        public class C { }
        protected internal class PIC { }
        protected class DC { }
    }
}

internal class IC
{
    public class Foo { }
    protected internal class FooProtectedInternal { }
    protected class FooProtected { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MarkTypeWithDebuggerDisplayAttribute)]
        public async Task TestNoDiagnostic_NonPubliclyVisibleType_PrivateProtecteed()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Diagnostics;

[DebuggerDisplay("""")]
public class C
{
    private protected class PPC
    {
        public class C { }
        protected internal class PIC { }
        protected class DC { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MarkTypeWithDebuggerDisplayAttribute)]
        public async Task TestNoDiagnostic_AbstractClass()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Diagnostics;

public abstract class C
{
}
");
        }
    }
}
