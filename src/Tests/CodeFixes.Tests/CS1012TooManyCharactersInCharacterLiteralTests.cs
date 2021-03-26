// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS1012TooManyCharactersInCharacterLiteralTests : AbstractCSharpCompilerDiagnosticFixVerifier<LiteralExpressionCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.TooManyCharactersInCharacterLiteral;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.TooManyCharactersInCharacterLiteral)]
        public async Task Test()
        {
            await VerifyFixAsync(@"
class C
{
    void M()
    {
        var x = 'a\'\""\nb';
    }
}
", @"
class C
{
    void M()
    {
        var x = ""a\'\""\nb"";
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
