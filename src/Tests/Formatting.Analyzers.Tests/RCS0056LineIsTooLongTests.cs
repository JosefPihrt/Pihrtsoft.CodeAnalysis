// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.LineIsTooLong;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0056LineIsTooLongTests : AbstractCSharpDiagnosticVerifier<LineIsTooLongAnalyzer, LineIsTooLongCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.LineIsTooLong;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_ExpressionBody_AddNewLineBeforeArrow()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
[|    string M(object ppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppp) => null;|]
}
",
@"
class C
{
    string M(object ppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppp)
        => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_ExpressionBody_AddNewLineAfterArrow()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
[|    string M(object pppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppp) => null;|]
}
",
@"
class C
{
    string M(object pppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppp) =>
        null;
}
", options: Options.EnableDiagnostic(DiagnosticDescriptors.AddNewLineBeforeExpressionBodyArrowInsteadOfAfterItOrViceVersa, AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterExpressionBodyArrowInsteadOfBeforeIt));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_ParameterList()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
[|    void M(object x, object y, object z, object xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)|]
    {
    }
}
",
@"
class C
{
    void M(
        object x,
        object y,
        object z,
        object xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)
    {
    }
}
", options: Options.EnableDiagnostic(DiagnosticDescriptors.AddNewLineBeforeExpressionBodyArrowInsteadOfAfterItOrViceVersa, AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterExpressionBodyArrowInsteadOfBeforeIt));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_ArgumentList()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(
        object x,
        object y,
        object z,
        object xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)
    {
[|        M(x, y, z, xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx);|]
    }
}
",
@"
class C
{
    void M(
        object x,
        object y,
        object z,
        object xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)
    {
        M(
            x,
            y,
            z,
            xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_ArgumentList_PreferOuterArgumentList()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M(
        object x,
        object y,
        object z,
        string xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)
    {
[|        return M(x, y, z, M(x, y, z, xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx));|]
    }
}
",
@"
class C
{
    string M(
        object x,
        object y,
        object z,
        string xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)
    {
        return M(
            x,
            y,
            z,
            M(x, y, z, xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_PreferArgumentListOverCallChain()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    C M(
        object x,
        object y,
        object zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz)
    {
[|        return M(x, y, zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz).M(x, y, zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz);|]
    }
}
",
@"
class C
{
    C M(
        object x,
        object y,
        object zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz)
    {
        return M(x, y, zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz).M(
            x,
            y,
            zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_PreferArgumentListOverCallChain2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    C M(
        string x,
        string yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy)
    {
[|        return M(x.ToString(), yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy.ToString().ToString().ToString());|]
    }
}
",
@"
class C
{
    C M(
        string x,
        string yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy)
    {
        return M(
            x.ToString(),
            yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy.ToString().ToString().ToString());
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_PreferArgumentListOverCallChain_WhenLeftIsSimpleName()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

[|        foreach ((string ffff, string gggg) item in items.Join(items, ffff => ffff, ffff => ffff, (ffff, gggg) => (ffff, gggg)))|]
        {
        }
    }
}
",
@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        foreach ((string ffff, string gggg) item in items.Join(
            items,
            ffff => ffff,
            ffff => ffff,
            (ffff, gggg) => (ffff, gggg)))
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_PreferArgumentListOverCallChain_WhenLeftIsCastExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

[|        foreach ((string ffff, string gggg) item in ((IEnumerable<string>)items).Join(items, ffff => ffff, ffff => ffff, (ffff, gggg) => (ffff, gggg)))|]
        {
        }
    }
}
",
@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        var items = new List<string>();

        foreach ((string ffff, string gggg) item in ((IEnumerable<string>)items).Join(
            items,
            ffff => ffff,
            ffff => ffff,
            (ffff, gggg) => (ffff, gggg)))
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_PreferArgumentListOverBinaryExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
[|        if (string.Compare(""xxxxxxxxxxxxxxxxxxxxxx"", 0, ""xxxxxxxxxxxxxxxxxxxxxx"", 1 + 1, 0, StringComparison.OrdinalIgnoreCase) == 0)|]
        {
        }
    }
}
",
@"
using System;

class C
{
    void M()
    {
        if (string.Compare(
            ""xxxxxxxxxxxxxxxxxxxxxx"",
            0,
            ""xxxxxxxxxxxxxxxxxxxxxx"",
            1 + 1,
            0,
            StringComparison.OrdinalIgnoreCase) == 0)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_PreferArgumentListOverBinaryExpression2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        string[] items = null;

[|        items[items.Length - 1] = items[items.Length - 1].Mxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx(items.Skip(items.Length));|]
    }
}

static class E
{
    public static string Mxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx(this string s, IEnumerable<string> items) => null;
}
",
@"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
        string[] items = null;

