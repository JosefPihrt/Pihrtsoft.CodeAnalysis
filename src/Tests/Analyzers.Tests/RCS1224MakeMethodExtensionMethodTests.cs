// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1224MakeMethodExtensionMethodTests : AbstractCSharpDiagnosticVerifier<MakeMethodExtensionMethodAnalyzer, MemberDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.MakeMethodExtensionMethod;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task Test_Accessibility_ImplicitlyInternal()
        {
            await VerifyDiagnosticAndFixAsync(@"
static class FooExtensions
{
    public static string [|M|](string s) => s;
}
", @"
static class FooExtensions
{
    public static string M(this string s) => s;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task Test_Accessibility_Internal()
        {
            await VerifyDiagnosticAndFixAsync(@"
internal static class FooExtensions
{
    internal static string [|M|](string s) => s;
}
", @"
internal static class FooExtensions
{
    internal static string M(this string s) => s;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task Test_Accessibility_Public()
        {
            await VerifyDiagnosticAndFixAsync(@"
public static class FooExtensions
{
    public static string [|M|](string s) => s;
}
", @"
public static class FooExtensions
{
    public static string M(this string s) => s;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task Test_Modifier_In_ValueType()
        {
            await VerifyDiagnosticAndFixAsync(@"
public static class FooExtensions
{
    public static void [|M|](in int i) { }
}
", @"
public static class FooExtensions
{
    public static void M(this in int i) { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task Test_Modifier_Ref_ValueType()
        {
            await VerifyDiagnosticAndFixAsync(@"
public static class FooExtensions
{
    public static void [|M|](ref int i) { }
}
", @"
public static class FooExtensions
{
    public static void M(this ref int i) { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task Test_Modifier_Ref_StructConstraint()
        {
            await VerifyDiagnosticAndFixAsync(@"
public static class FooExtensions
{
    public static void [|M|]<T>(ref T i) where T: struct { }
}
", @"
public static class FooExtensions
{
    public static void M<T>(this ref T i) where T: struct { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task Test_Modifier_Ref_UnmanagedConstraint()
        {
            await VerifyDiagnosticAndFixAsync(@"
public static class FooExtensions
{
    public static void [|M|]<T>(ref T i) where T: unmanaged { }
}
", @"
public static class FooExtensions
{
    public static void M<T>(this ref T i) where T: unmanaged { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task TestNoDiagnostic_NoSuffix()
        {
            await VerifyNoDiagnosticAsync(@"
public static class Foo
{
    public static string M(string s) => s;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task TestNoDiagnostic_NotStatic()
        {
            await VerifyNoDiagnosticAsync(@"
public class FooExtensions
{
    public string M(string s) => s;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task TestNoDiagnostic_NestedClass()
        {
            await VerifyNoDiagnosticAsync(@"
public static class Foo
{
    public static class FooExtensions
    {
        public static string M(string s) => s;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task TestNoDiagnostic_PrivateMethod()
        {
            await VerifyNoDiagnosticAsync(@"
public static class FooExtensions
{
    private static string M(string s) => s;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task TestNoDiagnostic_Modifier_This()
        {
            await VerifyNoDiagnosticAsync(@"
public static class FooExtensions
{
    public static string M(this string s) => s;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task TestNoDiagnostic_Modifier_In_ReferenceType()
        {
            await VerifyNoDiagnosticAsync(@"
public static class FooExtensions
{
    public static void M(in object p) { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task TestNoDiagnostic_Modifier_In_StructConstraint()
        {
            await VerifyNoDiagnosticAsync(@"
public static class FooExtensions
{
        public static void M<T>(in T t) where T : struct { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task TestNoDiagnostic_Modifier_In_UnmanagedConstraint()
        {
            await VerifyNoDiagnosticAsync(@"
public static class FooExtensions
{
        public static void M<T>(in T t) where T : unmanaged { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task TestNoDiagnostic_Modifier_Ref_ReferenceType()
        {
            await VerifyNoDiagnosticAsync(@"
public static class FooExtensions
{
        public static void M(ref object p) { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task TestNoDiagnostic_ParameterHasDefaultValue()
        {
            await VerifyNoDiagnosticAsync(@"
public static class FooExtensions
{
        public static void M(object p = null) { }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task TestNoDiagnostic_PointerType()
        {
            await VerifyNoDiagnosticAsync(@"
public static class FooExtensions
{
        public static unsafe void M(int* p) { }
}
", options: Options.WithAllowUnsafe(true));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.MakeMethodExtensionMethod)]
        public async Task TestNoDiagnostic_Params()
        {
            await VerifyNoDiagnosticAsync(@"
public static class FooExtensions
{
        public static void M(params object[] p) { }
}
");
        }
    }
}
