// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.Formatting.CSharp.Tests
{
    public class RCS0006AddEmptyLineBeforeUsingDirectiveListTests : AbstractCSharpDiagnosticVerifier<AddEmptyLineBeforeUsingDirectiveListAnalyzer, AddEmptyLineBeforeAndAfterUsingDirectiveListCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticRules.AddEmptyLineBeforeUsingDirectiveList;

        public override CSharpTestOptions Options
        {
            get { return base.Options.AddAllowedCompilerDiagnosticId("CS0430"); }
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeUsingDirectiveList)]
        public async Task Test_Comment_Before()
        {
            await VerifyDiagnosticAndFixAsync(@"// x
[||]using System;
using System.Linq;

namespace N
{
}
", @"// x

using System;
using System.Linq;

namespace N
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeUsingDirectiveList)]
        public async Task Test_CommentAndExternAlias_Before()
        {
            await VerifyDiagnosticAndFixAsync(@"
extern alias x;
[||]using System;
using System.Linq;

namespace N
{
}
", @"
extern alias x;

using System;
using System.Linq;

namespace N
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeUsingDirectiveList)]
        public async Task Test_Comment_ExternAliasAndComment_Before()
        {
            await VerifyDiagnosticAndFixAsync(@"
extern alias x; // x
[||]using System;
using System.Linq;

namespace N
{
}
", @"
extern alias x; // x

using System;
using System.Linq;

namespace N
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeUsingDirectiveList)]
        public async Task Test_ExternAliasAndRegionDirective_Before()
        {
            await VerifyNoDiagnosticAsync(@"
extern alias x;
#region
using System;
using System.Linq;
#endregion

namespace N
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeUsingDirectiveList)]
        public async Task TestNoDiagnostic_CommentAndRegionDirective_Before()
        {
            await VerifyNoDiagnosticAsync(@"// x
#region
using System;
using System.Linq;
#endregion

namespace N
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddEmptyLineBeforeUsingDirectiveList)]
        public async Task TestNoDiagnostic_CommentAndPragmaDirective_Before()
        {
            await VerifyNoDiagnosticAsync(@"
#pragma warning disable x
using System;
using System.Linq;

namespace N
{
}
");
        }
    }
}
