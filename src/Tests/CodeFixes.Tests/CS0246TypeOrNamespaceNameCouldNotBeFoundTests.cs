// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0246TypeOrNamespaceNameCouldNotBeFoundTests : AbstractCSharpCompilerDiagnosticFixVerifier<SimpleNameCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.TypeOrNamespaceNameCouldNotBeFound;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.TypeOrNamespaceNameCouldNotBeFound)]
        public async Task Test_ChangeType_Field()
        {
            await VerifyFixAsync(@"
class C
{
    private x F = default(C);
}
", @"
class C
{
    private C F = default(C);
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.ChangeMemberTypeAccordingToReturnExpression));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.TypeOrNamespaceNameCouldNotBeFound)]
        public async Task Test_ChangeType_Method()
        {
            await VerifyFixAsync(@"
class C
{
    x M()
    {
        return default(C);
    }
}
", @"
class C
{
    C M()
    {
        return default(C);
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.ChangeMemberTypeAccordingToReturnExpression));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.TypeOrNamespaceNameCouldNotBeFound)]
        public async Task Test_ChangeType_Method_ExpressionBody()
        {
            await VerifyFixAsync(@"
class C
{
    x M() => default(C);
}
", @"
class C
{
    C M() => default(C);
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.ChangeMemberTypeAccordingToReturnExpression));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.TypeOrNamespaceNameCouldNotBeFound)]
        public async Task Test_ChangeType_LocalFunction()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        x LocalFunction()
        {
            return default(C);
        }
    }
}
", @"
class C
{
    void M()
    {
        C LocalFunction()
        {
            return default(C);
        }
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.ChangeMemberTypeAccordingToReturnExpression));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.TypeOrNamespaceNameCouldNotBeFound)]
        public async Task Test_ChangeType_LocalFunction_ExpressionBody()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        x LocalFunction() => default(C);
    }
}
", @"
class C
{
    void M()
    {
        C LocalFunction() => default(C);
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.ChangeMemberTypeAccordingToReturnExpression));
        }
    }
}
