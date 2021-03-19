// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1212RemoveRedundantAssignmentTests : AbstractCSharpDiagnosticVerifier<RemoveRedundantAssignmentAnalyzer, RemoveRedundantAssignmentCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.RemoveRedundantAssignment;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAssignment)]
        public async Task Test_Local()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    bool M()
    {
        bool f = false;
        bool g = false;

        [|f = false|];
        return f;
    }
}
", @"
class C
{
    bool M()
    {
        bool f = false;
        bool g = false;

        return false;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAssignment)]
        public async Task Test_Parameter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    bool M(bool f)
    {
        [|f = false|];
        return f;
    }
}
", @"
class C
{
    bool M(bool f)
    {
        return false;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAssignment)]
        public async Task Test_Local_WithComment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M()
    {
        string s = null;
        M2();
        [|s = """"|]; //x
        return s;
    }

    void M2()
    {
    }
}
", @"
class C
{
    string M()
    {
        string s = null;
        M2();
        //x
        return """";
    }

    void M2()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAssignment)]
        public async Task Test_Local_ReferencedInRightSideOfAssignment()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M()
    {
        string s = """";
        [|s = s + s|];
        return s;
    }
}
", @"
class C
{
    string M()
    {
        string s = """";
        return s + s;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAssignment)]
        public async Task Test_LocalDeclaration()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string [|s|];
        s = null;
    }
}
", @"
class C
{
    void M()
    {
        string s = null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAssignment)]
        public async Task Test_LocalInsideLambda()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        M2(() =>
        {
            int x = 0;
            M();

            [|x = 1|];
            return x;
        });
    }

    int M2(Func<int> p) => 0;
}
", @"
using System;

class C
{
    void M()
    {
        M2(() =>
        {
            int x = 0;
            M();

            return 1;
        });
    }

    int M2(Func<int> p) => 0;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAssignment)]
        public async Task Test_ParameterInsideLambda()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    void M()
    {
        M2((x) =>
        {
            [|x = 1|];
            return x;
        });
    }

    int M2(Func<int, int> p) => 0;
}
", @"
using System;

class C
{
    void M()
    {
        M2((x) =>
        {
            return 1;
        });
    }

    int M2(Func<int, int> p) => 0;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAssignment)]
        public async Task TestNoDiagnostic_OutParameter()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    bool M(out bool f)
    {
        f = false;
        return f;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAssignment)]
        public async Task TestNoDiagnostic_SequenceOfAssignments()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    int M()
    {
        int x = 1;
        x = x * 2;
        x = x * 2;
        return x;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAssignment)]
        public async Task TestNoDiagnostic_LocalReferencedInLambda()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    C M()
    {
        C c = null;
        c = new C(f =>
        {
            c.M(f);
        });

        return c;
    }

    public C(Action<int> action)
    {
        _a = action;
    }

    void M(int p)
    {
    }

    Action<int> _a;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAssignment)]
        public async Task TestNoDiagnostic_LocalAssignedInsideLambda()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M()
    {
        int x = 0;

        M2(() =>
        {
            x = 1;
            return x;
        });
    }

    int M2(Func<int> p) => 0;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveRedundantAssignment)]
        public async Task TestNoDiagnostic_ParameterAssignedInsideLambda()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    void M(int x)
    {
        M2(() =>
        {
            x = 1;
            return x;
        });
    }

    int M2(Func<int> p) => 0;
}
");
        }
    }
}
