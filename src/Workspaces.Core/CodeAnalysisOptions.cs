// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal abstract class CodeAnalysisOptions
    {
        internal CodeAnalysisOptions(
            DiagnosticSeverity severityLevel = DiagnosticSeverity.Info,
            bool ignoreAnalyzerReferences = false,
            bool concurrentAnalysis = true,
            IEnumerable<string> supportedDiagnosticIds = null,
            IEnumerable<string> ignoredDiagnosticIds = null)
        {
            if (supportedDiagnosticIds?.Any() == true
                && ignoredDiagnosticIds?.Any() == true)
            {
                throw new ArgumentException($"Cannot specify both '{nameof(supportedDiagnosticIds)}' and '{nameof(ignoredDiagnosticIds)}'.", nameof(ignoredDiagnosticIds));
            }

            SeverityLevel = severityLevel;
            IgnoreAnalyzerReferences = ignoreAnalyzerReferences;
            ConcurrentAnalysis = concurrentAnalysis;
            SupportedDiagnosticIds = supportedDiagnosticIds?.ToImmutableHashSet() ?? ImmutableHashSet<string>.Empty;
            IgnoredDiagnosticIds = ignoredDiagnosticIds?.ToImmutableHashSet() ?? ImmutableHashSet<string>.Empty;
        }

        public DiagnosticSeverity SeverityLevel { get; }

        public bool IgnoreAnalyzerReferences { get; }

        public bool ConcurrentAnalysis { get; }

        public ImmutableHashSet<string> SupportedDiagnosticIds { get; }

        public ImmutableHashSet<string> IgnoredDiagnosticIds { get; }

        internal bool IsSupportedDiagnosticId(string diagnosticId)
        {
            return (SupportedDiagnosticIds.Count > 0)
                ? SupportedDiagnosticIds.Contains(diagnosticId)
                : !IgnoredDiagnosticIds.Contains(diagnosticId);
        }
    }
}
