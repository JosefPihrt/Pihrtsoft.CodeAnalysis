// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1007AddBracesTests : AbstractCSharpDiagnosticVerifier<AddBracesAnalyzer, AddBracesCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddBraces;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task Test_If()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(object x, object y)
    {
        if(true)
            [|M(
                x,
                y);|]
    }
}
", @"
class C
{
    void M(object x, object y)
    {
        if(true)
        {
            M(
                x,
                y);
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task Test_IfElse()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(object x, object y)
    {
        if (x is string)
            [|M(
                x,
                y);|]
        else
            [|M(
                x,
                y);|]
    }
}
", @"
class C
{
    void M(object x, object y)
    {
        if (x is string)
        {
            M(
                x,
                y);
        }
        else
        {
            M(
                x,
                y);
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task Test_IfElseIf()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(object x, object y, bool b)
    {
        if (b)
            [|M(
                x,
                y,
                b);|]
        else if (b)
            [|M(
                x,
                y,
                b);|]
    }
}
", @"
class C
{
    void M(object x, object y, bool b)
    {
        if (b)
        {
            M(
                x,
                y,
                b);
        }
        else if (b)
        {
            M(
                x,
                y,
                b);
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task Test_IfElseIfElse()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(object x, object y, bool b)
    {
        if (b)
            [|M(
                x,
                y,
                b);|]
        else if (!b)
            [|M(
                x,
                y,
                b);|]
        else
            [|M(
                x,
                y,
                b);|]
    }
}
", @"
class C
{
    void M(object x, object y, bool b)
    {
        if (b)
        {
            M(
                x,
                y,
                b);
        }
        else if (!b)
        {
            M(
                x,
                y,
                b);
        }
        else
        {
            M(
                x,
                y,
                b);
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task Test_Foreach()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M(object x, object y)
    {
        IEnumerable<object> items = null;

        foreach (object item in items)
            [|M(
                x,
                y);|]
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M(object x, object y)
    {
        IEnumerable<object> items = null;

        foreach (object item in items)
        {
            M(
                x,
                y);
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task Test_ForeachTuple()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(object x, object y)
    {
        foreach ((string, string) item in new [] { ("""","""") })
            [|M(
                x,
                y);|]
    }
}
", @"
class C
{
    void M(object x, object y)
    {
        foreach ((string, string) item in new [] { ("""","""") })
        {
            M(
                x,
                y);
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task Test_For()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(object x, object y)
    {
        for (int i = 0; i < 1; i++)
            [|M(
                x,
                y);|]
    }
}
", @"
class C
{
    void M(object x, object y)
    {
        for (int i = 0; i < 1; i++)
        {
            M(
                x,
                y);
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task Test_Using()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(object x, object y)
    {
        using (null)
            [|M(
                x,
                y);|]
    }
}
", @"
class C
{
    void M(object x, object y)
    {
        using (null)
        {
            M(
                x,
                y);
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task Test_While()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(object x, object y)
    {
        while (true)
            [|M(
                x,
                y);|]
    }
}
", @"
class C
{
    void M(object x, object y)
    {
        while (true)
        {
            M(
                x,
                y);
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task Test_DoWhile()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(object x, object y)
    {
        do
            [|M(
                x,
                y);|]
        while (false);
    }
}
", @"
class C
{
    void M(object x, object y)
    {
        do
        {
            M(
                x,
                y);
        }
        while (false);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task Test_Lock()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(object x, object y)
    {
        lock (null)
            [|M(
                x,
                y);|]
    }
}
", @"
class C
{
    void M(object x, object y)
    {
        lock (null)
        {
            M(
                x,
                y);
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task Test_Fixed()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(object x, object y)
    {
        unsafe
        {
            int[] a = new[] { 1 };
            fixed (int* i = a)
                [|M(
                    x,
                    y);|]
        }
    }
}
", @"
class C
{
    void M(object x, object y)
    {
        unsafe
        {
            int[] a = new[] { 1 };
            fixed (int* i = a)
            {
                M(
                    x,
                    y);
            }
        }
    }
}
", options: Options.WithAllowUnsafe(true));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task TestNoDiagnostic_IfElseIf()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(bool b)
    {
        if (b)
        {
        }
        else if (b)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task TestNoDiagnostic_Using()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        using (null)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddBraces)]
        public async Task TestNoDiagnostic_ConsecutiveUsing()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        using (null)
        using (null)
        {
        }
    }
}
");
        }
    }
}