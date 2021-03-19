// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents options for a code verifier.
    /// </summary>
    public abstract class TestOptions
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TestOptions"/>.
        /// </summary>
        /// <param name="metadataReferences"></param>
        /// <param name="allowedCompilerDiagnosticIds"></param>
        /// <param name="allowedCompilerDiagnosticSeverity"></param>
        internal TestOptions(
            IEnumerable<MetadataReference> metadataReferences = null,
            IEnumerable<string> allowedCompilerDiagnosticIds = null,
            DiagnosticSeverity allowedCompilerDiagnosticSeverity = DiagnosticSeverity.Info)
        {
            MetadataReferences = metadataReferences?.ToImmutableArray() ?? ImmutableArray<MetadataReference>.Empty;
            AllowedCompilerDiagnosticIds = allowedCompilerDiagnosticIds?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
            AllowedCompilerDiagnosticSeverity = allowedCompilerDiagnosticSeverity;
        }

        /// <summary>
        /// Gets a programming language identifier.
        /// </summary>
        public abstract string Language { get; }

        internal abstract string DocumentName { get; }

        /// <summary>
        /// Gets a common parse options.
        /// </summary>
        protected abstract ParseOptions CommonParseOptions { get; }

        /// <summary>
        /// Gets a common compilation options.
        /// </summary>
        protected abstract CompilationOptions CommonCompilationOptions { get; }

        /// <summary>
        /// Gets a parse options that should be used to parse tested source code.
        /// </summary>
        public ParseOptions ParseOptions => CommonParseOptions;

        /// <summary>
        /// Gets a compilation options that should be used to compile test project.
        /// </summary>
        public CompilationOptions CompilationOptions => CommonCompilationOptions;

        /// <summary>
        /// Gets metadata references of a test project.
        /// </summary>
        public ImmutableArray<MetadataReference> MetadataReferences { get; protected set; }

        /// <summary>
        /// Gets a collection of compiler diagnostic IDs.
        /// </summary>
        public ImmutableArray<string> AllowedCompilerDiagnosticIds { get; protected set; }

        /// <summary>
        /// Gets a maximal allowed compiler diagnostic severity.
        /// </summary>
        public DiagnosticSeverity AllowedCompilerDiagnosticSeverity { get; protected set; }

#pragma warning disable CS1591
        protected abstract TestOptions CommonWithMetadataReferences(IEnumerable<MetadataReference> values);

        protected abstract TestOptions CommonWithAllowedCompilerDiagnosticIds(IEnumerable<string> values);

        protected abstract TestOptions CommonWithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity value);

        public TestOptions WithMetadataReferences(IEnumerable<MetadataReference> values)
        {
            return CommonWithMetadataReferences(values);
        }

        public TestOptions WithAllowedCompilerDiagnosticIds(IEnumerable<string> values)
        {
            return CommonWithAllowedCompilerDiagnosticIds(values);
        }

        public TestOptions WithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity value)
        {
            return CommonWithAllowedCompilerDiagnosticSeverity(value);
        }
#pragma warning restore CS1591

        internal bool IsAllowedCompilerDiagnostic(Diagnostic diagnostic)
        {
            if (diagnostic.Severity <= AllowedCompilerDiagnosticSeverity)
                return true;

            foreach (string diagnosticId in AllowedCompilerDiagnosticIds)
            {
                if (diagnostic.Id == diagnosticId)
                    return true;
            }

            return false;
        }
    }
}
