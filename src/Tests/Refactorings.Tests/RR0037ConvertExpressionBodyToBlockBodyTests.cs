// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0037ConvertExpressionBodyToBlockBodyTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.ConvertExpressionBodyToBlockBody;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertExpressionBodyToBlockBody)]
        public async Task Test_MultipleMembers()
        {
            await VerifyRefactoringAsync(@"
class C
{
[|    public C() => M();

    ~C() => M();

    string M() => default;

    public string P => default;

    public string this[int index] => default;

    public static explicit operator C(string value) => default;

    public static explicit operator string(C value) => default;

    public static C operator !(C value) => default;|]
}", @"
class C
{
    public C()
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
    }
}", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertExpressionBodyToBlockBody)]
        public async Task Test_MultipleMembers_FirstAndLast()
        {
            await VerifyRefactoringAsync(@"
class C
{
[|    public C() => M();

    string M()
    {
        return default;
    }

    public string P => default;|]
}", @"
class C
{
    public C()
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
}", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.ConvertExpressionBodyToBlockBody)]
        public async Task Test_InitSetter()
        {
            await VerifyRefactoringAsync(@"
class C
{
    string _f;

    public string P
    {
        get => _f;

        init =>[||] _f = value;
    }
}
", @"
class C
{
    string _f;

    public string P
    {
        get => _f;

        init
        {
            _f = value;
        }
    }
}
", equivalenceKey: RefactoringId, options: Options.AddAllowedCompilerDiagnosticId("CS0518"));
        }
    }
}
