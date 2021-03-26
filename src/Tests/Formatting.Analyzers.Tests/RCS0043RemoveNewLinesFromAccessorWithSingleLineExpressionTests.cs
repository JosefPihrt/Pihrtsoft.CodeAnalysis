// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0043RemoveNewLinesFromAccessorWithSingleLineExpressionTests : AbstractCSharpDiagnosticVerifier<AccessorListAnalyzer, AccessorDeclarationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.RemoveNewLinesFromAccessorWithSingleLineExpression;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLinesFromAccessorWithSingleLineExpression)]
        public async Task Test_Getter_Setter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private string _p;

    string P
    {
        [|get
        {
            return _p;
        }|]
        [|set
        {
            _p = value;
        }|]
    }
}
", @"
class C
{
    private string _p;

    string P
    {
        get { return _p; }
        set { _p = value; }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLinesFromAccessorWithSingleLineExpression)]
        public async Task Test_Getter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private string _p;

    string P
    {
        [|get
        {
            return _p;
        }|]
        set { _p = value; }
    }
}
", @"
class C
{
    private string _p;

    string P
    {
        get { return _p; }
        set { _p = value; }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLinesFromAccessorWithSingleLineExpression)]
        public async Task Test_Setter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    private string _p;

    string P
    {
        get { return _p; }
        [|set
        {
            _p = value;
        }|]
    }
}
", @"
class C
{
    private string _p;

    string P
    {
        get { return _p; }
        set { _p = value; }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLinesFromAccessorWithSingleLineExpression)]
        public async Task TestNoDiagnostic_FullProperty()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    private string _p;

    string P
    {
        get { return _p; }
        set { _p = value; }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLinesFromAccessorWithSingleLineExpression)]
        public async Task TestNoDiagnostic_AutoProperty()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    string P
    {
        get;
        set;
    }
}
");
        }
    }
}
