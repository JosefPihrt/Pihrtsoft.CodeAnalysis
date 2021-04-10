// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0039RemoveNewLineBeforeBaseListTests : AbstractCSharpDiagnosticVerifier<RemoveNewLineBeforeBaseListAnalyzer, SyntaxTriviaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.RemoveNewLineBeforeBaseList;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLineBeforeBaseList)]
        public async Task Test_Class()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C[||]
    : B
{
}

class B
{
}
", @"
class C : B
{
}

class B
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLineBeforeBaseList)]
        public async Task Test_Class_EmptyLine()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C[||]

    : B
{
}

class B
{
}
", @"
class C : B
{
}

class B
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLineBeforeBaseList)]
        public async Task Test_ClassWithTypeParameters()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C<T>[||]
    : B
{
}

class B
{
}
", @"
class C<T> : B
{
}

class B
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLineBeforeBaseList)]
        public async Task Test_Interface()
        {
            await VerifyDiagnosticAndFixAsync(@"
interface C[||]
    : B
{
}

interface B
{
}
", @"
interface C : B
{
}

interface B
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLineBeforeBaseList)]
        public async Task Test_Interface_WithTypeParameters()
        {
            await VerifyDiagnosticAndFixAsync(@"
interface C<T>[||]
    : B
{
}

interface B
{
}
", @"
interface C<T> : B
{
}

interface B
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLineBeforeBaseList)]
        public async Task Test_Struct()
        {
            await VerifyDiagnosticAndFixAsync(@"
struct C[||]
    : B
{
}

interface B
{
}
", @"
struct C : B
{
}

interface B
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLineBeforeBaseList)]
        public async Task Test_Struct_WithTypeParameters()
        {
            await VerifyDiagnosticAndFixAsync(@"
struct C<T>[||]
    : B
{
}

interface B
{
}
", @"
struct C<T> : B
{
}

interface B
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLineBeforeBaseList)]
        public async Task Test_Enum()
        {
            await VerifyDiagnosticAndFixAsync(@"
enum E[||]
    : int
{
}
", @"
enum E : int
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemoveNewLineBeforeBaseList)]
        public async Task TestNoDiagnostic_Comment()
        {
            await VerifyNoDiagnosticAsync(@"
class C //x
    : B
{
}

class B
{
}
");
        }
    }
}
