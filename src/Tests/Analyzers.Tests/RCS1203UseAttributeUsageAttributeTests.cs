// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1203UseAttributeUsageAttributeTests : AbstractCSharpDiagnosticVerifier<UseAttributeUsageAttributeAnalyzer, ClassDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseAttributeUsageAttribute;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseAttributeUsageAttribute)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class [|FooAttribute|] : Attribute
{
}
", @"
using System;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
class FooAttribute : Attribute
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseAttributeUsageAttribute)]
        public async Task TestNoDiagnostic_AttributeUsageAttributeAlreadyExistsOrIsInherited()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

[AttributeUsageAttribute(AttributeTargets.All, AllowMultiple = false)]
class FooAttribute : Attribute
{
}

class BarAttribute : FooAttribute
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseAttributeUsageAttribute)]
        public async Task TestNoDiagnostic_DoesNotInheritFromAttribute()
        {
            await VerifyNoDiagnosticAsync(@"
class FooAttribute
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseAttributeUsageAttribute)]
        public async Task TestNoDiagnostic_NameDoesNotEndWithAttribute()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class Foo : Attribute
{
}
");
        }
    }
}
