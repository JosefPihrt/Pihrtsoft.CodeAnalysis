// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CodeFixes
{
    internal class ProjectFixResult
    {
        internal static ProjectFixResult Skipped { get; } = new ProjectFixResult(ProjectFixKind.Skipped);

        internal ProjectFixResult(
            ProjectFixKind kind,
            IEnumerable<Diagnostic> fixedDiagnostics = default,
            IEnumerable<Diagnostic> unfixedDiagnostics = default,
            IEnumerable<Diagnostic> unfixableDiagnostics = default,
            IEnumerable<DiagnosticAnalyzer> analyzers = default,
            IEnumerable<CodeFixProvider> fixers = default,
            int numberOfFormattedDocuments = -1,
            int numberOfAddedFileBanners = -1)
        {
            Kind = kind;
            FixedDiagnostics = fixedDiagnostics?.ToImmutableArray() ?? ImmutableArray<Diagnostic>.Empty;
            UnfixedDiagnostics = unfixedDiagnostics?.ToImmutableArray() ?? ImmutableArray<Diagnostic>.Empty;
            UnfixableDiagnostics = unfixableDiagnostics?.ToImmutableArray() ?? ImmutableArray<Diagnostic>.Empty;
            Analyzers = analyzers?.ToImmutableArray() ?? ImmutableArray<DiagnosticAnalyzer>.Empty;
            Fixers = fixers?.ToImmutableArray() ?? ImmutableArray<CodeFixProvider>.Empty;
            NumberOfFormattedDocuments = numberOfFormattedDocuments;
            NumberOfAddedFileBanners = numberOfAddedFileBanners;
        }

        public ProjectFixKind Kind { get; }

        public ImmutableArray<Diagnostic> FixedDiagnostics { get; }

        public ImmutableArray<Diagnostic> UnfixedDiagnostics { get; }

        public ImmutableArray<Diagnostic> UnfixableDiagnostics { get; }

        public ImmutableArray<DiagnosticAnalyzer> Analyzers { get; }

        public ImmutableArray<CodeFixProvider> Fixers { get; }

        public int NumberOfFormattedDocuments { get; }

        public int NumberOfAddedFileBanners { get; }
    }
}
