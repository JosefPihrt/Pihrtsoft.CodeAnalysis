// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1232OrderElementsInDocumentationCommentTests : AbstractCSharpDiagnosticVerifier<SingleLineDocumentationCommentTriviaAnalyzer, OrderElementsInDocumentationCommentCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.OrderElementsInDocumentationComment;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OrderElementsInDocumentationComment)]
        public async Task Test_Parameters()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary></summary>
    /// [|<param name=""c""></param>|]
    /// <param name=""b""></param>
    /// <param name=""a"" />
    void M(object a, object b, object c)
    {
    }
}
", @"
class C
{
    /// <summary></summary>
    /// <param name=""a"" />
    /// <param name=""b""></param>
    /// <param name=""c""></param>
    void M(object a, object b, object c)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OrderElementsInDocumentationComment)]
        public async Task Test_Parameters2()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary></summary>
    /// [|<param name=""b""></param>|]
    /// <param name=""a""></param>
    /// <param name=""c""></param>
    void M(object a, object b, object c)
    {
    }
}
", @"
class C
{
    /// <summary></summary>
    /// <param name=""a""></param>
    /// <param name=""b""></param>
    /// <param name=""c""></param>
    void M(object a, object b, object c)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OrderElementsInDocumentationComment)]
        public async Task Test_Parameters3()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary></summary>
    /// <param name=""a""></param>
    /// [|<param name=""c""></param>|]
    /// <param name=""b""></param>
    void M(object a, object b, object c)
    {
    }
}
", @"
class C
{
    /// <summary></summary>
    /// <param name=""a""></param>
    /// <param name=""b""></param>
    /// <param name=""c""></param>
    void M(object a, object b, object c)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OrderElementsInDocumentationComment)]
        public async Task Test_Parameters4()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary></summary>
    /// [|<param name=""c""></param>|]
    /// <param name=""b""></param>
    void M(object a, object b, object c)
    {
    }
}
", @"
class C
{
    /// <summary></summary>
    /// <param name=""b""></param>
    /// <param name=""c""></param>
    void M(object a, object b, object c)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OrderElementsInDocumentationComment)]
        public async Task Test_TypeParameters()
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    /// <summary></summary>
    /// [|<typeparam name=""T3""></param>|]
    /// <typeparam name=""T2""></param>
    /// <typeparam name=""T1"" />
    void M<T1, T2, T3>()
    {
    }
}
", @"
class C
{
    /// <summary></summary>
    /// <typeparam name=""T1"" />
    /// <typeparam name=""T2""></param>
    /// <typeparam name=""T3""></param>
    void M<T1, T2, T3>()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OrderElementsInDocumentationComment)]
        public async Task TestNoDiagnostic_Parameters()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    /// <summary></summary>
    /// <param name=""a""></param>
    /// <param name=""b""></param>
    /// <param name=""c""></param>
    void M(object a, object b, object c)
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.OrderElementsInDocumentationComment)]
        public async Task TestNoDiagnostic_TypeParameters()
        {
            await VerifyNoDiagnosticAsync(@"
class C
{
    /// <summary></summary>
    /// <typeparam name=""T1""></param>
    /// <typeparam name=""T2""></param>
    /// <typeparam name=""T3""></param>
    void M<T1, T2, T3>()
    {
    }
}
");
        }
    }
}
