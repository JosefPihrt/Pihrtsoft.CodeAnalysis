// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0030AddNewLineBeforeEmbeddedStatementTests : AbstractCSharpDiagnosticVerifier<AddNewLineBeforeEmbeddedStatementAnalyzer, StatementCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AddNewLineBeforeEmbeddedStatement;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEmbeddedStatement)]
        public async Task Test_If()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f) [||]M();
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;

        if (f)
            M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEmbeddedStatement)]
        public async Task Test_Else()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
            M();
        else [||]M();
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;

        if (f)
            M();
        else
            M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEmbeddedStatement)]
        public async Task Test_ForEach()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f = false;
        var items = new List<object>();

        foreach (object item in items) [||]M();
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f = false;
        var items = new List<object>();

        foreach (object item in items)
            M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEmbeddedStatement)]
        public async Task Test_ForEachVariable()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f = false;
        var items = new List<object>();

        foreach ((int a, int b) in new[] { (0, 0) }) [||]M();
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f = false;
        var items = new List<object>();

        foreach ((int a, int b) in new[] { (0, 0) })
            M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEmbeddedStatement)]
        public async Task Test_For()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f = false;
        var items = new List<object>();

        for (int i = 0; i < items.Count; i++) [||]M();
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f = false;
        var items = new List<object>();

        for (int i = 0; i < items.Count; i++)
            M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEmbeddedStatement)]
        public async Task Test_Using()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        bool f = false;

        using ((IDisposable)null) [||]M();
    }
}
", @"
using System;

class C
{
    void M()
    {
        bool f = false;

        using ((IDisposable)null)
            M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEmbeddedStatement)]
        public async Task Test_While()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;

        while (f) [||]M();
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;

        while (f)
            M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEmbeddedStatement)]
        public async Task Test_Do()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;

        do [||]M();
        while (f);
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;

        do
            M();
        while (f);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEmbeddedStatement)]
        public async Task Test_Lock()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;

        lock (null) [||]M();
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;

        lock (null)
            M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEmbeddedStatement)]
        public async Task Test_Fixed()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;

        unsafe
        {
            fixed (char* p = """") [||]M();
        }
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;

        unsafe
        {
            fixed (char* p = """")
                M();
        }
    }
}
", options: Options.WithAllowUnsafe(true));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEmbeddedStatement)]
        public async Task TestNoDiagnostic_EmbeddedStatement()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Collections.Generic;

class C
{
    private static void M()
    {
        bool f = false;
        var items = new List<object>();

        if (f)
            M();
        else
            M();

        foreach (object item in items)
            M();

        foreach ((int a, int b) in new[] { (0, 0) })
            M();

        for (int i = 0; i < items.Count; i++)
            M();

        using ((IDisposable)null)
            M();

        while (f)
            M();

        do
            M();
        while (f);

        lock (null)
            M();

        unsafe
        {
            fixed (char* p = """")
                M();
        }
    }
}
", options: Options.WithAllowUnsafe(true));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEmbeddedStatement)]
        public async Task TestNoDiagnostic_Block()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Collections.Generic;

class C
{
    private static void M()
    {
        bool f = false;
        var items = new List<object>();

        if (f)
        {
            M();
        }
        else
        {
            M();
        }

        foreach (object item in items)
        {
            M();
        }

        foreach ((int a, int b) in new[] { (0, 0) })
        {
            M();
        }

        for (int i = 0; i < items.Count; i++)
        {
            M();
        }

        using ((IDisposable)null)
        {
            M();
        }

        while (f)
        {
            M();
        }

        do
        {
            M();
        }
        while (f);

        lock (null)
        {
            M();
        }

        unsafe
        {
            fixed (char* p = """")
            {
                M();
            }
        }
    }
}
", options: Options.WithAllowUnsafe(true));
        }
    }
}
