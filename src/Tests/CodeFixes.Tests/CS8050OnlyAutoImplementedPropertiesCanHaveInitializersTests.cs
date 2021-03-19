// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public class CS8050OnlyAutoImplementedPropertiesCanHaveInitializersTests : AbstractCSharpCompilerDiagnosticFixVerifier<RemovePropertyOrFieldInitializerCodeFixProvider>
    {
        public override string DiagnosticId { get; } = CompilerDiagnosticIdentifiers.OnlyAutoImplementedPropertiesCanHaveInitializers;

        [Fact, Trait(Traits.CodeFix, CompilerDiagnosticIdentifiers.OnlyAutoImplementedPropertiesCanHaveInitializers)]
        public async Task Test()
        {
            await VerifyFixAsync(@"
class C
{
    private string _p;

    public string P
    {
        get { return _p; }
        set { _p = value; }
    } = "";
}
", @"
class C
{
    private string _p;

    public string P
    {
        get { return _p; }
        set { _p = value; }
    }
}
", equivalenceKey: EquivalenceKey.Create(DiagnosticId, CodeFixIdentifiers.RemovePropertyOrFieldInitializer));
        }
    }
}
