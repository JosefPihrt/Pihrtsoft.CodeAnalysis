// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1050AddArgumentListToObjectCreationOrViceVersaTests : AbstractCSharpDiagnosticVerifier<AddArgumentListToObjectCreationOrViceVersaAnalyzer, AddArgumentListToObjectCreationOrViceVersaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AddArgumentListToObjectCreationOrViceVersa;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddArgumentListToObjectCreationOrViceVersa)]
        public async Task Test_AddArgumentList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

public class C
{
    List<string> items = new List<string>[||] { ""a"", ""b"", ""c"" };
}
", @"
using System.Collections.Generic;

public class C
{
    List<string> items = new List<string>() { ""a"", ""b"", ""c"" };
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddArgumentListToObjectCreationOrViceVersa)]
        public async Task Test_RemoveArgumentList()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Collections.Generic;

public class C
{
    List<string> items = new List<string>[|()|] { ""a"", ""b"", ""c"" };
}
", @"
using System.Collections.Generic;

public class C
{
    List<string> items = new List<string> { ""a"", ""b"", ""c"" };
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.RemoveArgumentListFromObjectCreation));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddArgumentListToObjectCreationOrViceVersa)]
        public async Task TestNoDiagnostic_AddArgumentList()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;

public class C
{
    List<string> items = new List<string>() { ""a"", ""b"", ""c"" };
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddArgumentListToObjectCreationOrViceVersa)]
        public async Task TestNoDiagnostic_RemoveArgumentList()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;

public class C
{
    List<string> items = new List<string>() { ""a"", ""b"", ""c"" };
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddArgumentListToObjectCreationOrViceVersa)]
        public async Task TestNoDiagnostic_RemoveArgumentList_NoInitializer()
        {
            await VerifyNoDiagnosticAsync(@"
using System.Collections.Generic;

public class C
{
    List<string> items = new List<string>();
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticRules.RemoveArgumentListFromObjectCreation));
        }
    }
}