// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1202AvoidNullReferenceExceptionTests2 : AbstractCSharpDiagnosticVerifier<AvoidNullReferenceExceptionAnalyzer, AvoidNullReferenceExceptionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AvoidNullReferenceException;

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNullReferenceException)]
        [InlineData("(x as string)[|.|]ToString()", "(x as string)?.ToString()")]
        [InlineData("(x as string)[|[[|]0]", "(x as string)?[0]")]
        public async Task Test_AsExpression(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        object x = null;
        var y = [||];
    }
}
", source, expected);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNullReferenceException)]
        public async Task Test_AwaitExpression()
        {
            await VerifyDiagnosticAsync(@"
using System.Threading.Tasks;

static class C
{
    public static async Task M(object x)
    {
        await (x as string)[|.|]M2().ConfigureAwait(true);
    }

    public static async Task M2(this string s) => await Task.CompletedTask;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNullReferenceException)]
        public async Task TestNoFix_AwaitExpression()
        {
            await VerifyDiagnosticAndNoFixAsync(@"
using System.Threading.Tasks;

static class C
{
    public static async Task M(object x)
    {
        await (x as string)[|.|]M2().ConfigureAwait(true);
    }

    public static async Task M2(this string s) => await Task.CompletedTask;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNullReferenceException)]
        public async Task TestNoDiagnostic_UnconstrainedTypeParameter()
        {
            await VerifyNoDiagnosticAsync(@"
class C<T>
{
    T P { get; }

    void M()
    {
        object x = null;

        x = (x as C<T>).P;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNullReferenceException)]
        public async Task TestNoDiagnostic_UnconstrainedTypeParameter2()
        {
            await VerifyNoDiagnosticAsync(@"
class C<T, U> where T : B<U>
{
    T P { get; }

    void M()
    {
        object x = null;

        x = (x as C<T, U>).P.M();
    }
}

class B<T>
{
    public T M() => default;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNullReferenceException)]
        public async Task TestNoDiagnostic_ExtensionMethod()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        object x = null;

        (x as C).EM();
    }
}

static class E
{
    public static C EM(this C c) => c;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNullReferenceException)]
        public async Task TestNoDiagnostic_ThisCastedToItsInterface()
        {
            await VerifyNoDiagnosticAsync(@"
interface I
{
    void M();
}

class C : I
{
    public void M() 
    {
        (this as I).M();
    }
}
");
        }
    }
}
