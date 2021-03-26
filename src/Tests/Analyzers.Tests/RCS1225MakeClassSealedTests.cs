// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1225MakeClassSealedTests : AbstractCSharpDiagnosticVerifier<MakeClassSealedAnalyzer, ClassDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.MakeClassSealed;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassSealed)]
        public async Task Test_ClassWithoutAccessibilityModifier()
        {
            await VerifyDiagnosticAndFixAsync(@"
class [|C|]
{
    private C()
    {
    }
}
", @"
sealed class C
{
    private C()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassSealed)]
        public async Task Test_ClassWithAccessibilityModifier()
        {
            await VerifyDiagnosticAndFixAsync(@"
public class [|C|]
{
    private C()
    {
    }
}
", @"
public sealed class C
{
    private C()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassSealed)]
        public async Task Test_ClassWithTwoConstructors()
        {
            await VerifyDiagnosticAndFixAsync(@"
public class [|C|]
{
    private C()
    {
    }

    private C(object p)
    {
    }
}
", @"
public sealed class C
{
    private C()
    {
    }

    private C(object p)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassSealed)]
        public async Task TestNoDiagnostic_StaticClass()
        {
            await VerifyNoDiagnosticAsync(@"
static class C
{
    static C()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassSealed)]
        public async Task TestNoDiagnostic_SealedClass()
        {
            await VerifyNoDiagnosticAsync(@"
sealed class C
{
    private C()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassSealed)]
        public async Task TestNoDiagnostic_ProtectedConstructor()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    protected C()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassSealed)]
        public async Task TestNoDiagnostic_NoExplicitConstructor()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassSealed)]
        public async Task TestNoDiagnostic_VirtualMember()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    private C()
    {
    }

    protected virtual void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassSealed)]
        public async Task TestNoDiagnostic_ContainsDerivedClass()
        {
            await VerifyNoDiagnosticAsync(@"
class B
{
    private B()
    {
    }

    class C : B
    {
        C() : base()
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassSealed)]
        public async Task TestNoDiagnostic_ContainsDerivedClass2()
        {
            await VerifyNoDiagnosticAsync(@"
class B
{
    private B()
    {
    }

    class C
    {
        class D : B
        {
            D() : base()
            {
            }
        }
    }
}
");
        }
    }
}
