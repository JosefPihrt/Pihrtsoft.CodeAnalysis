// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Diagnostics.Telemetry;

namespace Roslynator.Diagnostics
{
    internal class ProjectAnalysisResult
    {
        internal ProjectAnalysisResult(ProjectId projectId)
            : this(
                projectId,
                ImmutableArray<DiagnosticAnalyzer>.Empty,
                ImmutableArray<Diagnostic>.Empty,
                ImmutableArray<Diagnostic>.Empty,
                ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo>.Empty)
        {
        }

        internal ProjectAnalysisResult(
            ProjectId projectId,
            ImmutableArray<DiagnosticAnalyzer> analyzers,
            ImmutableArray<Diagnostic> compilerDiagnostics,
            ImmutableArray<Diagnostic> diagnostics,
            ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo> telemetry)
        {
            ProjectId = projectId;
            Analyzers = analyzers;
            CompilerDiagnostics = compilerDiagnostics;
            Diagnostics = diagnostics;
            Telemetry = telemetry;
        }

        public ProjectId ProjectId { get; }

        public ImmutableArray<DiagnosticAnalyzer> Analyzers { get; }

        public ImmutableArray<Diagnostic> CompilerDiagnostics { get; }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo> Telemetry { get; }

        public IEnumerable<Diagnostic> GetAllDiagnostics()
        {
            return CompilerDiagnostics.Concat(Diagnostics);
        }
    }
}
