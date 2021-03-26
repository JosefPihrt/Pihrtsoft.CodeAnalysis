// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1102MakeClassStaticTests : AbstractCSharpDiagnosticVerifier<MakeClassStaticAnalyzer, ClassDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.MakeClassStatic;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassStatic)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
class [|C|]
{
    static void M()
    {
    }
}
", @"
static class C
{
    static void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassStatic)]
        public async Task Test2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

public class [|C|]
{
    public const string K = null;

    private static readonly string _f;

    public static string P { get; set; }

    public static event EventHandler E;

    public static void M()
    {
    }

    public class C2
    {
    }

    public struct ST
    {
    }

    public interface I
    {
    }

    public delegate void D();

    public enum EM
    {
    }
}
", @"
using System;

public static class C
{
    public const string K = null;

    private static readonly string _f;

    public static string P { get; set; }

    public static event EventHandler E;

    public static void M()
    {
    }

    public class C2
    {
    }

    public struct ST
    {
    }

    public interface I
    {
    }

    public delegate void D();

    public enum EM
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassStatic)]
        public async Task TestNoDiagnostic_SealedClass()
        {
            await VerifyNoDiagnosticAsync(@"
sealed class C
{
    const string K = null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassStatic)]
        public async Task TestNoDiagnostic_ImplementsInterface()
        {
            await VerifyNoDiagnosticAsync(@"
class C : I
{
    const string K = null;
}

interface I
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassStatic)]
        public async Task TestNoDiagnostic_TypeArgument()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;

class C
{
    static void M()
    {
        var x = new List<C>();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeClassStatic)]
        public async Task TestNoDiagnostic_ReturnType()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    static C M()
    {
        return null;
    }
}
");
        }
    }
}
