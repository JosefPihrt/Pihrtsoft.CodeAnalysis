// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0052AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersaTests : AbstractCSharpDiagnosticVerifier<AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersaAnalyzer, SyntaxTokenCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task Test_LocalDeclaration_BeforeInsteadOfAfter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s [|=|]
            null;
    }
}
", @"
class C
{
    void M()
    {
        string s
            = null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task Test_LocalDeclaration_AfterInsteadOfBefore()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s
            [|=|] null;
    }
}
", @"
class C
{
    void M()
    {
        string s =
            null;
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterEqualsSignInsteadOfBeforeIt));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task Test_Assignment_BeforeInsteadOfAfter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = null;

        s [|=|]
            null;
    }
}
", @"
class C
{
    void M()
    {
        string s = null;

        s
            = null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task Test_Assignment_AfterInsteadOfBefore()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        string s = null;

        s
            [|=|] null;
    }
}
", @"
class C
{
    void M()
    {
        string s = null;

        s =
            null;
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterEqualsSignInsteadOfBeforeIt));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task Test_PropertyInitializer_BeforeInsteadOfAfter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string P { get; } [|=|]
        null;
}
", @"
class C
{
    string P { get; }
        = null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task Test_PropertyInitializer_AfterInsteadOfBefore()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string P { get; }
        [|=|] null;
}
", @"
class C
{
    string P { get; } =
        null;
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterEqualsSignInsteadOfBeforeIt));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task Test_FieldValue_BeforeInsteadOfAfter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string F [|=|]
        null;
}
", @"
class C
{
    string F
        = null;
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task Test_FieldValue_AfterInsteadOfBefore()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    string F
        [|=|] null;
}
", @"
class C
{
    string F =
        null;
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterEqualsSignInsteadOfBeforeIt));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task Test_Parameter_BeforeInsteadOfAfter()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string p [|=|]
        null)
    {
    }
}
", @"
class C
{
    void M(string p
        = null)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task Test_Parameter_AfterInsteadOfBefore()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M(string p
        [|=|] null)
    {
    }
}
", @"
class C
{
    void M(string p =
        null)
    {
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterEqualsSignInsteadOfBeforeIt));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task Test_AnonymousType_BeforeInsteadOfAfter()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        List<string> list = null;
        var x = list.Select(f => new { X [|=|]
            """" });
    }
}
", @"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        List<string> list = null;
        var x = list.Select(f => new { X
            = """" });
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task Test_AnonymousType_AfterInsteadOfBefore()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        List<string> list = null;
        var x = list.Select(f => new { X
            [|=|] """" });
    }
}
", @"
using System.Linq;
using System.Collections.Generic;

class C
{
    void M()
    {
        List<string> list = null;
        var x = list.Select(f => new { X =
            """" });
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterEqualsSignInsteadOfBeforeIt));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task Test_AttributeArgument_BeforeInsteadOfAfter()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Diagnostics;

[DebuggerDisplay("""", Name [|=|]
    ""x"")]
class C
{
}
", @"
using System.Diagnostics;

[DebuggerDisplay("""", Name
    = ""x"")]
class C
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task Test_AttributeArgument_AfterInsteadOfBefore()
        {
            await VerifyDiagnosticAndFixAsync(@"
using System.Diagnostics;

[DebuggerDisplay("""", Name
    [|=|] ""x"")]
class C
{
}
", @"
using System.Diagnostics;

[DebuggerDisplay("""", Name =
    ""x"")]
class C
{
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterEqualsSignInsteadOfBeforeIt));
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task TestNoDiagnostic_BeforeInsteadOfAfter_Comment()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        string s // x
            = null;
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddNewLineBeforeEqualsSignInsteadOfAfterItOrViceVersa)]
        public async Task TestNoDiagnostic_AfterInsteadOfBefore_Comment()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    void M()
    {
        string s // x
            = null;
    }
}
", options: Options.EnableDiagnostic(AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterEqualsSignInsteadOfBeforeIt));
        }
    }
}
