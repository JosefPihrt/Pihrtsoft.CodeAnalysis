// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Roslynator.CodeFixes
{
    internal class ProjectFixResult
    {
        internal static ProjectFixResult Skipped { get; } = new ProjectFixResult(ProjectFixKind.Skipped);

        internal ProjectFixResult(
            ProjectFixKind kind,
            IEnumerable<DiagnosticInfo> fixedDiagnostics = default,
            IEnumerable<DiagnosticInfo> unfixedDiagnostics = default,
            IEnumerable<DiagnosticInfo> unfixableDiagnostics = default,
            int numberOfFormattedDocuments = -1,
            int numberOfAddedFileBanners = -1)
        {
            Kind = kind;
            FixedDiagnostics = fixedDiagnostics?.ToImmutableArray() ?? ImmutableArray<DiagnosticInfo>.Empty;
            UnfixedDiagnostics = unfixedDiagnostics?.ToImmutableArray() ?? ImmutableArray<DiagnosticInfo>.Empty;
            UnfixableDiagnostics = unfixableDiagnostics?.ToImmutableArray() ?? ImmutableArray<DiagnosticInfo>.Empty;
            NumberOfFormattedDocuments = numberOfFormattedDocuments;
            NumberOfAddedFileBanners = numberOfAddedFileBanners;
        }

        public ProjectFixKind Kind { get; }

        public ImmutableArray<DiagnosticInfo> FixedDiagnostics { get; }

        public ImmutableArray<DiagnosticInfo> UnfixedDiagnostics { get; }

        public ImmutableArray<DiagnosticInfo> UnfixableDiagnostics { get; }

        public int NumberOfFormattedDocuments { get; }

        public int NumberOfAddedFileBanners { get; }
    }
}
