// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1142AddTypeParamElementToDocumentationCommentTests : AbstractCSharpDiagnosticVerifier<SingleLineDocumentationCommentTriviaAnalyzer, SingleLineDocumentationCommentTriviaCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.AddTypeParamElementToDocumentationComment;

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddTypeParamElementToDocumentationComment)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
///[| <summary>
/// 
/// </summary>
/// <typeparam name=""T2""></typeparam>
|]class C<T1, T2, T3>
{
}
", @"
/// <summary>
/// 
/// </summary>
/// <typeparam name=""T1""></typeparam>
/// <typeparam name=""T2""></typeparam>
/// <typeparam name=""T3""></typeparam>
class C<T1, T2, T3>
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.AddTypeParamElementToDocumentationComment)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>Represents a wrapper around the specified <see cref=""ICollection{TItem}""/>.</summary>
/// <typeparam name=""TItem"">The type of the items in the collection.</typeparam>
public class WrapperCollection<TItem>
{
    /// <summary>The wrapped <see cref=""ICollection{TItem}""/>.</summary>
    private readonly ICollection<TItem> collection;

    /// <summary>Initializes a new instance of the <see cref=""WrapperCollection{TItem}""/> class.</summary>
    /// <param name=""collection""><see cref=""ICollection{TItem}""/> to wrap.</param>
    public WrapperCollection(ICollection<TItem> collection)
    {
        this.collection = collection ?? throw new ArgumentNullException(nameof(collection));
    }
}
");
        }
    }
}
