// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1134RemoveRedundantStatementTests : AbstractCSharpDiagnosticVerifier<RemoveRedundantStatementAnalyzer, StatementCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.RemoveRedundantStatement;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantStatement)]
        public async Task Test_SimpleIf_ReturnNull()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M()
    {
        bool f = false;

        if (f)
        {
            [|return null;|]
        }

        return null;
    }
}
", @"
class C
{
    string M()
    {
        bool f = false;

        if (f)
        {
        }

        return null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantStatement)]
        public async Task Test_SimpleIf_ReturnDefault()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M()
    {
        bool f = false;

        if (f)
        {
            [|return default;|]
        }

        return default;
    }
}
", @"
class C
{
    string M()
    {
        bool f = false;

        if (f)
        {
        }

        return default;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantStatement)]
        public async Task Test_SimpleIf_ReturnTrue()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    bool M()
    {
        bool f = false;

        if (f)
        {
            [|return true;|]
        }

        return true;
    }
}
", @"
class C
{
    bool M()
    {
        bool f = false;

        if (f)
        {
        }

        return true;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantStatement)]
        public async Task Test_SimpleIf_ReturnFalse()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    bool M()
    {
        bool f = false;

        if (f)
        {
            [|return false;|]
        }

        return false;
    }
}
", @"
class C
{
    bool M()
    {
        bool f = false;

        if (f)
        {
        }

        return false;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantStatement)]
        public async Task Test_SimpleLambdaBody()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        items.ForEach(_ =>
        {
            bool f = false;
            if (f)
            {
                M();
                [|return;|]
            }
        });
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        items.ForEach(_ =>
        {
            bool f = false;
            if (f)
            {
                M();
            }
        });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantStatement)]
        public async Task Test_ParenthesizedLambdaBody()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        items.ForEach((_) =>
        {
            bool f = false;
            if (f)
            {
                M();
                [|return;|]
            }
        });
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        items.ForEach((_) =>
        {
            bool f = false;
            if (f)
            {
                M();
            }
        });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantStatement)]
        public async Task Test_AnonymousMethodBody()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        items.ForEach(delegate(string s)
        {
            bool f = false;
            if (f)
            {
                M();
                [|return;|]
            }
        });
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var items = new List<string>();

        items.ForEach(delegate(string s)
        {
            bool f = false;
            if (f)
            {
                M();
            }
        });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantStatement)]
        public async Task TestNoDiagnostic_SimpleIf_ExpressionsAreNotEquivalent()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    bool M()
    {
        bool f = false;

        if (f)
        {
            return true;
        }

        return false;
    }
}
");
        }
    }
}
