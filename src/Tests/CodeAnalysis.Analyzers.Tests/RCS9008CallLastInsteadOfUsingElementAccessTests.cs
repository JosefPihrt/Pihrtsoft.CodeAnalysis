// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CodeAnalysis.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS9008CallLastInsteadOfUsingElementAccessTests : AbstractCSharpDiagnosticVerifier<ElementAccessExpressionAnalyzer, ElementAccessExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.CallLastInsteadOfUsingElementAccess;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.CallLastInsteadOfUsingElementAccess)]
        public async Task Test_SyntaxList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        SyntaxList<SyntaxNode> list = default;

        var last = list[|[list.Count - 1]|];
    }
}
", @"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        SyntaxList<SyntaxNode> list = default;

        var last = list.Last();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.CallLastInsteadOfUsingElementAccess)]
        public async Task Test_SeparatedSyntaxList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        SeparatedSyntaxList<SyntaxNode> list = default;

        var last = list[|[list.Count - 1]|];
    }
}
", @"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        SeparatedSyntaxList<SyntaxNode> list = default;

        var last = list.Last();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.CallLastInsteadOfUsingElementAccess)]
        public async Task Test_SyntaxTokenList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        SyntaxTokenList list = default;

        var last = list[|[list.Count - 1]|];
    }
}
", @"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        SyntaxTokenList list = default;

        var last = list.Last();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.CallLastInsteadOfUsingElementAccess)]
        public async Task Test_SyntaxTriviaList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        SyntaxTriviaList list = default;

        var last = list[|[list.Count - 1]|];
    }
}
", @"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        SyntaxTriviaList list = default;

        var last = list.Last();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.CallLastInsteadOfUsingElementAccess)]
        public async Task Test_ChildSyntaxList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        ChildSyntaxList list = default;

        var last = list[|[list.Count - 1]|];
    }
}
", @"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        ChildSyntaxList list = default;

        var last = list.Last();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.CallLastInsteadOfUsingElementAccess)]
        public async Task Test_SyntaxNodeOrTokenList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        SyntaxNodeOrTokenList list = default;

        var last = list[|[list.Count - 1]|];
    }
}
", @"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        SyntaxNodeOrTokenList list = default;

        var last = list.Last();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.CallLastInsteadOfUsingElementAccess)]
        public async Task TestNoDiagnostic_NotLastElement()
        {
            await VerifyNoDiagnosticAsync(@"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        SyntaxList<SyntaxNode> list = default;

        var last = list[list.Count - 2];
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.CallLastInsteadOfUsingElementAccess)]
        public async Task TestNoDiagnostic_ExpressionsAreNotEquivalent()
        {
            await VerifyNoDiagnosticAsync(@"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        SyntaxList<SyntaxNode> list = default;
        SyntaxList<SyntaxNode> list2 = default;

        var last = list[list2.Count - 1];
    }
}
");
        }
    }
}
