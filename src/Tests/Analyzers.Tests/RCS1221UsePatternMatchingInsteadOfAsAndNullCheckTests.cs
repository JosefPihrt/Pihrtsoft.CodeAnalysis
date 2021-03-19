// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.Analysis.UsePatternMatching;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1221UsePatternMatchingInsteadOfAsAndNullCheckTests : AbstractCSharpDiagnosticVerifier<UsePatternMatchingInsteadOfAsAndNullCheckAnalyzer, UsePatternMatchingInsteadOfAsAndNullCheckCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UsePatternMatchingInsteadOfAsAndNullCheck;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfAsAndNullCheck)]
        public async Task Test_EqualsToNull()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        object x = null;

        [|var s = x as string;|]

        if (s == null)
        {
            return;
        }
    }
}
", @"
class C
{
    void M()
    {
        object x = null;

        if (!(x is string s))
        {
            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfAsAndNullCheck)]
        public async Task Test_EqualsToNull_ExplicitType()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        object x = null;

        [|string s = x as string;|]
        if (s == null)
            return;
    }
}
", @"
class C
{
    void M()
    {
        object x = null;

        if (!(x is string s))
            return;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfAsAndNullCheck)]
        public async Task Test_EqualsToNull_ReturnVariable()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M()
    {
        object x = null;

        [|var s = x as string;|]
        if (s == null)
        {
            return s;
        }

        return s;
    }
}
", @"
class C
{
    string M()
    {
        object x = null;

        if (!(x is string s))
        {
            return null;
        }

        return s;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfAsAndNullCheck)]
        public async Task Test_IsNull()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        object x = null;

        [|var s = x as string;|]
        if (s is null)
        {
            return;
        }
    }
}
", @"
class C
{
    void M()
    {
        object x = null;

        if (!(x is string s))
        {
            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfAsAndNullCheck)]
        public async Task TestNoDiagnostic_MultipleLocalDeclarations()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        object x = null;

        string s = x as string, y = x as string;
        if (s == null)
        {
            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfAsAndNullCheck)]
        public async Task TestNoDiagnostic_NotSimpleIf()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        object x = null;

        var s = x as string;
        if (s == null)
        {
            return;
        }
        else
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfAsAndNullCheck)]
        public async Task TestNoDiagnostic_DoesNotContainJumpStatement()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        object x = null;

        var s = x as string;
        if (s == null)
        {
            M();
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfAsAndNullCheck)]
        public async Task TestNoDiagnostic_NotEqualsToNull()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        object x = null;

        var s = x as string;
        if (s != null)
        {
            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfAsAndNullCheck)]
        public async Task TestNoDiagnostic_OtherVariableCheckedForNull()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        object x = null;
        string s = null;

        var s2 = x as string;
        if (s == null)
        {
            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfAsAndNullCheck)]
        public async Task TestNoDiagnostic_TypesDoNotEqual()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        object x = null;

        object o = x as string;
        if (o == null)
        {
            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfAsAndNullCheck)]
        public async Task TestNoDiagnostic_Directive()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        object x = null;

#region
        var s = x as string;
#endregion
        if (s == null)
        {
            return;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfAsAndNullCheck)]
        public async Task TestNoDiagnostic_NullableType()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        object x = null;

        var y = x as int?;

        if (y == null)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePatternMatchingInsteadOfAsAndNullCheck)]
        public async Task TestNoDiagnostic_LanguageVersion()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        object x = null;

        var s = x as string;
        if (s == null)
        {
            return;
        }
    }
}
", options: WellKnownCSharpTestOptions.Default_CSharp6);
        }
    }
}
