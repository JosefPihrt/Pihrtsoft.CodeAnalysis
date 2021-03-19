// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0119NameIsNotValidInGivenContextTests : AbstractCSharpCompilerDiagnosticFixVerifier<SimpleNameCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.NameIsNotValidInGivenContext;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.NameIsNotValidInGivenContext)]
        public async Task Test()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        string s = null;
        s = s.ToString.ToString();
    }
}
", @"
class C
{
    void M()
    {
        string s = null;
        s = s.ToString().ToString();
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
