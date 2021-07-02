// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Diagnostics.Telemetry;

namespace Roslynator.Diagnostics
{
    internal class ProjectAnalysisResult
    {
        internal ProjectAnalysisResult(SimpleProjectInfo projectId)
            : this(
                projectId,
                ImmutableArray<DiagnosticInfo>.Empty,
                ImmutableArray<DiagnosticInfo>.Empty,
                ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo>.Empty)
        {
        }

        internal ProjectAnalysisResult(
            SimpleProjectInfo projectId,
            ImmutableArray<DiagnosticInfo> compilerDiagnostics,
            ImmutableArray<DiagnosticInfo> diagnostics,
            ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo> telemetry)
        {
            Project = projectId;
            CompilerDiagnostics = compilerDiagnostics;
            Diagnostics = diagnostics;
            Telemetry = telemetry;
        }

        public SimpleProjectInfo Project { get; }

        public ImmutableArray<DiagnosticInfo> CompilerDiagnostics { get; }

        public ImmutableArray<DiagnosticInfo> Diagnostics { get; }

        public ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo> Telemetry { get; }

        internal static ProjectAnalysisResult Create(Project project)
        {
            return new ProjectAnalysisResult(SimpleProjectInfo.Create(project));
        }

        internal static ProjectAnalysisResult Create(
            Project project,
            ImmutableArray<Diagnostic> compilerDiagnostics,
            ImmutableArray<Diagnostic> diagnostics,
            ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo> telemetry)
        {
            return new ProjectAnalysisResult(
                SimpleProjectInfo.Create(project),
                ImmutableArray.CreateRange(compilerDiagnostics, f => DiagnosticInfo.Create(f)),
                ImmutableArray.CreateRange(diagnostics, f => DiagnosticInfo.Create(f)),
                telemetry);
        }
    }
}
