// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1016ConvertExpressionBodyToBlockBodyTests : AbstractCSharpDiagnosticVerifier<ConvertBlockBodyToExpressionBodyOrViceVersaAnalyzer, ConvertBlockBodyToExpressionBodyOrViceVersaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.ConvertBlockBodyToExpressionBodyOrViceVersa;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_Constructor()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    public C() [|=> M()|];

    void M() { }
}
", @"
class C
{
    public C()
    {
        M();
    }

    void M() { }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_Destructor()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    ~C() [|=> M()|];

    void M() { }
}
", @"
class C
{
    ~C()
    {
        M();
    }

    void M() { }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_Method()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M() [|=> null|];
}
", @"
class C
{
    string M()
    {
        return null;
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_Method_MultilineExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M(object x, object y) [|=> M(
        x,
        y)|];
}
", @"
class C
{
    string M(object x, object y)
    {
        return M(
            x,
            y);
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_Method_MultilineExpression2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M(object x, object y, object z) [|=> M(
        x,
            y,
                z)|];
}
", @"
class C
{
    string M(object x, object y, object z)
    {
        return M(
            x,
                y,
                    z);
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_VoidMethod()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M() [|=> M()|];
}
", @"
class C
{
    void M()
    {
        M();
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_LocalFunction()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string LF() [|=> null|];
    }
}
", @"
class C
{
    void M()
    {
        string LF()
        {
            return null;
        }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_LocalFunction_MultilineExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M(object x, object y)
    {
        return null;

        string LF() [|=> M(
            x,
            y)|];
    }
}
", @"
class C
{
    string M(object x, object y)
    {
        return null;

        string LF()
        {
            return M(
                x,
                y);
        }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_VoidLocalFunction()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        void LF() [|=> M()|];
    }
}
", @"
class C
{
    void M()
    {
        void LF()
        {
            M();
        }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_PropertyWithGetter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string P [|=> null|];
}
", @"
class C
{
    string P
    {
        get { return null; }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_PropertyWithGetter_MultilineExpression()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string P [|=> M(
        null,
        null)|];

    string M(string x, string y) [|=> null|];
}
", @"
class C
{
    string P
    {
        get
        {
            return M(
                null,
                null);
        }
    }

    string M(string x, string y)
    {
        return null;
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_PropertyWithGetterAndSetter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string _f;

    public string P
    {
        get [|=> M(
            null,
            null)|];

        set [|=> _f = value|];
    }

    string M(string x, string y) [|=> null|];
}
", @"
class C
{
    string _f;

    public string P
    {
        get
        {
            return M(
                null,
                null);
        }

        set
        {
            _f = value;
        }
    }

    string M(string x, string y)
    {
        return null;
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_PropertyWithGetterAndInitSetter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string _f;

    public string P
    {
        get [|=> M(
            null,
            null)|];

        init [|=> _f = value|];
    }

    string M(string x, string y) [|=> null|];
}
", @"
class C
{
    string _f;

    public string P
    {
        get
        {
            return M(
                null,
                null);
        }

        init
        {
            _f = value;
        }
    }

    string M(string x, string y)
    {
        return null;
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody).AddAllowedCompilerDiagnosticId("CS0518"));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_IndexerWithGetter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string this[int index] [|=> null|];
}
", @"
class C
{
    string this[int index]
    {
        get { return null; }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_IndexerWithGetterAndSetter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string _f;

    string this[int index]
    {
        get [|=> _f|];
        set [|=> _f = value|];
    }
}
", @"
class C
{
    string _f;

    string this[int index]
    {
        get
        {
            return _f;
        }

        set
        {
            _f = value;
        }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_Operator()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    public static C operator !(C value) [|=> value|];
}
", @"
class C
{
    public static C operator !(C value)
    {
        return value;
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_ConversionOperator()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    public static explicit operator C(string value) [|=> new C()|];
}
", @"
class C
{
    public static explicit operator C(string value)
    {
        return new C();
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_Constructor_Throw()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    public C() [|=> throw new System.NotImplementedException()|];

    void M() { }
}
", @"
class C
{
    public C()
    {
        throw new System.NotImplementedException();
    }

