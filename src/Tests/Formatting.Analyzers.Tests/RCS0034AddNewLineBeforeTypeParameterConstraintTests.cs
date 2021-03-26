// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0034AddNewLineBeforeTypeParameterConstraintTests : AbstractCSharpDiagnosticVerifier<AddNewLineBeforeTypeParameterConstraintAnalyzer, TypeParameterConstraintClauseSyntaxCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddNewLineBeforeTypeParameterConstraint;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeTypeParameterConstraint)]
        public async Task Test_Class()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    class C<T1, T2> [||]where T1 : struct [||]where T2 : struct
    {
    }
}
", @"
namespace N
{
    class C<T1, T2>
        where T1 : struct
        where T2 : struct
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeTypeParameterConstraint)]
        public async Task Test_Struct()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    struct S<T1, T2> [||]where T1 : struct [||]where T2 : struct
    {
    }
}
", @"
namespace N
{
    struct S<T1, T2>
        where T1 : struct
        where T2 : struct
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeTypeParameterConstraint)]
        public async Task Test_Interface()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    interface I<T1, T2> [||]where T1 : struct [||]where T2 : struct
    {
    }
}
", @"
namespace N
{
    interface I<T1, T2>
        where T1 : struct
        where T2 : struct
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeTypeParameterConstraint)]
        public async Task Test_Delegate()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    delegate void DelegateName<T1, T2>() [||]where T1 : struct [||]where T2 : struct;
}
", @"
namespace N
{
    delegate void DelegateName<T1, T2>()
        where T1 : struct
        where T2 : struct;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeTypeParameterConstraint)]
        public async Task Test_Method()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    class C
    {
        void M<T1, T2>() [||]where T1 : struct [||]where T2 : struct
        {
        }
    }
}
", @"
namespace N
{
    class C
    {
        void M<T1, T2>()
            where T1 : struct
            where T2 : struct
        {
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeTypeParameterConstraint)]
        public async Task Test_LocalFunction()
        {
            await VerifyDiagnosticAndFixAsync(@"
namespace N
{
    class C
    {
        void M()
        {
            void M<T1, T2>() [||]where T1 : struct [||]where T2 : struct
            {
            }
        }
    }
}
", @"
namespace N
{
    class C
    {
        void M()
        {
            void M<T1, T2>()
                where T1 : struct
                where T2 : struct
            {
            }
        }
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeTypeParameterConstraint)]
        public async Task TestNoDiagnostic_SingleConstraint()
        {
            await VerifyNoDiagnosticAsync(@"
class C<T> where T : struct
{
}
");
        }
    }
}
