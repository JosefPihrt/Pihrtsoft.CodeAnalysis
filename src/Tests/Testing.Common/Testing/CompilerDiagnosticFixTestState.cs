﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Testing
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class CompilerDiagnosticFixTestState : TestState
    {
        public CompilerDiagnosticFixTestState(
            string diagnosticId,
            string source,
            string expectedSource = null,
            IEnumerable<AdditionalFile> additionalFiles = null,
            IEnumerable<KeyValuePair<string, ImmutableArray<TextSpan>>> expectedSpans = null,
            string codeActionTitle = null,
            string equivalenceKey = null) : base(source, expectedSource, additionalFiles, expectedSpans, codeActionTitle, equivalenceKey)
        {
            DiagnosticId = diagnosticId ?? throw new ArgumentNullException(nameof(diagnosticId));
        }

        private CompilerDiagnosticFixTestState(CompilerDiagnosticFixTestState other)
            : this(
                diagnosticId: other.DiagnosticId,
                source: other.Source,
                expectedSource: other.ExpectedSource,
                additionalFiles: other.AdditionalFiles,
                expectedSpans: other.ExpectedSpans,
                codeActionTitle: other.CodeActionTitle,
                equivalenceKey: other.EquivalenceKey)
        {
        }

        public string DiagnosticId { get; private set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{DiagnosticId}  {Source}";

        public CompilerDiagnosticFixTestState Update(
            string diagnosticId,
            string source,
            string expectedSource,
            IEnumerable<AdditionalFile> additionalFiles,
            IEnumerable<KeyValuePair<string, ImmutableArray<TextSpan>>> expectedSpans,
            string codeActionTitle,
            string equivalenceKey)
        {
            return new CompilerDiagnosticFixTestState(
                diagnosticId: diagnosticId,
                source: source,
                expectedSource: expectedSource,
                additionalFiles: additionalFiles,
                expectedSpans: expectedSpans,
                codeActionTitle: codeActionTitle,
                equivalenceKey: equivalenceKey);
        }

        protected override TestState CommonWithSource(string source)
        {
            return WithSource(source);
        }

        protected override TestState CommonWithExpectedSource(string expectedSource)
        {
            return WithExpectedSource(expectedSource);
        }

        protected override TestState CommonWithAdditionalFiles(IEnumerable<AdditionalFile> additionalFiles)
        {
            return WithAdditionalFiles(additionalFiles);
        }

        protected override TestState CommonWithCodeActionTitle(string codeActionTitle)
        {
            return WithCodeActionTitle(codeActionTitle);
        }

        protected override TestState CommonWithEquivalenceKey(string equivalenceKey)
        {
            return WithEquivalenceKey(equivalenceKey);
        }

        public CompilerDiagnosticFixTestState WithDiagnosticId(string diagnosticId)
        {
            return new CompilerDiagnosticFixTestState(this) { DiagnosticId = diagnosticId ?? throw new ArgumentNullException(nameof(diagnosticId)) };
        }

        new public CompilerDiagnosticFixTestState WithSource(string source)
        {
            return new CompilerDiagnosticFixTestState(this) { Source = source ?? throw new ArgumentNullException(nameof(source)) };
        }

        new public CompilerDiagnosticFixTestState WithExpectedSource(string expectedSource)
        {
            return new CompilerDiagnosticFixTestState(this) { ExpectedSource = expectedSource };
        }

        new public CompilerDiagnosticFixTestState WithAdditionalFiles(IEnumerable<AdditionalFile> additionalFiles)
        {
            return new CompilerDiagnosticFixTestState(this) { AdditionalFiles = additionalFiles?.ToImmutableArray() ?? ImmutableArray<AdditionalFile>.Empty };
        }

        new public CompilerDiagnosticFixTestState WithCodeActionTitle(string codeActionTitle)
        {
            return new CompilerDiagnosticFixTestState(this) { CodeActionTitle = codeActionTitle };
        }

        new public CompilerDiagnosticFixTestState WithEquivalenceKey(string equivalenceKey)
        {
            return new CompilerDiagnosticFixTestState(this) { EquivalenceKey = equivalenceKey };
        }
    }
}
