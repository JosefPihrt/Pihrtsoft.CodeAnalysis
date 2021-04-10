// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.Metadata;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CodeGeneration.CSharp
{
    public static class AnalyzerTestGenerator
    {
        public static CompilationUnitSyntax Generate(AnalyzerMetadata analyzer, string className)
        {
            string s = _sourceTemplate
                .Replace("$ClassName$", className)
                .Replace("$Id$", analyzer.Id)
                .Replace("$Identifier$", analyzer.Identifier);

            return ParseCompilationUnit(s);
        }

        private const string _sourceTemplate = @"
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Tests;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    //TODO: Add tests for $Id$
    public class $ClassName$ : AbstractCSharpFixVerifier
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.$Identifier$;

        public override DiagnosticAnalyzer Analyzer { get; } = new $Identifier$Analyzer();

        public override CodeFixProvider FixProvider { get; } = new $Identifier$CodeFixProvider();

        //[Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.$Identifier$)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@""
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class C
{
    void M()
    {
    }
}
"", @""
"");
        }

        //[Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.$Identifier$)]
        //[InlineData("""", """")]
        public async Task Test2(string source, string expected)
        {
            await VerifyDiagnosticAndFixAsync(@""
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class C
{
    void M()
    {
    }
}
"", source, expected);
        }

        //[Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.$Identifier$)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@""
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class C
{
    void M()
    {
    }
}
"");
        }

        //[Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.$Identifier$)]
        //[InlineData("""")]
        public async Task TestNoDiagnostic2(string source)
        {
            await VerifyNoDiagnosticAsync(@""
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class C
{
    void M()
    {
    }
}
"", source);
        }
    }
}
";
    }
}
