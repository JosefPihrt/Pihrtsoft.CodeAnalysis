// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0131LeftHandSideOfAssignmentMustBeVariablePropertyOrIndexerTests : AbstractCSharpCompilerDiagnosticFixVerifier<ExpressionCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.LeftHandSideOfAssignmentMustBeVariablePropertyOrIndexer;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.LeftHandSideOfAssignmentMustBeVariablePropertyOrIndexer)]
        public async Task Test()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        const string s = null;
        s = null;
    }
}
", @"
class C
{
    void M()
    {
        string s = null;
        s = null;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
