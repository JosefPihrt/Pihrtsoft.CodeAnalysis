// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0029CannotImplicitlyConvertTypeTests : AbstractCSharpCompilerDiagnosticFixVerifier<ExpressionCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.CannotImplicitlyConvertType;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.CannotImplicitlyConvertType)]
        public async Task Test_RemoveAssignmentOfVoidExpression()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        object x = null;
        x = M();
    }
}
", @"
class C
{
    void M()
    {
        object x = null;
        M();
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, "RemoveAssignment"));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.CannotImplicitlyConvertType)]
        public async Task Test_ChangeReturnTypeAccordingToReturnExpression()
        {
            await VerifyFixAsync(@"
class C
{
    int M()
    {
        return """";
    }
}
", @"
class C
{
    string M()
    {
        return """";
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.ChangeMemberTypeAccordingToReturnExpression));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.CannotImplicitlyConvertType)]
        public async Task Test_ChangeReturnTypeAccordingToReturnExpression_ExpressionBody()
        {
            await VerifyFixAsync(@"
class C
{
    int M() => """";
}
", @"
class C
{
    string M() => """";
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.ChangeMemberTypeAccordingToReturnExpression));
        }
    }
}
