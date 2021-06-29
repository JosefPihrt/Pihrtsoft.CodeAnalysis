// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator.Diagnostics
{
    internal class SimpleAnalysisResult
    {
        public SimpleAnalysisResult(
            IEnumerable<KeyValuePair<DiagnosticDescriptor, int>> compilerDiagnostics,
            IEnumerable<KeyValuePair<DiagnosticDescriptor, int>> diagnostics)
        {
            CompilerDiagnostics = compilerDiagnostics?.ToImmutableDictionary() ?? ImmutableDictionary<DiagnosticDescriptor, int>.Empty;
            Diagnostics = diagnostics?.ToImmutableDictionary() ?? ImmutableDictionary<DiagnosticDescriptor, int>.Empty;
        }

        public ImmutableDictionary<DiagnosticDescriptor, int> CompilerDiagnostics { get; }

        public ImmutableDictionary<DiagnosticDescriptor, int> Diagnostics { get; }

        public static SimpleAnalysisResult Create(IEnumerable<ProjectAnalysisResult> results)
        {
            return new SimpleAnalysisResult(
                compilerDiagnostics: results
                    .SelectMany(f => f.CompilerDiagnostics)
                    .GroupBy(f => f.Descriptor, DiagnosticDescriptorComparer.Id)
                    .ToImmutableDictionary(f => f.Key, f => f.Count()),
                diagnostics: results
                    .SelectMany(f => f.Diagnostics)
                    .GroupBy(f => f.Descriptor, DiagnosticDescriptorComparer.Id)
                    .ToImmutableDictionary(f => f.Key, f => f.Count()));
        }
    }
}
