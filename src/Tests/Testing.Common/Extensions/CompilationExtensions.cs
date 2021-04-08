// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal static class CompilationExtensions
    {
        public static Compilation EnsureDiagnosticEnabled(this Compilation compilation, DiagnosticDescriptor descriptor)
        {
            return compilation.WithOptions(compilation.Options.EnsureDiagnosticEnabled(descriptor));
        }

        public static Compilation EnsureDiagnosticEnabled(this Compilation compilation, IEnumerable<DiagnosticDescriptor> descriptors)
        {
            CompilationOptions compilationOptions = compilation.Options;

            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = compilationOptions.SpecificDiagnosticOptions;

            specificDiagnosticOptions = specificDiagnosticOptions.SetItems(
                descriptors
                    .Where(f => !f.IsEnabledByDefault)
                    .Select(f => new KeyValuePair<string, ReportDiagnostic>(f.Id, f.DefaultSeverity.ToReportDiagnostic())));

            return compilation.WithOptions(compilationOptions.WithSpecificDiagnosticOptions(specificDiagnosticOptions));
        }
    }
}
