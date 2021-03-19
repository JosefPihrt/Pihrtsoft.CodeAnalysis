// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0169ConvertBlockBodyToExpressionBodyTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ConvertBlockBodyToExpressionBody;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_Constructor()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public C()
    [|{
[||]       M();
    }|]

    void M() { }
}
", @"
class C
{
    public C() => M();

    void M() { }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_Destructor()
        {
            await VerifyRefactoringAsync(@"
class C
{
    ~C()
    [|{
[||]        M();
    }|]

    void M() { }
}
", @"
class C
{
    ~C() => M();

    void M() { }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_Method()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string M()
    [|{
[||]        return null;
    }|]
}
", @"
class C
{
    string M() => null;
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_VoidMethod()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    [|{
[||]        M();
    }|]
}
", @"
class C
{
    void M() => M();
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_LocalFunction()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        string LF()
        [|{
[||]            return null;
        }|]
    }
}
", @"
class C
{
    void M()
    {
        string LF() => null;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_VoidLocalFunction()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        void LF()
        [|{
[||]            M();
        }|]
    }
}
", @"
class C
{
    void M()
    {
        void LF() => M();
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_PropertyWithGetter()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string P
    [|{
[||]        get { return null; }
    }|]
}
", @"
class C
{
    string P => null;
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_PropertyWithGetterAndSetter_Getter()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string _f;

    public string P
    {
        [|get [|{ [||]return _f; }|]|]
        set { _f = value; }
    }
}
", @"
class C
{
    string _f;

    public string P
    {
        get => _f;
        set { _f = value; }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_PropertyWithGetterAndSetter_Setter()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string _f;

    public string P
    {
        get { return _f; }
        [|set [|{ [||]_f = value; }|]|]
    }
}
", @"
class C
{
    string _f;

    public string P
    {
        get { return _f; }
        set => _f = value;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_PropertyWithGetterAndInitSetter_InitSetter()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string _f;

    public string P
    {
        get { return _f; }
        [|init [|{ [||]_f = value; }|]|]
    }
}
", @"
class C
{
    string _f;

    public string P
    {
        get { return _f; }
        init => _f = value;
    }
}
", equivalenceKey: RefactoringId, options: Options.AddAllowedCompilerDiagnosticId("CS0518"));
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_IndexerWithGetter()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string this[int index]
    [|{
[||]        get { return null; }
    }|]
}
", @"
class C
{
    string this[int index] => null;
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_IndexerWithGetterAndSetter_Getter()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string _f;

    string this[int index]
    {
        [|get [|{ [||]return _f; }|]|]
        set { _f = value; }
    }
}
", @"
class C
{
    string _f;

    string this[int index]
    {
        get => _f;
        set { _f = value; }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_IndexerWithGetterAndSetter_Setter()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string _f;

    string this[int index]
    {
        get { return _f; }
        [|set [|{ [||]_f = value; }|]|]
    }
}
", @"
class C
{
    string _f;

    string this[int index]
    {
        get { return _f; }
        set => _f = value;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_Operator()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public static C operator !(C value)
    [|{
[||]        return value;
    }|]
}
", @"
class C
{
    public static C operator !(C value) => value;
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_ConversionOperator()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public static explicit operator C(string value)
    [|{
[||]        return new C();
    }|]
}
", @"
class C
{
    public static explicit operator C(string value) => new C();
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_Constructor_Throw()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public C()
    [|{
        throw new System.NotImplementedException();
    }|]

    void M() { }
}
", @"
class C
{
    public C() => throw new System.NotImplementedException();

    void M() { }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_Destructor_Throw()
        {
            await VerifyRefactoringAsync(@"
class C
{
    ~C()
    [|{
        throw new System.NotImplementedException();
    }|]
}
", @"
class C
{
    ~C() => throw new System.NotImplementedException();
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_Method_Throw()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string M()
    [|{
        throw new System.NotImplementedException();
    }|]
}
", @"
class C
{
    string M() => throw new System.NotImplementedException();
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_VoidMethod_Throw()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    [|{
        throw new System.NotImplementedException();
    }|]
}
", @"
class C
{
    void M() => throw new System.NotImplementedException();
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_LocalFunction_Throw()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        string LF()
        [|{
            throw new System.NotImplementedException();
        }|]
    }
}
", @"
class C
{
    void M()
    {
        string LF() => throw new System.NotImplementedException();
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_VoidLocalFunction_Throw()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        void LF()
        [|{
            throw new System.NotImplementedException();
        }|]
    }
}
", @"
class C
{
    void M()
    {
        void LF() => throw new System.NotImplementedException();
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_PropertyWithGetter_Throw()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string P
    [|{
        get { throw new System.NotImplementedException(); }
    }|]
}
", @"
class C
{
    string P => throw new System.NotImplementedException();
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_PropertyWithGetterAndSetter_Getter_Throw()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string _f;

    public string P
    {
        get [|{ throw new System.NotImplementedException(); }|]
        set { throw new System.NotImplementedException(); }
    }
}
", @"
class C
{
    string _f;

    public string P
    {
        get => throw new System.NotImplementedException();
        set { throw new System.NotImplementedException(); }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_PropertyWithGetterAndSetter_Setter_Throw()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string _f;

    public string P
    {
        get { throw new System.NotImplementedException(); }
        set [|{ throw new System.NotImplementedException(); }|]
    }
}
", @"
class C
{
    string _f;

    public string P
    {
        get { throw new System.NotImplementedException(); }
        set => throw new System.NotImplementedException();
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_IndexerWithGetter_Throw()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string this[int index]
    [|{
        get { throw new System.NotImplementedException(); }
    }|]
}
", @"
class C
{
    string this[int index] => throw new System.NotImplementedException();
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_IndexerWithGetterAndSetter_Getter_Throw()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string _f;

    string this[int index]
    {
        get [|{ throw new System.NotImplementedException(); }|]
        set { throw new System.NotImplementedException(); }
    }
}
", @"
class C
{
    string _f;

    string this[int index]
    {
        get => throw new System.NotImplementedException();
        set { throw new System.NotImplementedException(); }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_IndexerWithGetterAndSetter_Setter_Throw()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string _f;

    string this[int index]
    {
        get { throw new System.NotImplementedException(); }
        set [|{ throw new System.NotImplementedException(); }|]
    }
}
", @"
class C
{
    string _f;

    string this[int index]
    {
        get { throw new System.NotImplementedException(); }
        set => throw new System.NotImplementedException();
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_Operator_Throw()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public static C operator !(C value)
    [|{
        throw new System.NotImplementedException();
    }|]
}
", @"
class C
{
    public static C operator !(C value) => throw new System.NotImplementedException();
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_ConversionOperator_Throw()
        {
            await VerifyRefactoringAsync(@"
class C
{
    public static explicit operator C(string value)
    [|{
        throw new System.NotImplementedException();
    }|]
}
", @"
class C
{
    public static explicit operator C(string value) => throw new System.NotImplementedException();
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_MultipleMembers()
        {
            await VerifyRefactoringAsync(@"
class C
{
[|    public C()
    {
        M();
    }

    ~C()
    {
        M();
    }

    string M()
    {
        return default;
    }

    public string P
    {
        get { return default; }
    }

    public string this[int index]
    {
        get { return default; }
    }

    public static explicit operator C(string value)
    {
        return default;
    }

    public static explicit operator string(C value)
    {
        return default;
    }

    public static C operator !(C value)
    {
        return default;
    }|]
}
", @"
class C
{
    public C() => M();

    ~C() => M();

    string M() => default;

    public string P => default;

    public string this[int index] => default;

    public static explicit operator C(string value) => default;

    public static explicit operator string(C value) => default;

    public static C operator !(C value) => default;
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task Test_MultipleMembers_FirstAndLast()
        {
            await VerifyRefactoringAsync(@"
class C
{
[|    public C()
    {
        M();
    }

    string M() => default;

    public string P
    {
        get { return default; }
    }|]
}
", @"
class C
{
    public C() => M();

    string M() => default;

    public string P => default;
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task TestNoRefactoring_MultipleMembers()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
[|    string M()
    {
        return default;
    }

    string M2() => default;|]
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task TestNoDiagnostic_MethodWithMultipleStatements()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    string M()
    {
[||]        M();
        return null;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task TestNoDiagnostic_MethodWithLocalFunction()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M()
    {
[||]        void LF() { }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task TestNoDiagnostic_VoidMethodWithNoStatements()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M()
    {[||]
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task TestNoDiagnostic_VoidMethodWithMultipleStatements()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    void M()
    {[||]
        M();
        M();
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task TestNoDiagnostic_PropertyWithMultipleStatement()
        {
            await VerifyNoRefactoringAsync(@"
class C
{
    string P
    {
        get
        {
[||]            M();
            return null;
        }
    }

    string M() => null;
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task TestNoDiagnostic_AccessorWithAttribute()
        {
            await VerifyNoRefactoringAsync(@"
using System.Diagnostics;

class C
{
    string P
    {
        [DebuggerStepThrough]
        get
        {
[||]            return null;
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertBlockBodyToExpressionBody)]
        public async Task TestNoDiagnostic_IndexerWithMultipleStatements()
        {
            await VerifyNoRefactoringAsync(@"
using System.Diagnostics;

class C
{
    string this[int index]
    {
        get
        {
[||]            M();
            return null;
        }
    }

    string M() => null;
}
", equivalenceKey: RefactoringId);
        }
    }
}
