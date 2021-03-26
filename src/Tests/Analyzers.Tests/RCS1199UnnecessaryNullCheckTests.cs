// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1199UnnecessaryNullCheckTests : AbstractCSharpDiagnosticVerifier<UnnecessaryNullCheckAnalyzer, BinaryExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UnnecessaryNullCheck;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task Test_HasValue_Bool()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool? x = null;

        if ([|x.HasValue && x.Value|]) { }
    }
}
", @"
class C
{
    void M()
    {
        bool? x = null;

        if (x == true) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task Test_HasValue_Bool_EqualsTrue()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool? x = null;

        if ([|x.HasValue && x.Value == true|]) { }
    }
}
", @"
class C
{
    void M()
    {
        bool? x = null;

        if (x == true) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task Test_HasValue_Bool_Parentheses()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool? x = null;

        if ([|(x.HasValue) && (x.Value)|]) { }
    }
}
", @"
class C
{
    void M()
    {
        bool? x = null;

        if (x == true) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task Test_HasValue_Bool_False()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool? x = null;

        if ([|x.HasValue && !x.Value|]) { }
    }
}
", @"
class C
{
    void M()
    {
        bool? x = null;

        if (x == false) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task Test_HasValue_Bool_EqualsFalse()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool? x = null;

        if ([|x.HasValue && x.Value == false|]) { }
    }
}
", @"
class C
{
    void M()
    {
        bool? x = null;

        if (x == false) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task Test_HasValue_ValueEquals()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        DateTime? x = default;
        DateTime y = default;

        if ([|x.HasValue && x.Value == y|]) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        DateTime? x = default;
        DateTime y = default;

        if (x == y) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task Test_NotEqualsToNull_ValueEquals()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        DateTime? x = default;
        DateTime y = default;

        if ([|x != null && x.Value == y|]) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        DateTime? x = default;
        DateTime y = default;

        if (x == y) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task Test_NotEqualsToNull_ValueLessThan()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        DateTime? x = default;
        DateTime y = default;

        if ([|x != null && x.Value < y|]) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        DateTime? x = default;
        DateTime y = default;

        if (x < y) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task Test_NotEqualsToNull_ValueLessThanOrEquals()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        DateTime? x = default;
        DateTime y = default;

        if ([|x != null && x.Value <= y|]) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        DateTime? x = default;
        DateTime y = default;

        if (x <= y) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task Test_NotEqualsToNull_ValueGreaterThan()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        DateTime? x = default;
        DateTime y = default;

        if ([|x != null && x.Value > y|]) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        DateTime? x = default;
        DateTime y = default;

        if (x > y) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task Test_NotEqualsToNull_ValueGreaterThanOrEquals()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        DateTime? x = default;
        DateTime y = default;

        if ([|x != null && x.Value >= y|]) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        DateTime? x = default;
        DateTime y = default;

        if (x >= y) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        bool? x = null;
        bool? x2 = null;

        if (x2.HasValue && x.Value) { }

        if (x.HasValue && x2.Value) { }

        if (x.HasValue && x.HasValue) { }

        if (x2.HasValue && !x.Value) { }

        if (x.HasValue && !x2.Value) { }

        if (x.HasValue && !x.HasValue) { }

        if (x2.HasValue && x.Value == false) { }

        if (x.HasValue && x2.Value == false) { }

        if (x.HasValue && x.HasValue) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task TestNoDiagnostic2()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        Version x = null;
        Version y = null;

        if (x != null && x == y) { }

        if (x != null && x < y) { }

        if (x != null && x <= y) { }

        if (x != null && x > y) { }

        if (x != null && x >= y) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task TestNoDiagnostic_RightSideIsNullable()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        int? x = null;
        int? y = null;

        if (x != null && x.Value == y) { }

        if (x != null && x.Value < y) { }

        if (x != null && x.Value <= y) { }

        if (x != null && x.Value > y) { }

        if (x != null && x.Value >= y) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryNullCheck)]
        public async Task TestNoDiagnostic_RightSideIsNullOrDefault()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        int? x = null;

        if (x != null && x.Value == null) { }

        if (x != null && x.Value == default) { }
    }
}
", options: Options.AddAllowedCompilerDiagnosticId("CS0472"));
        }
    }
}
