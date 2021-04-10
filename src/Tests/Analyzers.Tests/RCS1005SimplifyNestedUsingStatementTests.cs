// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1005SimplifyNestedUsingStatementTests : AbstractCSharpDiagnosticVerifier<SimplifyNestedUsingStatementAnalyzer, SimplifyNestedUsingStatementCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.SimplifyNestedUsingStatement;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyNestedUsingStatement)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        using (var x = GetResource())
        [|{
            using (var y = GetResource())
            {
                using (var z = GetResource())
                {
                    return;
                }
            }
        }|]
    }

    IDisposable GetResource() => null;
}
", @"
using System;

class C
{
    void M()
    {
        using (var x = GetResource())
        using (var y = GetResource())
        using (var z = GetResource())
        {
            return;
        }
    }

    IDisposable GetResource() => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyNestedUsingStatement)]
        public async Task Test_OpenBraceAtTheEndOfLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        using (var x = GetResource()) [|{
            using (var y = GetResource()) {
                using (var z = GetResource())
                {
                    return;
                }
            }
        }|]
    }

    IDisposable GetResource() => null;
}
", @"
using System;

class C
{
    void M()
    {
        using (var x = GetResource())
        using (var y = GetResource())
        using (var z = GetResource())
        {
            return;
        }
    }

    IDisposable GetResource() => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyNestedUsingStatement)]
        public async Task TestNoDiagnostic_MultipleStatement()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        using (var x = GetResource())
        using (var y = GetResource())
        {
            using (var z = GetResource())
            {
            }

            return;
        }
    }

    IDisposable GetResource() => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyNestedUsingStatement)]
        public async Task TestNoDiagnostic_WithComment()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        using (var x = GetResource())
        using (var y = GetResource())
        { // comment
            using (var z = GetResource())
            {
                return;
            }
        }
    }

    IDisposable GetResource() => null;
}
");
        }
    }
}