        items[items.Length - 1] = items[items.Length - 1].Mxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx(
            items.Skip(items.Length));
    }
}

static class E
{
    public static string Mxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx(this string s, IEnumerable<string> items) => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_BracketedArgumentList()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M()
    {
        var c = new C();

[|        return c[""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx""];|]
    }

    string this[string p] => null;
}
",
@"
class C
{
    string M()
    {
        var c = new C();

        return c[
            ""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx""];
    }

    string this[string p] => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_CallChain()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string Xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx()
    {
[|        return Xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx().ToString().ToString().ToString()|]
            .ToString();
    }
}
",
@"
class C
{
    string Xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx()
    {
        return Xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx().ToString()
            .ToString()
            .ToString()
            .ToString();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_CallChain_SimpleMemberAccess()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    int M()
    {
[|        return C.Pxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.Length;|]
    }

    static string Pxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        => default;
}
",
@"
class C
{
    int M()
    {
        return C.Pxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            .Length;
    }

    static string Pxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        => default;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_PreferCallChainOverArgumentList()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx = null;

[|        var x = xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.Length.ToString(xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.ToString());|]
    }
}
",
@"
class C
{
    void M()
    {
        string xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx = null;

        var x = xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.Length
            .ToString(xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.ToString());
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_PreferCallChainOverArgumentList2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx = null;

[|        var x = xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.Length.ToString(xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.ToString())|]
            .ToString();
    }
}
",
@"
class C
{
    void M()
    {
        string xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx = null;

        var x = xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.Length
            .ToString(xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.ToString())
            .ToString();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_PreferCallChainOverArgumentList3()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string x = null;

        if (
[|                    x.Length.ToString(""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"")?.Length == 0)|]
        {
        }
    }
}
",
@"
class C
{
    void M()
    {
        string x = null;

        if (
                    x.Length.ToString(""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"")?
                        .Length == 0)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_BinaryExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool xxxxxxxxxxxxxxxxxxxxxxxxxx = false;

[|        if (xxxxxxxxxxxxxxxxxxxxxxxxxx && xxxxxxxxxxxxxxxxxxxxxxxxxx && xxxxxxxxxxxxxxxxxxxxxxxxxx && xxxxxxxxxxxxxxxxxxxxxxxxxx && xxxxxxxxxxxxxxxxxxxxxxxxxx)|]
        {
        }
    }
}
",
@"
class C
{
    void M()
    {
        bool xxxxxxxxxxxxxxxxxxxxxxxxxx = false;

        if (xxxxxxxxxxxxxxxxxxxxxxxxxx && xxxxxxxxxxxxxxxxxxxxxxxxxx && xxxxxxxxxxxxxxxxxxxxxxxxxx
            && xxxxxxxxxxxxxxxxxxxxxxxxxx
            && xxxxxxxxxxxxxxxxxxxxxxxxxx)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_InitializerExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string sssssssssssssssssssssssssssss = null;
[|        var arr = new string[] { sssssssssssssssssssssssssssss, sssssssssssssssssssssssssssss, sssssssssssssssssssssssssssss };|]
    }
}
",
@"
class C
{
    void M()
    {
        string sssssssssssssssssssssssssssss = null;
        var arr = new string[] {
            sssssssssssssssssssssssssssss,
            sssssssssssssssssssssssssssss,
            sssssssssssssssssssssssssssss };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_PropertyInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
[|    string P { get; } = """".ToString().ToString().ToString().ToString().ToString().ToString().ToString().ToString().ToString();|]
}
",
@"
class C
{
    string P { get; }
        = """".ToString().ToString().ToString().ToString().ToString().ToString().ToString().ToString().ToString();
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_FieldInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
[|    private string _f = ""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"";|]
}
",
@"
class C
{
    private string _f
        = ""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"";
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_LocalDeclaration()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
[|        var xxxxxxxxxxxxxxxx = ""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"";|]
    }
}
",
@"
class C
{
    void M()
    {
        var xxxxxxxxxxxxxxxx
            = ""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_Assignment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    static string Mxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx()
    {
[|        string x = Mxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx();|]

        return null;
    }
}
",
@"
class C
{
    static string Mxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx()
    {
        string x
            = Mxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx();

        return null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_AttributeList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
[|    [Obsolete(""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx""), Foo]|]
    void M()
    {
    }
}

class FooAttribute : Attribute
{
}
",
@"
using System;

class C
{
    [Obsolete(""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx""),
        Foo]
    void M()
    {
    }
}

class FooAttribute : Attribute
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_AttributeArgumentList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
[|    [Obsolete(""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"", error: false)]|]
    void M()
    {
    }
}

class FooAttribute : Attribute
{
}
",
@"
using System;

class C
{
    [Obsolete(
        ""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"",
        error: false)]
    void M()
    {
    }
}

class FooAttribute : Attribute
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_ConditionalExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        bool f = false;

[|        var x = f ? ""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"" : """";|]
    }
}
",
@"
class C
{
    void M()
    {
        bool f = false;

        var x = f
            ? ""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx""
            : """";
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_PreferConditionalExpressionOverCallChain()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = null;
        bool f = false;

[|        var x = f ? 0.ToString(""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"") : 1.ToString(""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"");|]
    }
}
",
@"
class C
{
    void M()
    {
        string s = null;
        bool f = false;

        var x = f
            ? 0.ToString(""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"")
            : 1.ToString(""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_ForStatement()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx = new List<string>();

[|        for (int i = 0; i < xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.Count; i++)|]
        {
        }
    }
}
",
@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx = new List<string>();

