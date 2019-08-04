// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.CSharp.CodeFixes;
using Roslynator.Tests;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1242UseStringEmptyInsteadOfEmptyStringLiteralTests : AbstractCSharpFixVerifier {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseStringEmptyInsteadOfEmptyStringLiteral;

        public override DiagnosticAnalyzer Analyzer { get; } = new UseStringEmptyInsteadOfEmptyStringLiteralAnalyzer();

        public override CodeFixProvider FixProvider { get; } = new UseStringEmptyInsteadOfEmptyStringLiteralCodeFixProvider();

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseStringEmptyInsteadOfEmptyStringLiteral)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class C
{
    void M()
    {
        string str = [|""""|];
    }
}
", @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class C
{
    void M()
    {
        string str = string.Empty;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseStringEmptyInsteadOfEmptyStringLiteral)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class C
{
    void M()
    {
        string a = string.Empty;
    }
}
");
        }
    }
}
