// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS1003SyntaxErrorCharExpectedTests : AbstractCSharpCompilerDiagnosticFixVerifier<SyntaxErrorCharExpectedCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.SyntaxErrorCharExpected;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.SyntaxErrorCharExpected)]
        public async Task Test_MissingCommaInInitializer_Singleline()
        {
            await VerifyFixAsync(@"
class C
{
    public void M()
    {
        string s = null;

        var items = new string[] { s s s };
    }
}
", @"
class C
{
    public void M()
    {
        string s = null;

        var items = new string[] { s, s, s };
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.SyntaxErrorCharExpected)]
        public async Task Test_MissingCommaInInitializer_Multiline()
        {
            await VerifyFixAsync(@"
class C
{
    public void M()
    {
        string s = null;

        var items = new string[]
        {
            s
            s
            s
        };
    }
}
", @"
class C
{
    public void M()
    {
        string s = null;

        var items = new string[]
        {
            s,
            s,
            s
        };
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.SyntaxErrorCharExpected)]
        public async Task Test_MissingCommaBetweenEnumMembers()
        {
            await VerifyFixAsync(@"
enum E
{
    A = 1
    B = 2
    C = 3
}
", @"
enum E
{
    A = 1,
    B = 2,
    C = 3
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
