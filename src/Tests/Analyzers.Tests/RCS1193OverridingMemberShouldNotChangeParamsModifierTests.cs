// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1193OverridingMemberShouldNotChangeParamsModifierTests : AbstractCSharpDiagnosticVerifier<OverridingMemberShouldNotChangeParamsModifierAnalyzer, ParameterCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.OverridingMemberShouldNotChangeParamsModifier;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OverridingMemberShouldNotChangeParamsModifier)]
        public async Task Test_WithoutParams()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C : B
{
    public override void M([|object[] x|])
    {
    }
}

class B
{
    public virtual void M(params object[] x)
    {
    }
}
", @"
class C : B
{
    public override void M(params object[] x)
    {
    }
}

class B
{
    public virtual void M(params object[] x)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OverridingMemberShouldNotChangeParamsModifier)]
        public async Task Test_WithParams()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C : B
{
    public override void M([|params object[] x|])
    {
    }
}

class B
{
    public virtual void M(object[] x)
    {
    }
}
", @"
class C : B
{
    public override void M(object[] x)
    {
    }
}

class B
{
    public virtual void M(object[] x)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OverridingMemberShouldNotChangeParamsModifier)]
        public async Task Test_WithoutParams_Indexer()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C : B
{
    public override string this[bool x, [|string[] y|]] => base[x, y];
}

class B
{
    public virtual string this[bool x, params string[] y] => null;
}
", @"
class C : B
{
    public override string this[bool x, params string[] y] => base[x, y];
}

class B
{
    public virtual string this[bool x, params string[] y] => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OverridingMemberShouldNotChangeParamsModifier)]
        public async Task Test_WithParams_Indexer()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C : B
{
    public override string this[int x, [|params string[] y|]] => base[x, y];
}

class B
{
    public virtual string this[int x, string[] y] => null;
}
", @"
class C : B
{
    public override string this[int x, string[] y] => base[x, y];
}

class B
{
    public virtual string this[int x, string[] y] => null;
}
");
        }
    }
}
