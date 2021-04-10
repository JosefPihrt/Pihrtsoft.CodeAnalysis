// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0045UseLinefeedAsNewLineTests : AbstractCSharpDiagnosticVerifier<UseLinefeedAsNewLineAnalyzer, NewLineCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.UseLinefeedAsNewLine;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseLinefeedAsNewLine)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync("\n"
                + "class C[|\r\n|]"
                + "{[|\r\n|]"
                + "    /// <summary>[|\r\n|]"
                + "    /// [|\r\n|]"
                + "    /// </summary>[|\r\n|]"
                + "    void M()[|\r\n|]"
                + "    {[|\r\n|]"
                + "    }[|\r\n|]"
                + "}\n",
                "\n"
                    + "class C\n"
                    + "{\n"
                    + "    /// <summary>\n"
                    + "    /// \n"
                    + "    /// </summary>\n"
                    + "    void M()\n"
                    + "    {\n"
                    + "    }\n"
                    + "}\n");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseLinefeedAsNewLine)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync("\n"
                + "class C\n"
                + "{\n"
                + "    void M()\n"
                + "    {\n"
                + "    }\n"
                + "}\n");
        }
    }
}
