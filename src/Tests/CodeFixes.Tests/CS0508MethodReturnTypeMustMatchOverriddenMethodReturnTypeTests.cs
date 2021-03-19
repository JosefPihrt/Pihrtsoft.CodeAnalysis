// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0508MethodReturnTypeMustMatchOverriddenMethodReturnTypeTests : AbstractCSharpCompilerDiagnosticFixVerifier<MemberDeclarationCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.MethodReturnTypeMustMatchOverriddenMethodReturnType;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.MethodReturnTypeMustMatchOverriddenMethodReturnType)]
        public async Task TestFix()
        {
            await VerifyFixAsync(@"
class C
{
    public override object ToString()
    {
    }
}
", @"
class C
{
    public override string ToString()
    {
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
