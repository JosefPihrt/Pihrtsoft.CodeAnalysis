// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1068SimplifyLogicalNegationTests2 : AbstractCSharpDiagnosticVerifier<InvocationExpressionAnalyzer, SimplifyLogicalNegationCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.SimplifyLogicalNegation;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_NotAny()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f1 = false;
        bool f2 = false;
        var items = new List<string>();

        f1 = [|!items.Any(s => !s.Equals(s))|];
    }
}
", @"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f1 = false;
        bool f2 = false;
        var items = new List<string>();

        f1 = items.All(s => s.Equals(s));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_NotAny2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f1 = false;
        bool f2 = false;
        var items = new List<string>();

        f1 = [|!(items.Any(s => (!s.Equals(s))))|];
    }
}
", @"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f1 = false;
        bool f2 = false;
        var items = new List<string>();

        f1 = items.All(s => (s.Equals(s)));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_NotAny3()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f1 = false;
        bool f2 = false;
        var items = new List<string>();

        f1 = [|!items.Any<string>(s => !s.Equals(s))|];
    }
}
", @"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f1 = false;
        bool f2 = false;
        var items = new List<string>();

        f1 = items.All<string>(s => s.Equals(s));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_NotAll()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f1 = false;
        bool f2 = false;
        var items = new List<string>();

        f1 = [|!items.All(s => !s.Equals(s))|];
    }
}
", @"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f1 = false;
        bool f2 = false;
        var items = new List<string>();

        f1 = items.Any(s => s.Equals(s));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_NotAll2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f1 = false;
        bool f2 = false;
        var items = new List<string>();

        f1 = [|!(items.All(s => (!s.Equals(s))))|];
    }
}
", @"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f1 = false;
        bool f2 = false;
        var items = new List<string>();

        f1 = items.Any(s => (s.Equals(s)));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.SimplifyLogicalNegation)]
        public async Task Test_NotAll3()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f1 = false;
        bool f2 = false;
        var items = new List<string>();

        f1 = [|!items.All<string>(s => !s.Equals(s))|];
    }
}
", @"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        bool f1 = false;
        bool f2 = false;
        var items = new List<string>();

        f1 = items.Any<string>(s => s.Equals(s));
    }
}
");
        }
    }
}
