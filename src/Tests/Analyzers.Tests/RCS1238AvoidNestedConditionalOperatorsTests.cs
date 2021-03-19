// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1238AvoidNestedConditionalOperatorsTests : AbstractCSharpDiagnosticVerifier<ConditionalExpressionAnalyzer, ConditionalExpressionCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AvoidNestedConditionalOperators;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNestedConditionalOperators)]
        public async Task Test_LocalDeclaration()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s = [|(f) ? ((f2) ? y : z) : x|];
    }
}
", @"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s;
        if (f)
        {
            if (f2)
            {
                s = y;
            }
            else
            {
                s = z;
            }
        }
        else
        {
            s = x;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNestedConditionalOperators)]
        public async Task Test_LocalDeclaration2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s = [|(f) ? x : ((f2) ? y : z)|];
    }
}
", @"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s;
        if (f)
        {
            s = x;
        }
        else if (f2)
        {
            s = y;
        }
        else
        {
            s = z;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNestedConditionalOperators)]
        public async Task Test_LocalDeclaration3()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(bool f, bool f2, bool f3, string a, string b, string c, string d)
    {
        string s = [|(f) ? ((f2) ? a : b) : ((f3) ? c : d)|];
    }
}
", @"
class C
{
    void M(bool f, bool f2, bool f3, string a, string b, string c, string d)
    {
        string s;
        if (f)
        {
            if (f2)
            {
                s = a;
            }
            else
            {
                s = b;
            }
        }
        else if (f3)
        {
            s = c;
        }
        else
        {
            s = d;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNestedConditionalOperators)]
        public async Task Test_SimpleAssignment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s = null;
        s = [|(f) ? ((f2) ? y : z) : x|];
    }
}
", @"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s = null;
        if (f)
        {
            if (f2)
            {
                s = y;
            }
            else
            {
                s = z;
            }
        }
        else
        {
            s = x;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNestedConditionalOperators)]
        public async Task Test_SimpleAssignment2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s = null;
        s = [|(f) ? x : ((f2) ? y : z)|];
    }
}
", @"
class C
{
    void M(bool f, bool f2, string x, string y, string z)
    {
        string s = null;
        if (f)
        {
            s = x;
        }
        else if (f2)
        {
            s = y;
        }
        else
        {
            s = z;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNestedConditionalOperators)]
        public async Task Test_SimpleAssignment3()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(bool f, bool f2, bool f3, string a, string b, string c, string d)
    {
        string s = null;
        s = [|(f) ? ((f2) ? a : b) : ((f3) ? c : d)|];
    }
}
", @"
class C
{
    void M(bool f, bool f2, bool f3, string a, string b, string c, string d)
    {
        string s = null;
        if (f)
        {
            if (f2)
            {
                s = a;
            }
            else
            {
                s = b;
            }
        }
        else if (f3)
        {
            s = c;
        }
        else
        {
            s = d;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNestedConditionalOperators)]
        public async Task Test_ReturnStatement()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M(bool f, bool f2, bool f3)
    {
        return [|(f) ? ""a"" : ((f2) ? (f3) ? ""c"" : ""d"" : ""b"")|];
    }
}
", @"
class C
{
    string M(bool f, bool f2, bool f3)
    {
        if (f)
        {
            return ""a"";
        }
        else if (f2)
        {
            if (f3)
            {
                return ""c"";
            }
            else
            {
                return ""d"";
            }
        }
        else
        {
            return ""b"";
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNestedConditionalOperators)]
        public async Task Test_ReturnStatement2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M(bool f, bool f2, bool f3)
    {
        return [|(f) ? ""a"" : ((f2) ? ""b"" : (f3) ? ""c"" : ""d"")|];
    }
}
", @"
class C
{
    string M(bool f, bool f2, bool f3)
    {
        if (f)
        {
            return ""a"";
        }
        else if (f2)
        {
            return ""b"";
        }
        else if (f3)
        {
            return ""c"";
        }
        else
        {
            return ""d"";
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNestedConditionalOperators)]
        public async Task Test_ReturnStatement3()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M(bool f, bool f2, bool f3, string a, string b, string c, string d)
    {
        return [|(f) ? ((f2) ? a : b) : ((f3) ? c : d)|];
    }
}
", @"
class C
{
    string M(bool f, bool f2, bool f3, string a, string b, string c, string d)
    {
        if (f)
        {
            if (f2)
            {
                return a;
            }
            else
            {
                return b;
            }
        }
        else if (f3)
        {
            return c;
        }
        else
        {
            return d;
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNestedConditionalOperators)]
        public async Task Test_YieldReturnStatement()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(bool f, bool f2, bool f3)
    {
        yield return [|(f) ? ""a"" : ((f2) ? (f3) ? ""c"" : ""d"" : ""b"")|];
    }
}
", @"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(bool f, bool f2, bool f3)
    {
        if (f)
        {
            yield return ""a"";
        }
        else if (f2)
        {
            if (f3)
            {
                yield return ""c"";
            }
            else
            {
                yield return ""d"";
            }
        }
        else
        {
            yield return ""b"";
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNestedConditionalOperators)]
        public async Task Test_YieldReturnStatement2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(bool f, bool f2, bool f3)
    {
        yield return [|(f) ? ""a"" : ((f2) ? ""b"" : (f3) ? ""c"" : ""d"")|];
    }
}
", @"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(bool f, bool f2, bool f3)
    {
        if (f)
        {
            yield return ""a"";
        }
        else if (f2)
        {
            yield return ""b"";
        }
        else if (f3)
        {
            yield return ""c"";
        }
        else
        {
            yield return ""d"";
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidNestedConditionalOperators)]
        public async Task Test_YieldReturnStatement3()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(bool f, bool f2, bool f3, string a, string b, string c, string d)
    {
        yield return [|(f) ? ((f2) ? a : b) : ((f3) ? c : d)|];
    }
}
", @"
using System.Collections.Generic;

class C
{
    IEnumerable<string> M(bool f, bool f2, bool f3, string a, string b, string c, string d)
    {
        if (f)
        {
            if (f2)
            {
                yield return a;
            }
            else
            {
                yield return b;
            }
        }
        else if (f3)
        {
            yield return c;
        }
        else
        {
            yield return d;
        }
    }
}
");
        }
    }
}
