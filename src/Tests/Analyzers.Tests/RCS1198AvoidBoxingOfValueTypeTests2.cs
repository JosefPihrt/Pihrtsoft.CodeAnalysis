// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1198AvoidBoxingOfValueTypeTests2 : AbstractCSharpDiagnosticVerifier<InvocationExpressionAnalyzer, AvoidBoxingOfValueTypeCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AvoidBoxingOfValueType;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidBoxingOfValueType)]
        public async Task Test_StringBuilder_Append()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Text;

class C
{
    void M()
    {
        StringSplitOptions o = StringSplitOptions.None;
        var sb = new StringBuilder();

        sb.Append([|o|]);
    }
}
", @"
using System;
using System.Text;

class C
{
    void M()
    {
        StringSplitOptions o = StringSplitOptions.None;
        var sb = new StringBuilder();

        sb.Append(o.ToString());
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidBoxingOfValueType)]
        public async Task Test_StringBuilder_Insert()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Text;

class C
{
    void M()
    {
        StringSplitOptions o = StringSplitOptions.None;
        var sb = new StringBuilder();

        sb.Insert(1, [|o|]);
    }
}
", @"
using System;
using System.Text;

class C
{
    void M()
    {
        StringSplitOptions o = StringSplitOptions.None;
        var sb = new StringBuilder();

        sb.Insert(1, o.ToString());
    }
}
");
        }

        // https://github.com/dotnet/roslyn/pull/35006
        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidBoxingOfValueType)]
        public async Task TestNoDiagnostic_AppendFormat()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Text;

class C
{
    void M()
    {
        int i = 0;
        var sb = new StringBuilder();

        sb.AppendFormat(""f"", i);
        sb.AppendFormat(""f"", i, i);
        sb.AppendFormat(""f"", i, i, i);
        sb.AppendFormat(""f"", i, i, i, i);
    }
}
");
        }
    }
}
