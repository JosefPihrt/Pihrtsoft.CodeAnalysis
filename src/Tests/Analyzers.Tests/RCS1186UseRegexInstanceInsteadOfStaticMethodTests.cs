// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1186UseRegexInstanceInsteadOfStaticMethodTests : AbstractCSharpDiagnosticVerifier<InvocationExpressionAnalyzer, UseRegexInstanceInsteadOfStaticMethodCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.UseRegexInstanceInsteadOfStaticMethod;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_IsMatch1()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    protected Regex _regex;

    class C2
    {
        private static readonly string _input = """";

        void M()
        {
            bool isMatch = Regex.[|IsMatch|](_input, (""pattern""));
        }
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    protected Regex _regex;

    class C2
    {
        private static readonly string _input = """";
        private static readonly Regex _regex2 = new Regex((""pattern""));

        void M()
        {
            bool isMatch = _regex2.IsMatch(_input);
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_IsMatch2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private const string _pattern = """";

    void M()
    {
        bool isMatch = Regex.[|IsMatch|](_input, _pattern);
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly Regex _regex = new Regex(_pattern);
    private const string _pattern = """";

    void M()
    {
        bool isMatch = _regex.IsMatch(_input);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_IsMatch3()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";

    void M()
    {
        bool isMatch = Regex.[|IsMatch|](_input, ""pattern"", RegexOptions.Singleline);
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly Regex _regex = new Regex(""pattern"", RegexOptions.Singleline);

    void M()
    {
        bool isMatch = _regex.IsMatch(_input);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_IsMatch4()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";

    void M()
    {
        bool isMatch = Regex.[|IsMatch|](_input, ""pattern"", RegexOptions.Singleline | RegexOptions.Multiline);
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly Regex _regex = new Regex(""pattern"", RegexOptions.Singleline | RegexOptions.Multiline);

    void M()
    {
        bool isMatch = _regex.IsMatch(_input);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_Match1()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";

    void M()
    {
        Match match = Regex.[|Match|](_input, ""pattern"");
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly Regex _regex = new Regex(""pattern"");

    void M()
    {
        Match match = _regex.Match(_input);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_Match2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";

    void M()
    {
        Match match = Regex.[|Match|](_input, ""pattern"", RegexOptions.None);
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly Regex _regex = new Regex(""pattern"", RegexOptions.None);

    void M()
    {
        Match match = _regex.Match(_input);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_Matches1()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";

    void M()
    {
        MatchCollection matches = Regex.[|Matches|](_input, ""pattern"");
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly Regex _regex = new Regex(""pattern"");

    void M()
    {
        MatchCollection matches = _regex.Matches(_input);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_Matches2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";

    void M()
    {
        MatchCollection matches = Regex.[|Matches|](_input, ""pattern"", RegexOptions.None);
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly Regex _regex = new Regex(""pattern"", RegexOptions.None);

    void M()
    {
        MatchCollection matches = _regex.Matches(_input);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_Split1()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";

    void M()
    {
        var values = Regex.[|Split|](_input, ""pattern"");
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly Regex _regex = new Regex(""pattern"");

    void M()
    {
        var values = _regex.Split(_input);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_Split2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";

    void M()
    {
        var values = Regex.[|Split|](_input, ""pattern"", RegexOptions.None);
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly Regex _regex = new Regex(""pattern"", RegexOptions.None);

    void M()
    {
        var values = _regex.Split(_input);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_Replace1()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private const string _pattern = """";
    private static readonly string _replacement = """";

    void M()
    {
        string s = Regex.[|Replace|](_input, _pattern, _replacement);
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private const string _pattern = """";
    private static readonly string _replacement = """";
    private static readonly Regex _regex = new Regex(_pattern);

    void M()
    {
        string s = _regex.Replace(_input, _replacement);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_Replace2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly string _replacement = """";

    void M()
    {
        string s = Regex.[|Replace|](_input, ""pattern"", _replacement, RegexOptions.None);
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly string _replacement = """";
    private static readonly Regex _regex = new Regex(""pattern"", RegexOptions.None);

    void M()
    {
        string s = _regex.Replace(_input, _replacement);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_Replace3()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly string _replacement = """";

    void M()
    {
        string s = Regex.[|Replace|](_input, ""pattern"", default(MatchEvaluator));
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly string _replacement = """";
    private static readonly Regex _regex = new Regex(""pattern"");

    void M()
    {
        string s = _regex.Replace(_input, default(MatchEvaluator));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_Replace4()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly string _replacement = """";

    void M()
    {
        string s = Regex.[|Replace|](_input, ""pattern"", default(MatchEvaluator), RegexOptions.None);
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly string _replacement = """";
    private static readonly Regex _regex = new Regex(""pattern"", RegexOptions.None);

    void M()
    {
        string s = _regex.Replace(_input, default(MatchEvaluator));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_LambdaExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";

    void M()
    {
        Action<object> action = f => { Match match = Regex.[|Match|](_input, ""pattern""); };
    }
}
", @"
using System;
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly Regex _regex = new Regex(""pattern"");

    void M()
    {
        Action<object> action = f => { Match match = _regex.Match(_input); };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task Test_Property()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private const string _pattern = """";

    string P
    {
        get
        {
            Match match = Regex.[|Match|](_input, _pattern);
            return null;
        }
    }
}
", @"
using System.Text.RegularExpressions;

class C
{
    private static readonly string _input = """";
    private static readonly Regex _regex = new Regex(_pattern);
    private const string _pattern = """";

    string P
    {
        get
        {
            Match match = _regex.Match(_input);
            return null;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task TestNoDiagnostic_InstanceCall()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Text.RegularExpressions;

class C
{
    void M()
    {
        Regex regex = null;

        bool isMatch = regex.IsMatch(""pattern"");

        Match match = regex.Match(""pattern"");

        MatchCollection matches = regex.Matches(""pattern"");

        string[] values = regex.Split(""pattern"");

        string value = regex.Replace(""pattern"", ""replacement"");

        Action<object> action = f => { Match m = regex.Match(""pattern""); };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task TestNoDiagnostic_PatternIsLocalConst()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Text.RegularExpressions;

class C
{
    private readonly string _pattern;

    void M()
    {
        const string pattern = """";

        bool isMatch = Regex.IsMatch(""input"", pattern);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task TestNoDiagnostic_OptionsIsLocalConst()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Text.RegularExpressions;

class C
{
    private readonly string _pattern;

    void M()
    {
        const RegexOptions options = RegexOptions.None;

        bool isMatch2 = Regex.IsMatch(""input"", ""pattern"", options);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseRegexInstanceInsteadOfStaticMethod)]
        public async Task TestNoDiagnostic_NonConstValue()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Text.RegularExpressions;

class C
{
    private readonly string _pattern;

    void M()
    {
        bool isMatch1 = Regex.IsMatch(""input"", _pattern);
        bool isMatch2 = Regex.IsMatch(""input"", ""pattern"", RegexOptions.None, TimeSpan.Zero);

        Match match = Regex.Match(""input"", ""pattern"", RegexOptions.None, TimeSpan.Zero);

        MatchCollection matches = Regex.Matches(""input"", ""pattern"", RegexOptions.None, TimeSpan.Zero);

        string[] values = Regex.Split(""input"", ""pattern"", RegexOptions.None, TimeSpan.Zero);

        string value1 = Regex.Replace(""input"", ""pattern"", default(MatchEvaluator), RegexOptions.None, TimeSpan.Zero);
        string value2 = Regex.Replace(""input"", ""pattern"", ""replacement"", RegexOptions.None, TimeSpan.Zero);
    }
}
");
        }
    }
}
