// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1020SimplifyNullableOfTTests : AbstractCSharpDiagnosticVerifier<SimplifyNullableOfTAnalyzer, SimplifyNullableOfTCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.SimplifyNullableOfT;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyNullableOfT)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        int? x = null;

        x = default([|Nullable<int>|]);
        x = default([|System.Nullable<int>|]);
        x = default([|global::System.Nullable<int>|]);

        x = default([|Nullable<Int32>|]);
        x = default([|System.Nullable<Int32>|]);
        x = default([|global::System.Nullable<Int32>|]);
    }
}
", @"
using System;

class C
{
    void M()
    {
        int? x = null;

        x = default(int?);
        x = default(int?);
        x = default(int?);

        x = default(Int32?);
        x = default(Int32?);
        x = default(Int32?);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyNullableOfT)]
        public async Task Test_NameOf()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        string s = nameof(List<[|Nullable<int>|]>);
    }
}
", @"
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        string s = nameof(List<int?>);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyNullableOfT)]
        public async Task TestNoDiagnostic_NameOf()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        string s = null;

        s = nameof(Nullable<int>);
        s = nameof(System.Nullable<int>);
        s = nameof(global::System.Nullable<int>);

        s = nameof(Nullable<int>.Value);
        s = nameof(System.Nullable<int>.Value);
        s = nameof(global::System.Nullable<int>.Value);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyNullableOfT)]
        public async Task TestNoDiagnostic_TypeOfNullableOfT()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        var x = typeof(Nullable<>);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyNullableOfT)]
        public async Task TestNoDiagnostic_Cref()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
        /// <summary>
        /// <see cref=""Nullable{T}""/>
        /// <see cref=""System.Nullable{T}""/>
        /// <see cref=""global::System.Nullable{T}""/>
        /// <see cref=""Nullable{T}.HasValue""/>
        /// <see cref=""System.Nullable{T}.HasValue""/>
        /// <see cref=""global::System.Nullable{T}.HasValue""/>
        /// </summary>
        void M()
        {
        }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyNullableOfT)]
        public async Task TestNoDiagnostic_UsingDirective()
        {
            await VerifyNoDiagnosticAsync(@"
using NullableOfInt = System.Nullable<int>;

class C
{
}
");
        }
    }
}
