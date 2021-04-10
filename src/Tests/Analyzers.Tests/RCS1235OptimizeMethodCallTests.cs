// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1235OptimizeMethodCallTests : AbstractCSharpDiagnosticVerifier<InvocationExpressionAnalyzer, OptimizeMethodCallCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.OptimizeMethodCall;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task Test_CallCompareOrdinalInsteadOfCompare()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        string x = null;
        string y = null;

        var result = [|string.Compare(x, y, StringComparison.Ordinal)|];
    }
}
", @"
using System;

class C
{
    void M()
    {
        string x = null;
        string y = null;

        var result = string.CompareOrdinal(x, y);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task Test_CallEqualsInsteadOfCompare()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        string x = null;
        string y = null;

        if ([|string.Compare(x, y, StringComparison.Ordinal) == 0|])
        {
        }
    }
}
", @"
using System;

class C
{
    void M()
    {
        string x = null;
        string y = null;

        if (string.Equals(x, y, StringComparison.Ordinal))
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task Test_CallNotEqualsInsteadOfCompare()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        string x = null;
        string y = null;

        if ([|string.Compare(x, y, StringComparison.Ordinal) != 0|])
        {
        }
    }
}
", @"
using System;

class C
{
    void M()
    {
        string x = null;
        string y = null;

        if (!string.Equals(x, y, StringComparison.Ordinal))
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task Test_CallStringConcatInsteadOfStringJoin_EmptyStringLiteral()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = [|string.Join("""", default(object), default(object))|];
    }
}
", @"
class C
{
    void M()
    {
        string s = string.Concat(default(object), default(object));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task Test_CallStringConcatInsteadOfStringJoin_EmptyStringLiteral2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = [|string.Join("""", ""a"", ""b"")|];
    }
}
", @"
class C
{
    void M()
    {
        string s = string.Concat(""a"", ""b"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task Test_CallStringConcatInsteadOfStringJoin_StringEmpty()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = [|string.Join(string.Empty, new string[] { """" })|];
    }
}
", @"
class C
{
    void M()
    {
        string s = string.Concat(new string[] { """" });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task Test_CallStringConcatInsteadOfStringJoin_EmptyStringConstant()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    const string EmptyString = """";

    void M()
    {
        string s = [|string.Join(EmptyString, new object[] { """" })|];
    }
}
", @"
class C
{
    const string EmptyString = """";

    void M()
    {
        string s = string.Concat(new object[] { """" });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task Test_CallDebugFailInsteadOfDebugAssert()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Diagnostics;

class C
{
    void M()
    {
        [|Debug.Assert(false)|];
    }
}
", @"
using System.Diagnostics;

class C
{
    void M()
    {
        Debug.Fail("""");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task Test_CallDebugFailInsteadOfDebugAssert2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Diagnostics;

class C
{
    void M()
    {
        [|Debug.Assert(false, ""x"")|];
    }
}
", @"
using System.Diagnostics;

class C
{
    void M()
    {
        Debug.Fail(""x"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task Test_CallDebugFailInsteadOfDebugAssert3()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Diagnostics;

class C
{
    void M()
    {
        [|Debug.Assert(false, ""x"", ""y"")|];
    }
}
", @"
using System.Diagnostics;

class C
{
    void M()
    {
        Debug.Fail(""x"", ""y"");
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task Test_OptimizeDictionaryContainsKey()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var dic = new Dictionary<string, string>();
        string key = null;
        string value = null;

        [|if (dic.ContainsKey(key))
        {
            dic[key] = value;
        }
        else
        {
            dic.Add(key, value);
        }|]
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var dic = new Dictionary<string, string>();
        string key = null;
        string value = null;

        dic[key] = value;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task Test_OptimizeDictionaryContainsKey_EmbeddedStatement()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var dic = new Dictionary<string, string>();
        string key = null;
        string value = null;

        [|if (dic.ContainsKey(key))
            dic[key] = value;
        else
            dic.Add(key, value);|]
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var dic = new Dictionary<string, string>();
        string key = null;
        string value = null;

        dic[key] = value;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task Test_OptimizeDictionaryContainsKey_LogicalNot()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    void M()
    {
        var dic = new Dictionary<string, string>();
        string key = null;
        string value = null;

        [|if (!dic.ContainsKey(key))
        {
            dic.Add(key, value);
        }
        else
        {
            dic[key] = value;
        }|]
    }
}
", @"
using System.Collections.Generic;

class C
{
    void M()
    {
        var dic = new Dictionary<string, string>();
        string key = null;
        string value = null;

        dic[key] = value;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task TestNoDiagnostic_CallCompareOrdinalInsteadOfCompare_NotStringComparisonOrdinal()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        string x = null;
        string y = null;

        var result = string.Compare(x, y, StringComparison.CurrentCulture);
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OptimizeMethodCall)]
        public async Task TestNoDiagnostic_CallStringConcatInsteadOfStringJoin()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class C
{
    void M()
    {
        string s = string.Join(""x"", new object[] { """" });
    }
}
");
        }
    }
}
