// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator.CodeFixes
{
    internal class SimpleFixResult
    {
        internal SimpleFixResult(
            IEnumerable<KeyValuePair<DiagnosticDescriptor, int>> fixedDiagnostics = default,
            IEnumerable<KeyValuePair<DiagnosticDescriptor, int>> unfixedDiagnostics = default,
            IEnumerable<KeyValuePair<DiagnosticDescriptor, int>> unfixableDiagnostics = default,
            int numberOfFormattedDocuments = 0,
            int numberOfAddedFileBanners = 0)
        {
            FixedDiagnostics = fixedDiagnostics?.ToImmutableDictionary() ?? ImmutableDictionary<DiagnosticDescriptor, int>.Empty;
            UnfixedDiagnostics = unfixedDiagnostics?.ToImmutableDictionary() ?? ImmutableDictionary<DiagnosticDescriptor, int>.Empty;
            UnfixableDiagnostics = unfixableDiagnostics?.ToImmutableDictionary() ?? ImmutableDictionary<DiagnosticDescriptor, int>.Empty;
            NumberOfFormattedDocuments = numberOfFormattedDocuments;
            NumberOfAddedFileBanners = numberOfAddedFileBanners;
        }

        public ImmutableDictionary<DiagnosticDescriptor, int> FixedDiagnostics { get; }

        public ImmutableDictionary<DiagnosticDescriptor, int> UnfixedDiagnostics { get; }

        public ImmutableDictionary<DiagnosticDescriptor, int> UnfixableDiagnostics { get; }

        public int NumberOfFormattedDocuments { get; }

        public int NumberOfAddedFileBanners { get; }

        public static SimpleFixResult Create(IEnumerable<ProjectFixResult> results)
        {
            return new SimpleFixResult(
                fixedDiagnostics: results
                    .SelectMany(f => f.FixedDiagnostics)
                    .GroupBy(f => f.Descriptor, DiagnosticDescriptorComparer.Id)
                    .ToImmutableDictionary(f => f.Key, f => f.Count()),
                unfixedDiagnostics: results
                    .SelectMany(f => f.UnfixedDiagnostics)
                    .GroupBy(f => f.Descriptor, DiagnosticDescriptorComparer.Id)
                    .ToImmutableDictionary(f => f.Key, f => f.Count()),
                unfixableDiagnostics: results
                    .SelectMany(f => f.UnfixableDiagnostics)
                    .GroupBy(f => f.Descriptor, DiagnosticDescriptorComparer.Id)
                    .ToImmutableDictionary(f => f.Key, f => f.Count()),
                numberOfFormattedDocuments: results.Sum(f => f.NumberOfFormattedDocuments),
                numberOfAddedFileBanners: results.Sum(f => f.NumberOfAddedFileBanners));
        }
    }
}
