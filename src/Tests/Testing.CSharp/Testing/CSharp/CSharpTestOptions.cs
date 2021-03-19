// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

#pragma warning disable RCS1223

namespace Roslynator.Testing.CSharp
{
    /// <summary>
    /// Represents options for a C# code verifier.
    /// </summary>
    public sealed class CSharpTestOptions : TestOptions
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CSharpTestOptions"/>.
        /// </summary>
        /// <param name="compilationOptions"></param>
        /// <param name="parseOptions"></param>
        /// <param name="metadataReferences"></param>
        /// <param name="allowedCompilerDiagnosticIds"></param>
        /// <param name="allowedCompilerDiagnosticSeverity"></param>
        public CSharpTestOptions(
            CSharpCompilationOptions compilationOptions = null,
            CSharpParseOptions parseOptions = null,
            IEnumerable<MetadataReference> metadataReferences = null,
            IEnumerable<string> allowedCompilerDiagnosticIds = null,
            DiagnosticSeverity allowedCompilerDiagnosticSeverity = DiagnosticSeverity.Info)
            : base(metadataReferences, allowedCompilerDiagnosticIds, allowedCompilerDiagnosticSeverity)
        {
            CompilationOptions = compilationOptions ?? new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            ParseOptions = parseOptions ?? CSharpParseOptions.Default;
        }

        private CSharpTestOptions(CSharpTestOptions other)
            : base(
                other.MetadataReferences,
                other.AllowedCompilerDiagnosticIds,
                other.AllowedCompilerDiagnosticSeverity)
        {
            CompilationOptions = other.CompilationOptions;
            ParseOptions = other.ParseOptions;
        }

        /// <summary>
        /// Gets C# programming language identifier.
        /// </summary>
        public override string Language => LanguageNames.CSharp;

        internal override string DocumentName => "Test.cs";

        /// <summary>
        /// Gets a parse options that should be used to parse tested source code.
        /// </summary>
        new public CSharpParseOptions ParseOptions { get; private set; }

        /// <summary>
        /// Gets a compilation options that should be used to compile test project.
        /// </summary>
        new public CSharpCompilationOptions CompilationOptions { get; private set; }

        /// <summary>
        /// Gets a common parse options.
        /// </summary>
        protected override ParseOptions CommonParseOptions => ParseOptions;

        /// <summary>
        /// Gets a common compilation options.
        /// </summary>
        protected override CompilationOptions CommonCompilationOptions => CompilationOptions;

        /// <summary>
        /// Gets a default code verification options.
        /// </summary>
        public static CSharpTestOptions Default { get; } = CreateDefault();

        private static CSharpTestOptions CreateDefault()
        {
            return new CSharpTestOptions(
                metadataReferences: RuntimeMetadataReference.DefaultMetadataReferences.Select(f => f.Value).ToImmutableArray());
        }

        /// <summary>
        /// Adds specified compiler diagnostic ID to the list of allowed compiler diagnostic IDs.
        /// </summary>
        /// <param name="diagnosticId"></param>
        public CSharpTestOptions AddAllowedCompilerDiagnosticId(string diagnosticId)
        {
            return WithAllowedCompilerDiagnosticIds(AllowedCompilerDiagnosticIds.Add(diagnosticId));
        }

        /// <summary>
        /// Adds a list of specified compiler diagnostic IDs to the list of allowed compiler diagnostic IDs.
        /// </summary>
        /// <param name="diagnosticIds"></param>
        public CSharpTestOptions AddAllowedCompilerDiagnosticIds(IEnumerable<string> diagnosticIds)
        {
            return WithAllowedCompilerDiagnosticIds(AllowedCompilerDiagnosticIds.AddRange(diagnosticIds));
        }

        internal CSharpTestOptions EnableDiagnostic(DiagnosticDescriptor descriptor)
        {
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = CompilationOptions.SpecificDiagnosticOptions.SetItem(
                descriptor.Id,
                descriptor.DefaultSeverity.ToReportDiagnostic());

            return WithSpecificDiagnosticOptions(specificDiagnosticOptions);
        }