        for (
            int i = 0;
            i < xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.Count;
            i++)
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_BaseList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections;
using System.Collections.Generic;

namespace N
{
[|    interface IC : Xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx, IEnumerable, IEnumerable<object>|]
    {
    }

    interface Xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    {
    }
}
",
@"
using System.Collections;
using System.Collections.Generic;

namespace N
{
    interface IC :
        Xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx,
        IEnumerable,
        IEnumerable<object>
    {
    }

    interface Xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task Test_BaseList_Constraint()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections;
using System.Collections.Generic;

namespace N
{
[|    interface IC<T> : Xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx, IEnumerable, IEnumerable<T> where T : IComparer|]
    {
    }

    interface Xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    {
    }
}
",
@"
using System.Collections;
using System.Collections.Generic;

namespace N
{
    interface IC<T> :
        Xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx,
        IEnumerable,
        IEnumerable<T>
        where T : IComparer
    {
    }

    interface Xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task TestNoFix_ExpressionBody_TooLongAfterWrapping()
        {
            await VerifyDiagnosticAndNoFixAsync(@"
class C
{
[|    string Foooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo() => null;|]
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task TestNoFix_ExpressionBody_TooLongAfterWrapping2()
        {
            await VerifyDiagnosticAndNoFixAsync(@"
class C
{
[|    string Fooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo() => null;|]
}
", options: Options.EnableDiagnostic(DiagnosticDescriptors.AddNewLineBeforeExpressionBodyArrowInsteadOfAfterItOrViceVersa, AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterExpressionBodyArrowInsteadOfBeforeIt));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task TestNoFix_ExpressionBody_AlreadyWrapped()
        {
            await VerifyDiagnosticAndNoFixAsync(
@"
class C
{
    string M(object p)
[|        => ""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"";|]
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task TestNoFix_ExpressionBody_AlreadyWrapped2()
        {
            await VerifyDiagnosticAndNoFixAsync(@"
class C
{
    string M(object p) =>
[|        ""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"";|]
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task TestNoDiagnostic_Banner()
        {
            await VerifyNoDiagnosticAsync(@"//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task TestNoDiagnostic_DoNotWrapNameof()
        {
            await VerifyDiagnosticAndNoFixAsync(@"
class C
{
    string M(string xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)
    {
[|        return nameof(xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx);|]
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task TestNoDiagnostic_DocumentationComment()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    /// <summary>
    /// xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    /// </summary>
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task TestNoDiagnostic_SingleLineComment()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    
    // xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    void M()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task TestNoDiagnostic_StringLiteral()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        string s = @""
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
"";

        s =
@""xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx""
;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task TestNoDiagnostic_ThisExpression_BaseExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C : B
{

    void M()
    {
        var c = new C();

[|        var x = this.Pxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx;|]

[|        var y = base.Pxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx;|]
    }
}

class B
{
    public C Pxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        { get; }
}
"
,
@"
class C : B
{

    void M()
    {
        var c = new C();

        var x
            = this.Pxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx;

        var y
            = base.Pxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx;
    }
}

class B
{
    public C Pxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        { get; }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task TestNoDiagnostic_EnumField()
        {
            await VerifyDiagnosticAndNoFixAsync(@"
using System.Text.RegularExpressions;

class C
{

    void M()
    {
[|        var x =                                                                                               RegexOptions.None;|]
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.LineIsTooLong)]
        public async Task TestNoDiagnostic_Namespace()
        {
            await VerifyDiagnosticAndNoFixAsync(@"
using System.Text.RegularExpressions;

class C
{

    void M()
    {
[|        var x =                                                                             System.Text.RegularExpressions.RegexOptions.None;|]
[|        var y =                                                                                        global::System.Text.RegularExpressions.RegexOptions.None;|]
    }
}
");
        }
    }
}
