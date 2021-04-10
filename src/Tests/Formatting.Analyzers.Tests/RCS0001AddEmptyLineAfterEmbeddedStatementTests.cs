// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0001AddEmptyLineAfterEmbeddedStatementTests : AbstractCSharpDiagnosticVerifier<AddEmptyLineAfterEmbeddedStatementAnalyzer, StatementCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AddEmptyLineAfterEmbeddedStatement;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterEmbeddedStatement)]
        public async Task Test_If()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
            M();[||]
        M();
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

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterEmbeddedStatement)]
        public async Task Test_ElseIf()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;

        if (f)
            M();
        else if (f)
            M();[||]
        M();
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
        else if (f)
            M();

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterEmbeddedStatement)]
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
        else
            M();[||]
        M();
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

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterEmbeddedStatement)]
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

        foreach (object item in items)
            M();[||]
        M();
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

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterEmbeddedStatement)]
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

        foreach ((int a, int b) in new[] { (0, 0) })
            M();[||]
        M();
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

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterEmbeddedStatement)]
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

        for (int i = 0; i < items.Count; i++)
            M();[||]
        M();
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

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterEmbeddedStatement)]
        public async Task Test_Using()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        bool f = false;

        using ((IDisposable)null)
            M();[||]
        M();
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

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterEmbeddedStatement)]
        public async Task Test_While()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;

        while (f)
            M();[||]
        M();
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

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterEmbeddedStatement)]
        public async Task Test_Lock()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;

        lock (null)
            M();[||]
        M();
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

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterEmbeddedStatement)]
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
            fixed (char* p = """")
                M();[||]
            M();
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

            M();
        }
    }
}
", options: Options.WithAllowUnsafe(true));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterEmbeddedStatement)]
        public async Task Test_Switch()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;
        string s = null;

        switch (s)
        {
            case null:
                if (f)
                    M();[||]
                break;
        }
    }
}
", @"
class C
{
    void M()
    {
        bool f = false;
        string s = null;

        switch (s)
        {
            case null:
                if (f)
                    M();

                break;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterEmbeddedStatement)]
        public async Task TestNoDiagnostic_EmbeddedStatement()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f = false;
        var items = new List<object>();

        if (f) M();
        if (f) M(); else M();
        if (f)
            M();
        else M();
        foreach (object item in items) M();
        foreach ((int a, int b) in new[] { (0, 0) }) M();
        for (int i = 0; i < items.Count; i++) M();
        using ((IDisposable)null) M();
        while (f) M();
        do M();
        while (f);
        lock (null) M();
        unsafe
        {
            fixed (char* p = """") M();
        }
    }
}
", options: Options.WithAllowUnsafe(true));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterEmbeddedStatement)]
        public async Task TestNoDiagnostic_NoEmbeddedStatement()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Collections.Generic;

class C
{
    void M()
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
