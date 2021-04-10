// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0050AddEmptyLineBeforeTopDeclarationTests : AbstractCSharpDiagnosticVerifier<AddEmptyLineBeforeTopDeclarationAnalyzer, SyntaxTriviaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AddEmptyLineBeforeTopDeclaration;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeTopDeclaration)]
        public async Task Test_ExternAlias()
        {
            await VerifyDiagnosticAndFixAsync(@"
extern alias x;[||]
class C
{
}
", @"
extern alias x;

class C
{
}
", options: Options.AddAllowedCompilerDiagnosticId("CS0430"));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeTopDeclaration)]
        public async Task Test_Using()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;[||]
class C
{
}
", @"
using System;

class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeTopDeclaration)]
        public async Task Test_Attribute()
        {
            await VerifyDiagnosticAndFixAsync(@"
[assembly: AssemblyAttribute][||]
class C
{
}

class AssemblyAttribute : System.Attribute
{
}
", @"
[assembly: AssemblyAttribute]

class C
{
}

class AssemblyAttribute : System.Attribute
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeTopDeclaration)]
        public async Task Test_If()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;[||]
class C
{
}
", @"
using System;

class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeTopDeclaration)]
        public async Task Test_Namespace()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;[||]
namespace N
{
}
", @"
using System;

namespace N
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeTopDeclaration)]
        public async Task Test_Struct()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;[||]
struct C
{
}
", @"
using System;

struct C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeTopDeclaration)]
        public async Task Test_Interface()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System;[||]
interface IC
{
}
", @"
using System;

interface IC
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeTopDeclaration)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
using System;

class C
{
}
");
        }
    }
}
