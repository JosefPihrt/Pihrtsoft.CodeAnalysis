// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0020AddNewLineAfterOpeningBraceOfAccessorTests : AbstractCSharpDiagnosticVerifier<AddNewLineAfterOpeningBraceOfAccessorAnalyzer, BlockCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddNewLineAfterOpeningBraceOfAccessor;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfAccessor)]
        public async Task Test_Getter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string _p;

    string P
    {
        get {[||] return _p; }
    }
}
", @"
class C
{
    string _p;

    string P
    {
        get
        {
            return _p;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfAccessor)]
        public async Task Test_Setter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string _p;

    string P
    {
        set {[||] _p = value; }
    }
}
", @"
class C
{
    string _p;

    string P
    {
        set
        {
            _p = value;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfAccessor)]
        public async Task Test_InitSetter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string _p;

    string P
    {
        get
        {
            return _p;
        }

        init {[||] _p = value; }
    }
}
", @"
class C
{
    string _p;

    string P
    {
        get
        {
            return _p;
        }

        init
        {
            _p = value;
        }
    }
}
", options: Options.AddAllowedCompilerDiagnosticId("CS0518"));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfAccessor)]
        public async Task Test_AddRemove()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    event EventHandler E
    {
        add {[||] }
        remove {[||] }
    }
}
", @"
using System;

class C
{
    event EventHandler E
    {
        add
        {
        }
        remove
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfAccessor)]
        public async Task TestNoDiagnostic_AutoProperty()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    string P { get; set; }
}
");
        }
    }
}