    void M() { }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_Destructor_Throw()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    ~C() [|=> throw new System.NotImplementedException()|];
}
", @"
class C
{
    ~C()
    {
        throw new System.NotImplementedException();
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_Method_Throw()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string M() [|=> throw new System.NotImplementedException()|];
}
", @"
class C
{
    string M()
    {
        throw new System.NotImplementedException();
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_VoidMethod_Throw()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M() [|=> throw new System.NotImplementedException()|];
}
", @"
class C
{
    void M()
    {
        throw new System.NotImplementedException();
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_LocalFunction_Throw()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string LF() [|=> throw new System.NotImplementedException()|];
    }
}
", @"
class C
{
    void M()
    {
        string LF()
        {
            throw new System.NotImplementedException();
        }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_VoidLocalFunction_Throw()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        void LF() [|=> throw new System.NotImplementedException()|];
    }
}
", @"
class C
{
    void M()
    {
        void LF()
        {
            throw new System.NotImplementedException();
        }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_PropertyWithGetter_Throw()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string P [|=> throw new System.NotImplementedException()|];
}
", @"
class C
{
    string P
    {
        get { throw new System.NotImplementedException(); }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_PropertyWithGetterAndSetter_Throw()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string _f;

    public string P
    {
        get [|=> throw new System.NotImplementedException()|];
        set [|=> throw new System.NotImplementedException()|];
    }
}
", @"
class C
{
    string _f;

    public string P
    {
        get
        {
            throw new System.NotImplementedException();
        }

        set
        {
            throw new System.NotImplementedException();
        }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_IndexerWithGetter_Throw()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string this[int index] [|=> throw new System.NotImplementedException()|];
}
", @"
class C
{
    string this[int index]
    {
        get { throw new System.NotImplementedException(); }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_IndexerWithGetterAndSetter_Throw()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string _f;

    string this[int index]
    {
        get [|=> throw new System.NotImplementedException()|];
        set [|=> throw new System.NotImplementedException()|];
    }
}
", @"
class C
{
    string _f;

    string this[int index]
    {
        get
        {
            throw new System.NotImplementedException();
        }

        set
        {
            throw new System.NotImplementedException();
        }
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_Operator_Throw()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    public static C operator !(C value) [|=> throw new System.NotImplementedException()|];
}
", @"
class C
{
    public static C operator !(C value)
    {
        throw new System.NotImplementedException();
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task Test_ConversionOperator_Throw()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    public static explicit operator C(string value) [|=> throw new System.NotImplementedException()|];
}
", @"
class C
{
    public static explicit operator C(string value)
    {
        throw new System.NotImplementedException();
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.ConvertBlockBodyToExpressionBodyOrViceVersa)]
        public async Task TestNoDiagnostic_ExpressionBody()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    public C()
    {
        VM();
    }

    ~C()
    {
        VM();
    }

    string M()
    {
        return null;
    }

    void VM()
    {
        VM();
    }

    void M2()
    {
        string LF()
        {
            return null;
        }
    }

    void M3()
    {
        void LF()
        {
            VM();
        }
    }

    string P
    {
        get { return null; }
    }

    public string P2
    {
        get
        {
            return _f;
        }

        set
        {
            _f = value;
        }
    }

    string this[int index]
    {
        get { return null; }
    }

    string this[int index, int index2]
    {
        get
        {
            return _f;
        }

        set
        {
            _f = value;
        }
    }

    public static C operator !(C value)
    {
        return value;
    }

    public static explicit operator C(string value)
    {
        return new C();
    }

    string _f;
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.ConvertExpressionBodyToBlockBody));
        }
    }
}
