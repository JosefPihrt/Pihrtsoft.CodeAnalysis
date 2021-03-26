// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1014UseImplicitlyTypedArrayTests : AbstractCSharpDiagnosticVerifier<UseExplicitlyTypedArrayOrViceVersaAnalyzer, UseExplicitlyTypedArrayOrViceVersaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseExplicitlyTypedArrayOrViceVersa;

        public override CSharpTestOptions Options
        {
            get { return base.Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.UseImplicitlyTypedArray); }
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitlyTypedArrayOrViceVersa)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        var x = new [|string|][] { """" };
    }
}
", @"
class C
{
    void M()
    {
        var x = new[] { """" };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitlyTypedArrayOrViceVersa)]
        public async Task Test_TypeIsNotObvious()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        var x = new [|string|][] { M2() };
    }

    string M2() => default;
}
", @"
class C
{
    void M()
    {
        var x = new[] { M2() };
    }

    string M2() => default;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitlyTypedArrayOrViceVersa)]
        public async Task Test_TypeIsObvious()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        var x = new [|string|][] { """" };
    }
}
", @"
class C
{
    void M()
    {
        var x = new[] { """" };
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.UseImplicitlyTypedArrayWhenTypeIsObvious));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitlyTypedArrayOrViceVersa)]
        public async Task Test_TypesAreNotEqual()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C : A
{
    void M()
    {
        var x = new [|A|][]
        {
            default(B),
            default(C)
        };
    }
}

class A
{
}

class B : A
{
}
", @"
class C : A
{
    void M()
    {
        var x = new[]
        {
            default(B),
            (A)default(C)
        };
    }
}

class A
{
}

class B : A
{
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.UseImplicitlyTypedArrayWhenTypeIsObvious));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitlyTypedArrayOrViceVersa)]
        public async Task Test_NestedArray()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string[][] _f = new [|string[]|][]
    {
        new[] { """" },
    };
}
", @"
class C
{
    string[][] _f = new[]
    {
        new[] { """" },
    };
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitlyTypedArrayOrViceVersa)]
        public async Task TestNoDiagnostic_TypeIsObvious()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        var x = new[]
        {
            new[] { """", new string('a', 1) },
            new[] { new string('a', 1), """" }
        };
    }

    string M2() => null;
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.UseImplicitlyTypedArrayWhenTypeIsObvious));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseExplicitlyTypedArrayOrViceVersa)]
        public async Task TestNoDiagnostic_NoInitializer()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        var items = new string[0];
    }
}
");
        }
    }
}
