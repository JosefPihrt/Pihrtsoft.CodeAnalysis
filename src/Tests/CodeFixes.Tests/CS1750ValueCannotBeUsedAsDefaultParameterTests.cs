
// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS1750ValueCannotBeUsedAsDefaultParameterTests : AbstractCSharpCompilerDiagnosticFixVerifier<TokenCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.ValueCannotBeUsedAsDefaultParameter;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.ValueCannotBeUsedAsDefaultParameter)]
        public async Task Test_ChangeParameterType()
        {
            await VerifyFixAsync(@"
class C
{
    void M(string p = 0)
    {
    }
}
", @"
class C
{
    void M(int p = 0)
    {
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
