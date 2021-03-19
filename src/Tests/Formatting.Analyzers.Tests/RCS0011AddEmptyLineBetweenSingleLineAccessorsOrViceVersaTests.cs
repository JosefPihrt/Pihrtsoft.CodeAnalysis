// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0011AddEmptyLineBetweenSingleLineAccessorsOrViceVersaTests : AbstractCSharpDiagnosticVerifier<AddOrRemoveEmptyLineBetweenAccessorsAnalyzer, SyntaxTriviaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddEmptyLineBetweenSingleLineAccessorsOrViceVersa;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineAccessorsOrViceVersa)]
        public async Task Test_Property()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string _p;

    string P
    {
        get { return _p; }[||]
        set { _p = value; }
    }
}
", @"
class C
{
    string _p;

    string P
    {
        get { return _p; }

        set { _p = value; }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineAccessorsOrViceVersa)]
        public async Task Test_RemoveEmptyLine_Property()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string _p;

    string P
    {
        get { return _p; }
[||]
        set { _p = value; }
    }
}
", @"
class C
{
    string _p;

    string P
    {
        get { return _p; }
        set { _p = value; }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.RemoveEmptyLineBetweenSingleLineAccessors));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineAccessorsOrViceVersa)]
        public async Task Test_RemoveEmptyLines_Property()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string _p;

    string P
    {
        get { return _p; }
[||]

        set { _p = value; }
    }
}
", @"
class C
{
    string _p;

    string P
    {
        get { return _p; }
        set { _p = value; }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.RemoveEmptyLineBetweenSingleLineAccessors));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineAccessorsOrViceVersa)]
        public async Task Test_RemoveEmptyLine_Event()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    event EventHandler E
    {
        add { }
[||]
        remove { }
    }
}
", @"
using System;

class C
{
    event EventHandler E
    {
        add { }
        remove { }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.RemoveEmptyLineBetweenSingleLineAccessors));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineAccessorsOrViceVersa)]
        public async Task TestNoDiagnostic_Property_FirstIsMultiline()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    string _p;

    string P
    {
        get
        {
            return _p;
        }
        set { _p = value; }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineAccessorsOrViceVersa)]
        public async Task TestNoDiagnostic_Property_SecondIsMultiline()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    string _p;

    string P
    {
        get { return _p; }
        set
        {
            _p = value;
        }
    }
}
");
        }
    }
}
