// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1151RemoveRedundantCastTests2 : AbstractCSharpDiagnosticVerifier<InvocationExpressionAnalyzer, RemoveRedundantCastCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.RemoveRedundantCast;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        public async Task Test_CastToDerivedType()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<C> x = Enumerable.Empty<C>();

        IEnumerable<C> y = x
            .Where(x => x != default)
            .[|Cast<C>()|];
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<C> x = Enumerable.Empty<C>();

        IEnumerable<C> y = x
            .Where(x => x != default);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        public async Task TestNoDiagnostic_CastFromObject()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections;
using System.Linq;

class C
{
    void M()
    {
        object value = null;

        var values = ((IEnumerable)value).Cast<object>();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        public async Task TestNoDiagnostic_CastFromDynamic()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections;
using System.Linq;

class C
{
    void M()
    {
        dynamic value = null;

        var values = ((IEnumerable)value).Cast<object>();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantCast)]
        internal async Task TestNoDiagnostic_NullableReferenceType()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;
using System.Linq;

#nullable enable

class C
{
    void M()
    {
        IEnumerable<C?> nullables = Enumerable.Empty<C?>();

        IEnumerable<C> notNullables = nullables
            .Where(x => x != default)
            .Cast<C>();
    }
}
");
        }
    }
}
