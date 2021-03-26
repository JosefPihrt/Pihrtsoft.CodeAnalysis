// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CodeAnalysis.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS9001UsePatternMatchingTests : AbstractCSharpDiagnosticVerifier<UsePatternMatchingAnalyzer, UsePatternMatchingCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UsePatternMatching;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatching)]
        public async Task Test_SwitchStatement()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode node = null;

        [|switch|] (node.Kind())
        {
            case SyntaxKind.IdentifierName:
                {
                    var identifierName = (IdentifierNameSyntax)node;
                    break;
                }
            case SyntaxKind.GenericName:
                var genericName = (GenericNameSyntax)node;
                break;
            default:
                {
                    throw new InvalidOperationException();
                }
        }
    }
}
", @"
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode node = null;

        switch (node)
        {
            case IdentifierNameSyntax identifierName:
                {
                    break;
                }

            case GenericNameSyntax genericName:
                break;
            default:
                {
                    throw new InvalidOperationException();
                }
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatching)]
        public async Task Test_SwitchStatement_LocalDeclaration()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode node = null;

        SyntaxKind kind = node.Kind();

        [|switch|] (kind)
        {
            case SyntaxKind.IdentifierName:
                {
                    var identifierName = (IdentifierNameSyntax)node;
                    break;
                }
            case SyntaxKind.GenericName:
                var genericName = (GenericNameSyntax)node;
                break;
            default:
                {
                    throw new InvalidOperationException();
                }
        }
    }
}
", @"
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode node = null;

        switch (node)
        {
            case IdentifierNameSyntax identifierName:
                {
                    break;
                }

            case GenericNameSyntax genericName:
                break;
            default:
                {
                    throw new InvalidOperationException();
                }
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatching)]
        public async Task Test_IfStatement_IsKind()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        [|if|] (x.IsKind(SyntaxKind.IdentifierName))
        {
            var y = (IdentifierNameSyntax)x;
        }
    }
}
", @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        if (x is IdentifierNameSyntax y)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatching)]
        public async Task Test_IfStatement_IsKind_Conditional()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        [|if|] (x?.IsKind(SyntaxKind.IdentifierName) == true)
        {
            var y = (IdentifierNameSyntax)x;
        }
    }
}
", @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        if (x is IdentifierNameSyntax y)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatching)]
        public async Task Test_IfStatement_Kind()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        [|if|] (x.Kind() == SyntaxKind.IdentifierName)
        {
            var y = (IdentifierNameSyntax)x;
        }
    }
}
", @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        if (x is IdentifierNameSyntax y)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatching)]
        public async Task Test_IfStatement_Kind_Conditional()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        [|if|] (x?.Kind() == SyntaxKind.IdentifierName)
        {
            var y = (IdentifierNameSyntax)x;
        }
    }
}
", @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        if (x is IdentifierNameSyntax y)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatching)]
        public async Task Test_IfStatement_NotIsKind()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        [|if|] (!x.IsKind(SyntaxKind.IdentifierName))
        {
            return;
        }

        var y = (IdentifierNameSyntax)x;
    }
}
", @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        if (!(x is IdentifierNameSyntax y))
        {
            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatching)]
        public async Task Test_IfStatement_NotIsKind_Embedded()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        [|if|] (!x.IsKind(SyntaxKind.IdentifierName))
            return;

        var y = (IdentifierNameSyntax)x;
    }
}
", @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        if (!(x is IdentifierNameSyntax y))
            return;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatching)]
        public async Task Test_IfStatement_NotIsKind_Conditional()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        [|if|] (x?.IsKind(SyntaxKind.IdentifierName) != true)
        {
            return;
        }

        var y = (IdentifierNameSyntax)x;
    }
}
", @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        if (!(x is IdentifierNameSyntax y))
        {
            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatching)]
        public async Task Test_IfStatement_NotKind()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        [|if|] (x.Kind() != SyntaxKind.IdentifierName)
        {
            return;
        }

        var y = (IdentifierNameSyntax)x;
    }
}
", @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        if (!(x is IdentifierNameSyntax y))
        {
            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatching)]
        public async Task Test_IfStatement_NotKind_Conditional()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        [|if|] (x?.Kind() != SyntaxKind.IdentifierName)
        {
            return;
        }

        var y = (IdentifierNameSyntax)x;
    }
}
", @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        if (!(x is IdentifierNameSyntax y))
        {
            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatching)]
        public async Task TestNoDiagnostic_SwitchStatement_VariableIsReferenced()
        {
            await VerifyNoDiagnosticAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode node = null;

        SyntaxKind kind = node.Kind();

        switch (kind)
        {
            case SyntaxKind.IdentifierName:
                {
                    var identifierName = (IdentifierNameSyntax)node;
                    break;
                }
        }

        if (kind == SyntaxKind.None) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatching)]
        public async Task TestNoDiagnostic_IfStatement_SimpleMemberAccessExpression()
        {
            await VerifyNoDiagnosticAsync(@"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class C
{
    void M()
    {
        SyntaxNode x = null;

        if (x.IsKind(SyntaxKind.SimpleMemberAccessExpression))
        {
            var y = (MemberAccessExpressionSyntax)x;
        }
    }
}
");
        }
    }
}
