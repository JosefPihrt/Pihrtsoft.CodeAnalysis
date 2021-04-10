// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1013UsePredefinedTypeTests : AbstractCSharpDiagnosticVerifier<UsePredefinedTypeAnalyzer, UsePredefinedTypeCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.UsePredefinedType;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePredefinedType)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Collections.Generic;

/// <summary>
/// <see cref=""[|String|]""/>
/// <see cref=""[|System.String|]""/>
/// <see cref=""[|global::System.String|]""/>
/// </summary>
class C
{
    void M([|String|] x)
    {
        x = default([|String|]);
        x = default([|System.String|]);
        x = default([|global::System.String|]);

        x = [|String|].Empty;
        x = [|System.String|].Empty;
        x = [|global::System.String|].Empty;

        x = nameof([|String|].Empty);
        x = nameof(List<[|String|]>);
        x = nameof(Dictionary<[|System.String|], [|global::System.String|]>);
    }
}
", @"
using System;
using System.Collections.Generic;

/// <summary>
/// <see cref=""string""/>
/// <see cref=""string""/>
/// <see cref=""string""/>
/// </summary>
class C
{
    void M(string x)
    {
        x = default(string);
        x = default(string);
        x = default(string);

        x = string.Empty;
        x = string.Empty;
        x = string.Empty;

        x = nameof(string.Empty);
        x = nameof(List<string>);
        x = nameof(Dictionary<string, string>);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePredefinedType)]
        public async Task Test_NullableReferenceType()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M([|String|]? x)
    {
    }
}
", @"
using System;

class C
{
    void M(string? x)
    {
    }
}
", options: WellKnownCSharpTestOptions.Default_NullableReferenceTypes);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePredefinedType)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using s = System.String;

/// <summary>
/// <see cref=""s""/>
/// </summary>
class C
{
    void M(string x)
    {
        s value = s.Empty;

        x = nameof(String);
        x = nameof(System.String);
        x = nameof(global::System.String);
    }
}
");
        }
    }
}
