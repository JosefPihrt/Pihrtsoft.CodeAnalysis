// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1179UnnecessaryAssignmentTests : AbstractCSharpDiagnosticVerifier<UnnecessaryAssignmentAnalyzer, UnnecessaryAssignmentCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UnnecessaryAssignment;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryAssignment)]
        public async Task Test_IfStatement()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    int M()
    {
        bool f = false;
        int x = 1; // x
        [|if (f)
        {
            x = 2;
        }
        else if (f)
        {
            x = 3;
        }|]

        return x;
    }
}
", @"
class C
{
    int M()
    {
        bool f = false;
        if (f)
        {
            return 2;
        }
        else if (f)
        {
            return 3;
        }

        return 1; // x
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryAssignment)]
        public async Task Test_IfStatement2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    int M()
    {
        bool f = false;

        // x
        int x = 1; 
        [|if (f)
        {
            x = 2;
        }
        else if (f)
        {
            x = 3;
        }|]

        return x; // 1
    }
}
", @"
class C
{
    int M()
    {
        bool f = false;

        // x
        if (f)
        {
            return 2;
        }
        else if (f)
        {
            return 3;
        }

        return 1; // 1
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryAssignment)]
        public async Task Test_IfStatement_Throw()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    int M()
    {
        bool f = false;

        int x = 1;
        [|if (f)
        {
            x = 2;
        }
        else if (f)
        {
            x = 3;
        }
        else
        {
            throw new Exception();
        }|]

        return x;
    }
}
", @"
using System;

class C
{
    int M()
    {
        bool f = false;

        int x = 1;
        if (f)
        {
            return 2;
        }
        else if (f)
        {
            return 3;
        }
        else
        {
            throw new Exception();
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryAssignment)]
        public async Task Test_SwitchStatement()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    int M()
    {
        string s = null;
        int x = 1; // x
        [|switch (s)
        {
            case ""a"":
                {
                    x = 2;
                    break;
                }
            case ""b"":
                x = 3;
                break;
        }|]

        return x;
    }
}
", @"
class C
{
    int M()
    {
        string s = null;
        switch (s)
        {
            case ""a"":
                {
                    return 2;
                }
            case ""b"":
                return 3;
        }

        return 1; // x
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryAssignment)]
        public async Task Test_SwitchStatement2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    int M()
    {
        string s = null;

        // x
        int x = 1;
        [|switch (s)
        {
            case ""a"":
                {
                    x = 2;
                    break;
                }
            case ""b"":
                x = 3;
                break;
        }|]

        return x; // 1
    }
}
", @"
class C
{
    int M()
    {
        string s = null;

        // x
        switch (s)
        {
            case ""a"":
                {
                    return 2;
                }
            case ""b"":
                return 3;
        }

        return 1; // 1
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UnnecessaryAssignment)]
        public async Task Test_SwitchStatement_Throw()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    int M()
    {
        string s = null;

        int x = 1;
        [|switch (s)
        {
            case ""a"":
                {
                    x = 2;
                    break;
                }
            case ""b"":
                x = 3;
                break;
            default:
                throw new Exception();
        }|]

        return x;
    }
}
", @"
using System;

class C
{
    int M()
    {
        string s = null;

        int x = 1;
        switch (s)
        {
            case ""a"":
                {
                    return 2;
                }
            case ""b"":
                return 3;
            default:
                throw new Exception();
        }
    }
}
");
        }
    }
}
