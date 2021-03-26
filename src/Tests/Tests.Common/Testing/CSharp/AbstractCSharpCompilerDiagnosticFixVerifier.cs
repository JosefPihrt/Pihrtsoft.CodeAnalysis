// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Testing.CSharp.Xunit;
using Roslynator.Testing.Text;

namespace Roslynator.Testing.CSharp
{
    public abstract class AbstractCSharpCompilerDiagnosticFixVerifier<TFixProvider> : XunitCompilerDiagnosticFixVerifier<TFixProvider>
        where TFixProvider : CodeFixProvider, new()
    {
        public abstract string DiagnosticId { get; }

        public override CSharpTestOptions Options => DefaultCSharpTestOptions.Value;

        public async Task VerifyFixAsync(
            string source,
            string sourceData,
            string expectedData,
            IEnumerable<(string source, string expectedSource)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var code = TestCode.Parse(source, sourceData, expectedData);

            Debug.Assert(code.Spans.Length == 0);

            ExpectedTestState expected = ExpectedTestState.Parse(code.ExpectedValue);

            var state = new CompilerDiagnosticFixTestState(
                DiagnosticId,
                code.Value,
                AdditionalFile.CreateRange(additionalFiles),
                equivalenceKey: equivalenceKey);

            await VerifyFixAsync(
                state,
                expected,
                options: options,
                cancellationToken: cancellationToken);
        }

        public async Task VerifyFixAsync(
            string source,
            string expectedSource,
            IEnumerable<(string source, string expectedSource)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var expected = ExpectedTestState.Parse(expectedSource);

            var state = new CompilerDiagnosticFixTestState(
                DiagnosticId,
                source,
                AdditionalFile.CreateRange(additionalFiles),
                equivalenceKey: equivalenceKey);

            await VerifyFixAsync(
                state,
                expected,
                options,
                cancellationToken);
        }

        public async Task VerifyNoFixAsync(
            string source,
            IEnumerable<string> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var state = new CompilerDiagnosticFixTestState(
                DiagnosticId,
                source,
                additionalFiles: AdditionalFile.CreateRange(additionalFiles),
                equivalenceKey: equivalenceKey);

            await VerifyNoFixAsync(
                state,
                options,
                cancellationToken);
        }
    }
}
