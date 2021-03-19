// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1240UnnecessaryOperatorTests : AbstractCSharpDiagnosticVerifier<BinaryOperatorAnalyzer, BinaryExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UnnecessaryOperator;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryOperator)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{
    void M()
    {
        var lst = new List<string>();
        var ar = Array.Empty<string>();
        var ia = ImmutableArray.Create<string>();
        var ss = default(string);

        if (lst.Count [|<|]= 0) { }
        if (ar.Length [|<|]= 0) { }
        if (ia.Length [|<|]= 0) { }
        if (ss.Length [|<|]= 0) { }
    }
}
", @"
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{
    void M()
    {
        var lst = new List<string>();
        var ar = Array.Empty<string>();
        var ia = ImmutableArray.Create<string>();
        var ss = default(string);

        if (lst.Count == 0) { }
        if (ar.Length == 0) { }
        if (ia.Length == 0) { }
        if (ss.Length == 0) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryOperator)]
        public async Task Test_RightToLeft()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{
    void M()
    {
        var lst = new List<string>();
        var ar = Array.Empty<string>();
        var ia = ImmutableArray.Create<string>();
        var ss = default(string);

        if (0 [|>|]= lst.Count) { }
        if (0 [|>|]= ar.Length) { }
        if (0 [|>|]= ia.Length) { }
        if (0 [|>|]= ss.Length) { }
    }
}
", @"
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{
    void M()
    {
        var lst = new List<string>();
        var ar = Array.Empty<string>();
        var ia = ImmutableArray.Create<string>();
        var ss = default(string);

        if (0 == lst.Count) { }
        if (0 == ar.Length) { }
        if (0 == ia.Length) { }
        if (0 == ss.Length) { }
    }
}
");
        }
    }
}
