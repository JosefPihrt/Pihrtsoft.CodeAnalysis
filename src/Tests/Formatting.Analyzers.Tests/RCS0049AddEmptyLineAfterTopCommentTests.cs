// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0049AddEmptyLineAfterTopCommentTests : AbstractCSharpDiagnosticVerifier<AddEmptyLineAfterTopCommentAnalyzer, SyntaxTriviaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddEmptyLineAfterTopComment;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterTopComment)]
        public async Task Test_Class()
        {
            await VerifyDiagnosticAndFixAsync(@"// x[||]
class C
{
}
", @"// x

class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterTopComment)]
        public async Task Test_ExternAlias()
        {
            await VerifyDiagnosticAndFixAsync(@"// x[||]
extern alias x;

class C
{
}
", @"// x

extern alias x;

class C
{
}
", options: Options.AddAllowedCompilerDiagnosticIds(new[] { "CS0430", "CS8020" }));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterTopComment)]
        public async Task Test_Using()
        {
            await VerifyDiagnosticAndFixAsync(@"// x[||]
using System;

class C
{
}
", @"// x

using System;

class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterTopComment)]
        public async Task Test_AttributeList()
        {
            await VerifyDiagnosticAndFixAsync(@"// x[||]
[assembly: AssemblyAttribute]

class C
{
}

class AssemblyAttribute : System.Attribute
{
}
", @"// x

[assembly: AssemblyAttribute]

class C
{
}

class AssemblyAttribute : System.Attribute
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineAfterTopComment)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"// x

class C
{
}
");
        }
    }
}
