// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0008AddEmptyLineBetweenBlockAndStatementTests : AbstractCSharpDiagnosticVerifier<AddEmptyLineBetweenBlockAndStatementAnalyzer, SyntaxTokenCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AddEmptyLineBetweenBlockAndStatement;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_Block()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        {
        }[||]
        M();
    }
}
", @"
class C
{
    void M()
    {
        {
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_While()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool x = false;
        while (x)
        {
        }[||]
        M();
    }
}
", @"
class C
{
    void M()
    {
        bool x = false;
        while (x)
        {
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_For()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        for (int i = 0; i < items.Count; i++)
        {
        }[||]
        M();
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        for (int i = 0; i < items.Count; i++)
        {
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_ForEach()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        foreach (var item in items)
        {
        }[||]
        M();
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        foreach (var item in items)
        {
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_ForEachVariable()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        foreach ((int x, int y) in new[] { (0, 0) })
        {
        }[||]
        M();
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        foreach ((int x, int y) in new[] { (0, 0) })
        {
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_Using()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        using (default(IDisposable))
        {
        }[||]
        M();
    }
}
", @"
using System;

class C
{
    void M()
    {
        using (default(IDisposable))
        {
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_Fixed()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    unsafe void M()
    {
        fixed (char* p = """")
        {
        }[||]
        M();
    }
}
", @"
class C
{
    unsafe void M()
    {
        fixed (char* p = """")
        {
        }

        M();
    }
}
", options: Options.WithAllowUnsafe(true));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_Checked()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        checked
        {
        }[||]
        M();
    }
}
", @"
class C
{
    void M()
    {
        checked
        {
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_Unchecked()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        unchecked
        {
        }[||]
        M();
    }
}
", @"
class C
{
    void M()
    {
        unchecked
        {
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_Unsafe()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        unsafe
        {
        }[||]
        M();
    }
}
", @"
class C
{
    void M()
    {
        unsafe
        {
        }

        M();
    }
}
", options: Options.WithAllowUnsafe(true));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_Lock()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        lock (null)
        {
        }[||]
        M();
    }
}
", @"
class C
{
    void M()
    {
        lock (null)
        {
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_If()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        if (true)
        {
        }[||]
        M();
    }
}
", @"
class C
{
    void M()
    {
        if (true)
        {
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_ElseIf()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool x = false;

        if (x)
        {
        }
        else if (x)
        {
        }[||]
        M();
    }
}
", @"
class C
{
    void M()
    {
        bool x = false;

        if (x)
        {
        }
        else if (x)
        {
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_Else()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        if (true)
        {
        }
        else
        {
        }[||]
        M();
    }
}
", @"
class C
{
    void M()
    {
        if (true)
        {
        }
        else
        {
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_Switch()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        switch ("""")
        {
            default:
                break;
        }[||]
        M();
    }
}
", @"
class C
{
    void M()
    {
        switch ("""")
        {
            default:
                break;
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_TryCatch()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        try
        {
        }
        catch (System.Exception)
        {
        }[||]
        M();
    }
}
", @"
class C
{
    void M()
    {
        try
        {
        }
        catch (System.Exception)
        {
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task Test_TryFinally()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        try
        {
        }
        finally
        {
        }[||]
        M();
    }
}
", @"
class C
{
    void M()
    {
        try
        {
        }
        finally
        {
        }

        M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
    }
}
");
        }
    }
}
