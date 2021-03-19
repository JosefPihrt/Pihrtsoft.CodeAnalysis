// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0191ReadOnlyFieldCannotBeAssignedToTests : AbstractCSharpCompilerDiagnosticFixVerifier<ExpressionCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.ReadOnlyFieldCannotBeAssignedTo;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.ReadOnlyFieldCannotBeAssignedTo)]
        public async Task Test()
        {
            await VerifyFixAsync(@"
class C
{
    private readonly string _f = null;

    void M()
    {
        _f = null;
    }
}
", @"
class C
{
    private string _f = null;

    void M()
    {
        _f = null;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
