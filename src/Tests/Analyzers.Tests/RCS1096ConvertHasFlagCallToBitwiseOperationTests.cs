// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1096ConvertHasFlagCallToBitwiseOperationTests : AbstractCSharpDiagnosticVerifier<ConvertHasFlagCallToBitwiseOperationOrViceVersaAnalyzer, ConvertHasFlagCallToBitwiseOperationOrViceVersaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.ConvertHasFlagCallToBitwiseOperationOrViceVersa;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_HasFlag()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ([|options.HasFlag(StringSplitOptions.RemoveEmptyEntries)|]) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ((options & StringSplitOptions.RemoveEmptyEntries) != 0) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_HasFlag_Flag()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if ([|x.HasFlag(RegexOptions.Singleline)|]) { }
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if ((x & RegexOptions.Singleline) != 0) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_HasFlags()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if ([|x.HasFlag(RegexOptions.Singleline | RegexOptions.Multiline)|]) { }
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if ((x & (RegexOptions.Singleline | RegexOptions.Multiline)) == (RegexOptions.Singleline | RegexOptions.Multiline)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_HasFlag_Parentheses()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ([|options.HasFlag(StringSplitOptions.None | StringSplitOptions.RemoveEmptyEntries)|]) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ((options & (StringSplitOptions.None | StringSplitOptions.RemoveEmptyEntries)) != 0) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_NotHasFlag()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if (![|options.HasFlag(StringSplitOptions.RemoveEmptyEntries)|]) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ((options & StringSplitOptions.RemoveEmptyEntries) == 0) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_NotHasFlag_Flag()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if (![|x.HasFlag(RegexOptions.Singleline)|]) { }
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if ((x & RegexOptions.Singleline) == 0) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_NotHasFlags()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if (![|x.HasFlag(RegexOptions.Singleline | RegexOptions.Multiline)|]) { }
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if ((x & (RegexOptions.Singleline | RegexOptions.Multiline)) != (RegexOptions.Singleline | RegexOptions.Multiline)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_HasFlag_EqualsTrue()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ([|options.HasFlag(StringSplitOptions.RemoveEmptyEntries)|] == true) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ((options & StringSplitOptions.RemoveEmptyEntries) != 0) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_HasFlags_EqualsTrue()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if ([|x.HasFlag(RegexOptions.Singleline | RegexOptions.Multiline)|] == true) { }
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if ((x & (RegexOptions.Singleline | RegexOptions.Multiline)) == (RegexOptions.Singleline | RegexOptions.Multiline)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_HasFlag_EqualsFalse()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ([|options.HasFlag(StringSplitOptions.RemoveEmptyEntries)|] == false) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ((options & StringSplitOptions.RemoveEmptyEntries) == 0) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_HasFlags_EqualsFalse()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if ([|x.HasFlag(RegexOptions.Singleline | RegexOptions.Multiline)|] == false) { }
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        var x = RegexOptions.Singleline | RegexOptions.Multiline;

        if ((x & (RegexOptions.Singleline | RegexOptions.Multiline)) != (RegexOptions.Singleline | RegexOptions.Multiline)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task Test_HasFlag_WithTrivia()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ( /*lt*/ [|options.HasFlag(StringSplitOptions.RemoveEmptyEntries /*tt*/ )|].Equals(true)) { }
    }
}
", @"
using System;

class C
{
    void M()
    {
        var options = StringSplitOptions.None;

        if ( /*lt*/ ((options & StringSplitOptions.RemoveEmptyEntries /*tt*/ ) != 0).Equals(true)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task TestNoDiagnostic_TypeIsSystemEnum()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        var @enum = default(Enum);
        var options = StringSplitOptions.None;

        if (options.HasFlag(@enum)) { }

        if (@enum.HasFlag(options)) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task TestNoDiagnostic_ConditionalAccess()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    StringSplitOptions P { get; }

    void M()
    {
        C c = null;

        if (c?.P.HasFlag(StringSplitOptions.RemoveEmptyEntries) == true) { }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertHasFlagCallToBitwiseOperationOrViceVersa)]
        public async Task TestNoDiagnostic_ConvertBitwiseOperationToHasFlagCall()
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
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertBitwiseOperationToHasFlagCall));
        }
    }
}