        internal CSharpTestOptions EnableDiagnostic(DiagnosticDescriptor descriptor1, DiagnosticDescriptor descriptor2)
        {
            ImmutableDictionary<string, ReportDiagnostic> options = CompilationOptions.SpecificDiagnosticOptions;

            options = options
                .SetItem(descriptor1.Id, descriptor1.DefaultSeverity.ToReportDiagnostic())
                .SetItem(descriptor2.Id, descriptor2.DefaultSeverity.ToReportDiagnostic());

            return WithSpecificDiagnosticOptions(options);
        }

        internal CSharpTestOptions DisableDiagnostic(DiagnosticDescriptor descriptor)
        {
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = CompilationOptions.SpecificDiagnosticOptions.SetItem(
                descriptor.Id,
                ReportDiagnostic.Suppress);

            return WithSpecificDiagnosticOptions(specificDiagnosticOptions);
        }

        /// <summary>
        /// Adds specified assembly name to the list of assembly names.
        /// </summary>
        /// <param name="metadataReference"></param>
        internal CSharpTestOptions AddMetadataReference(MetadataReference metadataReference)
        {
            return WithMetadataReferences(MetadataReferences.Add(metadataReference));
        }

        internal CSharpTestOptions WithSpecificDiagnosticOptions(IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions)
        {
            return WithCompilationOptions(CompilationOptions.WithSpecificDiagnosticOptions(specificDiagnosticOptions));
        }

        internal CSharpTestOptions WithAllowUnsafe(bool enabled)
        {
            return WithCompilationOptions(CompilationOptions.WithAllowUnsafe(enabled));
        }

        internal CSharpTestOptions WithDebugPreprocessorSymbol()
        {
            return WithParseOptions(
                ParseOptions.WithPreprocessorSymbols(
                    ParseOptions.PreprocessorSymbolNames.Concat(new[] { "DEBUG" })));
        }

#pragma warning disable CS1591
        protected override TestOptions CommonWithMetadataReferences(IEnumerable<MetadataReference> values)
        {
            return new CSharpTestOptions(this) { MetadataReferences = values?.ToImmutableArray() ?? ImmutableArray<MetadataReference>.Empty };
        }

        protected override TestOptions CommonWithAllowedCompilerDiagnosticIds(IEnumerable<string> values)
        {
            return new CSharpTestOptions(this) { AllowedCompilerDiagnosticIds = values?.ToImmutableArray() ?? ImmutableArray<string>.Empty };
        }

        protected override TestOptions CommonWithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity value)
        {
            return new CSharpTestOptions(this) { AllowedCompilerDiagnosticSeverity = value };
        }

        public CSharpTestOptions WithCompilationOptions(CSharpCompilationOptions compilationOptions)
        {
            return new CSharpTestOptions(this) { CompilationOptions = compilationOptions ?? throw new ArgumentNullException(nameof(compilationOptions)) };
        }

        public CSharpTestOptions WithParseOptions(CSharpParseOptions parseOptions)
        {
            return new CSharpTestOptions(this) { ParseOptions = parseOptions ?? throw new ArgumentNullException(nameof(parseOptions)) };
        }

        new public CSharpTestOptions WithMetadataReferences(IEnumerable<MetadataReference> values)
        {
            return (CSharpTestOptions)base.WithMetadataReferences(values);
        }

        new public CSharpTestOptions WithAllowedCompilerDiagnosticIds(IEnumerable<string> values)
        {
            return (CSharpTestOptions)base.WithAllowedCompilerDiagnosticIds(values);
        }

        new public CSharpTestOptions WithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity value)
        {
            return (CSharpTestOptions)base.WithAllowedCompilerDiagnosticSeverity(value);
        }
#pragma warning restore CS1591
    }
}
