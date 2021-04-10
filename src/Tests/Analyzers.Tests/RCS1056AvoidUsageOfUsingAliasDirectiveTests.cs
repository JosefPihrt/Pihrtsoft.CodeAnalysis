// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1056AvoidUsageOfUsingAliasDirectiveTests : AbstractCSharpDiagnosticVerifier<AvoidUsageOfUsingAliasDirectiveAnalyzer, UsingDirectiveCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AvoidUsageOfUsingAliasDirective;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AvoidUsageOfUsingAliasDirective)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
[|using s = System;|]
[|using scg = System.Collections.Generic;|]
[|using ss = System.String;|]

class C
{
    void M()
    {
        ss s1 = ss.Empty;
        ss s2 = ss.Empty;

        scg.List<string> t1 = new scg.List<string>();
        scg.List<string> t2 = new scg.List<string>();

        string u1 = s.String.Empty;
        string u2 = s.String.Empty;
    }
}
", @"
class C
{
    void M()
    {
        string s1 = string.Empty;
        string s2 = string.Empty;

        System.Collections.Generic.List<string> t1 = new System.Collections.Generic.List<string>();
        System.Collections.Generic.List<string> t2 = new System.Collections.Generic.List<string>();

        string u1 = System.String.Empty;
        string u2 = System.String.Empty;
    }
}
");
        }
    }
}
