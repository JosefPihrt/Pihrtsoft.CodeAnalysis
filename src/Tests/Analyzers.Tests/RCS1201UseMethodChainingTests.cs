// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1201UseMethodChainingTests : AbstractCSharpDiagnosticVerifier<InvocationExpressionAnalyzer, StatementCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseMethodChaining;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseMethodChaining)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        var sb = new StringBuilder();

        [|sb.Append("""")|];
        sb.AppendFormat(""f"", """");
        sb.AppendLine("""");
        sb.Clear();
        sb.Insert(0, """");
        sb.Remove(0, 0);
        sb.Replace("""", """");
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        var sb = new StringBuilder();

        sb.Append("""")
            .AppendFormat(""f"", """")
            .AppendLine("""")
            .Clear()
            .Insert(0, """")
            .Remove(0, 0)
            .Replace("""", """");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseMethodChaining)]
        public async Task Test2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        var sb = new StringBuilder();

        [|sb.Append(""1"")|];
        sb.Append(""2"").Append(""3"");
        sb.Append(""4"").Append(""5"").Append(""6"");
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        var sb = new StringBuilder();

        sb.Append(""1"")
            .Append(""2"").Append(""3"")
            .Append(""4"").Append(""5"").Append(""6"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseMethodChaining)]
        public async Task Test_Assignment()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<object> q = Enumerable.Empty<object>();

        q = [|q.Select(f => ""1"")|];

        q = q.Select(f => ""2"")
            .Select(f => ""3"");

        q = q.Select(f => ""4"")
            .Select(f => ""5"")
            .Select(f => ""6"");
    }
}
", @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<object> q = Enumerable.Empty<object>();

        q = q.Select(f => ""1"")
            .Select(f => ""2"")
            .Select(f => ""3"")
            .Select(f => ""4"")
            .Select(f => ""5"")
            .Select(f => ""6"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseMethodChaining)]
        public async Task TestNoDiagnostic_ReturnTypesAreNotEqual()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var q = Enumerable.Empty<object>();

        q = q.Select(f => f);
        q = q.Select(f => (object)q);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseMethodChaining)]
        public async Task TestNoDiagnostic_NoAssignment()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        IEnumerable<object> q = Enumerable.Empty<object>();

        q.Select(f => ""1"");
        q.Select(f => ""2"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseMethodChaining)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
public class C
{
    public C2 M()
    {
        var x = new C();

        x.M();
        x.M().M2();

        return default;
    }
}

public class C2
{
    public C2 M2()
    {
        return default;
    }
}
");
        }
    }
}
