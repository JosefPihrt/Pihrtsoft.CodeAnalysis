// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1190JoinStringExpressionsTests : AbstractCSharpDiagnosticVerifier<JoinStringExpressionsAnalyzer, BinaryExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.JoinStringExpressions;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task Test_Literal_Regular()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s)
    {
        s = [|""\x1g"" + ""\x1\x1234"" + ""\x1""|];
    }
}
", @"
class C
{
    void M(string s)
    {
        s = ""\x1g\x1\x1234\x1"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task Test_Literal_Regular2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s)
    {
        s = s + [|""a"" + ""b"" + ""c""|] + s;
    }
}
", @"
class C
{
    void M(string s)
    {
        s = s + ""abc"" + s;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task Test_Literal_Verbatim()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s)
    {
        s = [|@""a"" + @""b"" + @""c""|];
    }
}
", @"
class C
{
    void M(string s)
    {
        s = @""abc"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task Test_Literal_Verbatim2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s)
    {
        s = s + [|@""a"" + @""b"" + @""c""|] + s;
    }
}
", @"
class C
{
    void M(string s)
    {
        s = s + @""abc"" + s;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task Test_InterpolatedString_Regular()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s)
    {
        s = [|$""\x1g"" + $""\x1\x1234"" + $""{s}\x1""|];
    }
}
", @"
class C
{
    void M(string s)
    {
        s = $""\x1g\x1\x1234{s}\x1"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task Test_InterpolatedString_Regular2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s)
    {
        s = s + [|$""a"" + $""b"" + $""{s}\x1""|] + s;
    }
}
", @"
class C
{
    void M(string s)
    {
        s = s + $""ab{s}\x1"" + s;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task Test_InterpolatedString_Verbatim()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s)
    {
        s = [|$@""a"" + $@""b"" + $@""c""|];
    }
}
", @"
class C
{
    void M(string s)
    {
        s = $@""abc"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task Test_InterpolatedString_Verbatim2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s)
    {
        s = s + [|$@""a"" + $@""b"" + $@""c""|] + s;
    }
}
", @"
class C
{
    void M(string s)
    {
        s = s + $@""abc"" + s;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task Test_Verbatim_Multiline()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s)
    {
            s = [|@""a"" + @""b
c"" + @""d
e""|];
    }
}
", @"
class C
{
    void M(string s)
    {
            s = @""ab
cd
e"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task Test_InterpolatedString_Multiline()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string s)
    {
            s = [|$@""a"" + $@""b
c"" + $@""d
e""|];
    }
}
", @"
class C
{
    void M(string s)
    {
            s = $@""ab
cd
e"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task TestNoDiagnostic_Regular_Multiline()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(string s)
    {
        s = ""a""
            + ""b"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task TestNoDiagnostic_RegularAndVerbatim()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(string s)
    {
        s = ""a"" + @""b"";
        s = @""a"" + ""b"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task TestNoDiagnostic_LiteralAndInterpolated()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(string s)
    {
        s = ""a"" + $""b"";
        s = $""a"" + ""b"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task TestNoDiagnostic_LiteralAndInterpolated_Verbatim()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(string s)
    {
        s = @""a"" + $@""b"";
        s = $@""a"" + @""b"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task TestNoDiagnostic_AddExpressionIsNotStringConcatenation()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(string s)
    {
        s = default(C) + ""a"" + ""b"";
    }

    public static string operator +(C left, string right) => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task TestNoDiagnostic_LiteralsMixedWithExpressions()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M(string s1, string s2, string s3)
    {
        s1 = s1 + ""a"" + s2 +  ""b"" + s3;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.JoinStringExpressions)]
        public async Task TestNoDiagnostic_HexadecimalDigit()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Text.RegularExpressions;
class C
{
    private static readonly Regex _regex = new Regex(""\x1A"" + ""E\\d+E"");

    void M(string s)
    {
        s = ""a"" + ""\x1"" + ""b"";
        s = ""a"" + ""\x12"" + ""b"";
        s = ""a"" + ""\x123"" + ""b"";

        s = ""a"" + $""{s}\x1"" + ""b"";
        s = ""a"" + $""{s}\x12"" + ""b"";
        s = ""a"" + $""{s}\x123"" + ""b"";
    }
}
");
        }
    }
}
