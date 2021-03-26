// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1207ConvertMethodGroupToAnonymousFunctionTests : AbstractCSharpDiagnosticVerifier<ConvertAnonymousFunctionToMethodGroupOrViceVersaAnalyzer, ConvertAnonymousFunctionToMethodGroupOrViceVersaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.ConvertAnonymousFunctionToMethodGroupOrViceVersa;

        public override CSharpTestOptions Options
        {
            get { return base.Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.ConvertMethodGroupToAnonymousFunction); }
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Argument()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        M2([|M|]);
    }
}
", @"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        M2(() => M());
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Argument_OneParameter()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M(object p) => null;

    void M2(Func<object, object> p)
    {
        M2([|M|]);
    }
}
", @"
using System;

class C
{
    static object M(object p) => null;

    void M2(Func<object, object> p)
    {
        M2(f => M(f));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Argument_TwoParameters()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M(object p1, object p2) => null;

    void M2(Func<object, object, object> p)
    {
        M2([|M|]);
    }
}
", @"
using System;

class C
{
    static object M(object p1, object p2) => null;

    void M2(Func<object, object, object> p)
    {
        M2((f, f2) => M(f, f2));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Argument_TwoParameters2()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M(object p1, object p2) => null;

    void M2(Func<object, object, object> p)
    {
        object f = null;
        object f2 = null;

        M2([|M|]);
    }
}
", @"
using System;

class C
{
    static object M(object p1, object p2) => null;

    void M2(Func<object, object, object> p)
    {
        object f = null;
        object f2 = null;

        M2((f3, f4) => M(f3, f4));
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Argument_WithClassName()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        M2([|C.M|]);
    }
}
", @"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        M2(() => C.M());
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Argument_WithNamespaceName()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

namespace N
{
    class C
    {
        static object M() => null;

        void M2(Func<object> p)
        {
            M2([|N.C.M|]);
        }
    }
}
", @"
using System;

namespace N
{
    class C
    {
        static object M() => null;

        void M2(Func<object> p)
        {
            M2(() => N.C.M());
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Argument_WithGlobalName()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

namespace N
{
    class C
    {
        static object M() => null;

        void M2(Func<object> p)
        {
            M2([|global::N.C.M|]);
        }
    }
}
", @"
using System;

namespace N
{
    class C
    {
        static object M() => null;

        void M2(Func<object> p)
        {
            M2(() => global::N.C.M());
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_LocalDeclaration()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        Func<object> x = [|M|];
    }
}
", @"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        Func<object> x = () => M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_Assignment()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        Func<object> l = null;

        l = [|M|];
        l += [|M|];
        l -= [|M|];
        l ??= [|M|];
    }
}
", @"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        Func<object> l = null;

        l = () => M();
        l += () => M();
        l -= () => M();
        l ??= () => M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_ObjectInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    Func<object> P { get; set; }

    static object M() => null;

    void M2(Func<object> p)
    {
        var c = new C() { P = [|M|] };
    }
}
", @"
using System;

class C
{
    Func<object> P { get; set; }

    static object M() => null;

    void M2(Func<object> p)
    {
        var c = new C() { P = () => M() };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_PropertyInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    public Func<object> P { get; set; } = [|M|];

    static object M() => null;
}
", @"
using System;

class C
{
    public Func<object> P { get; set; } = () => M();

    static object M() => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_FieldInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    Func<object> F = [|M|];

    static object M() => null;
}
", @"
using System;

class C
{
    Func<object> F = () => M();

    static object M() => null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_ReturnStatement()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M() => null;

    Func<object> M2(Func<object> p)
    {
        return [|M|];
    }
}
", @"
using System;

class C
{
    static object M() => null;

    Func<object> M2(Func<object> p)
    {
        return () => M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_ArrowExpressionClause()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M() => null;

    Func<object> M2(Func<object> p) => [|M|];
}
", @"
using System;

class C
{
    static object M() => null;

    Func<object> M2(Func<object> p) => () => M();
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_ArrayInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        var x = new Func<object>[] { [|M|] };
    }
}
", @"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        var x = new Func<object>[] { () => M() };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_CollectionInitializer()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Collections.Generic;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        var x = new List<Func<object>>() { [|M|] };
    }
}
", @"
using System;
using System.Collections.Generic;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        var x = new List<Func<object>>() { () => M() };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_YieldReturnStatement()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;
using System.Collections.Generic;

class C
{
    static object M() => null;

    IEnumerable<Func<object>> M2(Func<object> p)
    {
        yield return [|M|];
    }
}
", @"
using System;
using System.Collections.Generic;

class C
{
    static object M() => null;

    IEnumerable<Func<object>> M2(Func<object> p)
    {
        yield return () => M();
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task Test_SwitchExpressionArm()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;

class C
{
    static object M() => null;

    Func<object> M(Func<object> p)
    {
        return """" switch
        {
            _ => [|M|],
        };
    }
}
", @"
using System;

class C
{
    static object M() => null;

    Func<object> M(Func<object> p)
    {
        return """" switch
        {
            _ => () => M(),
        };
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa)]
        public async Task TestNoDiagnostic_AnalyzerOptionDisabled()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
    static object M() => null;

    void M2(Func<object> p)
    {
        M2(M);
    }
}
", options: Options.DisableDiagnostic(AnalyzerOptionDiagnosticDescriptors.ConvertMethodGroupToAnonymousFunction));
        }
    }
}
