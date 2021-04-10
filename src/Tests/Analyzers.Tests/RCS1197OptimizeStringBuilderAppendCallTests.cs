// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1197OptimizeStringBuilderAppendCallTests : AbstractCSharpDiagnosticVerifier<InvocationExpressionAnalyzer, OptimizeStringBuilderAppendCallCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.OptimizeStringBuilderAppendCall;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_Substring_Int32_Int32()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append([|s.Substring(0, 2)|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(s, 0, 2);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_Substring_Int32()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append([|s.Substring(2)|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(s, 2, s.Length - 2);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_Substring_Int32_Int32_AppendLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.AppendLine([|s.Substring(0, 2)|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(s, 0, 2).AppendLine();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_Substring_Int32_AppendLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.AppendLine([|s.Substring(2)|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(s, 2, s.Length - 2).AppendLine();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_Remove()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append([|s.Remove(2)|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(s, 0, 2);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_Remove_AppendLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.AppendLine([|s.Remove(2)|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(s, 0, 2).AppendLine();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_StringFormat()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append([|string.Format(""f"", s)|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.AppendFormat(""f"", s);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_StringFormat_AppendLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.AppendLine([|string.Format(""f"", s)|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.AppendFormat(""f"", s).AppendLine();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_InterpolatedString()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append([|$""{s}s""|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(s).Append('s');
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_InterpolatedString_Braces()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(
            [|$""a{{b}}c{s}a{{b}}c""|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(
            ""a{b}c"").Append(s).Append(""a{b}c"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_InterpolatedString_Char()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append([|$""\""{s}'""|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append('\""').Append(s).Append('\'');
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_InterpolatedString_AppendLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.AppendLine([|$""{s}s""|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(s).AppendLine(""s"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_InterpolatedString_AppendLine2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        var sb = new StringBuilder();

        sb.AppendLine([|$""ab{'s'}""|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        var sb = new StringBuilder();

        sb.Append(""ab"").Append('s').AppendLine();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_InterpolatedString_AppendLine3()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        var sb = new StringBuilder();

        sb.AppendLine([|$""ab{'s'}s""|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        var sb = new StringBuilder();

        sb.Append(""ab"").Append('s').AppendLine(""s"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_InterpolatedString_WithFormat_AppendLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.AppendLine([|$""{s,1:f}""|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.AppendFormat(""{0,1:f}"", s).AppendLine();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_Concatenation()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append([|""ab"" + s + ""cd""|]).Append([|""ef"" + s + ""gh""|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(""ab"").Append(s).Append(""cd"").Append(""ef"").Append(s).Append(""gh"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_Concatentation_Char()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        char a = 'a';

        var sb = new StringBuilder();

        sb.Append([|a + ""b"" + ""c""|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        char a = 'a';

        var sb = new StringBuilder();

        sb.Append(a).Append('b').Append('c');
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_Concatenation_AppendLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append([|s + ""ab""|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(s).Append(""ab"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_Concatenation_AppendLine2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.AppendLine([|""ab"" + s|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(""ab"").AppendLine(s);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_Concatenation_AppendLine3()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        int i = 0;
        var sb = new StringBuilder();

        sb.AppendLine([|""ab"" + i|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        int i = 0;
        var sb = new StringBuilder();

        sb.Append(""ab"").Append(i).AppendLine();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_Concatenation_AppendLine4()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        object o = null;
        var sb = new StringBuilder();

        sb.AppendLine([|""ab"" + o|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        object o = null;
        var sb = new StringBuilder();

        sb.Append(""ab"").Append(o).AppendLine();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_Concatenation_AppendLine5()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.AppendLine([|""ab"" + s + ""b""|]).AppendLine([|""ef"" + s + ""d""|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(""ab"").Append(s).AppendLine(""b"").Append(""ef"").Append(s).AppendLine(""d"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_AppendLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        var sb = new StringBuilder();

        sb.Append("""").[|AppendLine|]();
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        var sb = new StringBuilder();

        sb.AppendLine("""");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_AppendLine2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.Append(s).[|AppendLine|]();
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        var sb = new StringBuilder();

        sb.AppendLine(s);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task Test_Join()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text;

class C
{
    void M()
    {
        var sb = new StringBuilder();

        sb.Append([|string.Join('x', """", """")|]);
        sb.Append([|string.Join('x', default(object), default(object))|]);
        sb.Append([|string.Join(""x"", """", """")|]);
        sb.Append([|string.Join(""x"", default(object), default(object))|]);
        sb.Append([|string.Join('x', new[] { """" })|]);
        sb.Append([|string.Join(""x"", new[] { default(object) })|]);
    }
}
", @"
using System.Text;

class C
{
    void M()
    {
        var sb = new StringBuilder();

        sb.AppendJoin('x', """", """");
        sb.AppendJoin('x', default(object), default(object));
        sb.AppendJoin(""x"", """", """");
        sb.AppendJoin(""x"", default(object), default(object));
        sb.AppendJoin('x', new[] { """" });
        sb.AppendJoin(""x"", new[] { default(object) });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task TestNoDiagnostic_Const()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Text;

class C
{
    void M()
    {
        const string s = null;

        var sb = new StringBuilder();

        sb.Append(""x"" + s);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeStringBuilderAppendCall)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Text;

class C
{
    void M()
    {
        string s = null;
        int i = 0;
        object o = null;

        var sb = new StringBuilder();

        sb.Append(s.Remove(2, 3));

        sb.AppendLine(s.Remove(2, 3));

        sb.Insert(0, i);

        sb.Insert(0, o);

        sb.Append(i).AppendLine();

        sb.Append(o).AppendLine();
    }
}
");
        }
    }
}
