// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1180InlineLazyInitializationTests : AbstractCSharpDiagnosticVerifier<UseCoalesceExpressionAnalyzer, StatementCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.InlineLazyInitialization;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.InlineLazyInitialization)]
        public async Task Test_If()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        List<string> x = null;

        M();

        [|if (x == null)
        {
            x = new List<string>();
        }|]

        x.Add("""");
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        List<string> x = null;

        M();

        (x ??= new List<string>()).Add("""");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.InlineLazyInitialization)]
        public async Task Test_If_WithoutBraces()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        List<string> x = null;

        M();

        [|if (x == null)
            x = new List<string>();|]

        x.Add("""");
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        List<string> x = null;

        M();

        (x ??= new List<string>()).Add("""");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.InlineLazyInitialization)]
        public async Task Test_If_Trivia()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        List<string> x = null;

        M();

        // a
        [|if (x == null) // b
            x = new List<string>();|] // c

        // d
        x.Add(""""); // e
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        List<string> x = null;

        M();

        // a
        // b
        // c

        // d
        (x ??= new List<string>()).Add(""""); // e
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.InlineLazyInitialization)]
        public async Task Test_If_CSharp7_3()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        List<string> x = null;

        M();

        [|if (x == null)
        {
            x = new List<string>();
        }|]

        x.Add("""");
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        List<string> x = null;

        M();

        (x ?? (x = new List<string>())).Add("""");
    }
}
", options: WellKnownCSharpTestOptions.Default_CSharp7_3);
        }
    }
}
