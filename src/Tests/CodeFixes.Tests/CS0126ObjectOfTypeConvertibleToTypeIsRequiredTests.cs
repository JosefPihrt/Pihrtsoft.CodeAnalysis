// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS0126ObjectOfTypeConvertibleToTypeIsRequiredTests : AbstractCSharpCompilerDiagnosticFixVerifier<TokenCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.ObjectOfTypeConvertibleToTypeIsRequired;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.ObjectOfTypeConvertibleToTypeIsRequired)]
        public async Task Test()
        {
            await VerifyFixAsync(@"
using System;

class C
{
    DateTime M()
    {
        return;
    }
}
", @"
using System;

class C
{
    DateTime M()
    {
        return default;
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
