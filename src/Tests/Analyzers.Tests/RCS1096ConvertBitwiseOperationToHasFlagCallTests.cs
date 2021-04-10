// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1096ConvertBitwiseOperationToHasFlagCallTests : AbstractCSharpDiagnosticVerifier<ConvertHasFlagCallToBitwiseOperationOrViceVersaAnalyzer, ConvertHasFlagCallToBitwiseOperationOrViceVersaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.ConvertHasFlagCallToBitwiseOperationOrViceVersa;

        public override CSharpTestOptions Options
        {
            get { return base.Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertBitwiseOperationToHasFlagCall); }
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_NotEquals_Zero()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ([|(options & StringSplitOptions.RemoveEmptyEntries) != 0|]) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if (options.HasFlag(StringSplitOptions.RemoveEmptyEntries)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_NotEquals_Value()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if ([|(x & (RegexOptions.Singleline | RegexOptions.IgnoreCase)) != (RegexOptions.Singleline | RegexOptions.IgnoreCase)|]) { }
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if (!x.HasFlag(RegexOptions.Singleline | RegexOptions.IgnoreCase)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_NotEquals_Parentheses()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ([|((options) & (StringSplitOptions.None | StringSplitOptions.RemoveEmptyEntries)) != (0)|]) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ((options).HasFlag(StringSplitOptions.None | StringSplitOptions.RemoveEmptyEntries)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_NotEquals_WithTrivia()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ( /*lt*/ ([|(options & StringSplitOptions.RemoveEmptyEntries /*tt*/ ) != 0|]).Equals(true)) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ( /*lt*/ (options.HasFlag(StringSplitOptions.RemoveEmptyEntries /*tt*/ )).Equals(true)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_Equals_ZeroOnLeftSide()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ([|0 == (options & StringSplitOptions.RemoveEmptyEntries)|]) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if (!options.HasFlag(StringSplitOptions.RemoveEmptyEntries)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_Equals_Value()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if ([|(x & (RegexOptions.Singleline | RegexOptions.IgnoreCase)) == (RegexOptions.Singleline | RegexOptions.IgnoreCase)|]) { }
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if (x.HasFlag(RegexOptions.Singleline | RegexOptions.IgnoreCase)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_NoDiagnostic_ConditionalAccess()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        var c = new C();

        if ((c?.P & StringSplitOptions.RemoveEmptyEntries) == 0) { }

        if ((c?.P & StringSplitOptions.RemoveEmptyEntries) != 0) { }
    }

    StringSplitOptions P { get; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task TestNoDiagnostic_HasFlag()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if (options.HasFlag(StringSplitOptions.RemoveEmptyEntries)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task TestNoDiagnostic_Equals_CompositeValue()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if ((x & (RegexOptions.Singleline | RegexOptions.IgnoreCase)) == 0) { }
        if ((x & (RegexOptions.Singleline | RegexOptions.IgnoreCase)) != 0) { }
    }
}
");
        }
    }
}
