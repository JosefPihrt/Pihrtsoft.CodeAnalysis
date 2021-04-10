// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0029AddNewLineBeforeConstructorInitializerTests : AbstractCSharpDiagnosticVerifier<AddNewLineBeforeConstructorInitializerAnalyzer, MemberDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AddNewLineBeforeConstructorInitializer;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeConstructorInitializer)]
        public async Task Test_ThisInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    C(object p1)
    {
    }

    C(object p1, object p2) [||]: this(p1)
    {
    }
}
", @"
class C
{
    C(object p1)
    {
    }

    C(object p1, object p2)
        : this(p1)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeConstructorInitializer)]
        public async Task Test_ThisInitializer_Multiline()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    C(object p1)
    {
    }

    C(
        object p1,
        object p2) [||]: this(p1)
    {
    }
}
", @"
class C
{
    C(object p1)
    {
    }

    C(
        object p1,
        object p2)
        : this(p1)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeConstructorInitializer)]
        public async Task Test_BaseInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
class B
{
    protected B(object p1)
    {
    }
}
class C : B
{
    C(object p1, object p2) [||]: base(p1)
    {
    }
}
", @"
class B
{
    protected B(object p1)
    {
    }
}
class C : B
{
    C(object p1, object p2)
        : base(p1)
    {
    }
}
");
        }
    }
}
