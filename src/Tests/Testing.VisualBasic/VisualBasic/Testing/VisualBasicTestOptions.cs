// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Roslynator.Testing
{
    public sealed class VisualBasicTestOptions : TestOptions
    {
        public VisualBasicTestOptions(
            VisualBasicCompilationOptions compilationOptions = null,
            VisualBasicParseOptions parseOptions = null,
            IEnumerable<MetadataReference> metadataReferences = null,
            IEnumerable<string> allowedCompilerDiagnosticIds = null,
            DiagnosticSeverity allowedCompilerDiagnosticSeverity = DiagnosticSeverity.Info)
            : base(metadataReferences, allowedCompilerDiagnosticIds, allowedCompilerDiagnosticSeverity)
        {
            CompilationOptions = compilationOptions ?? new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            ParseOptions = parseOptions ?? VisualBasicParseOptions.Default;
        }

        private VisualBasicTestOptions(VisualBasicTestOptions other)
            : base(
                other.MetadataReferences,
                other.AllowedCompilerDiagnosticIds,
                other.AllowedCompilerDiagnosticSeverity)
        {
            CompilationOptions = other.CompilationOptions;
            ParseOptions = other.ParseOptions;
        }

        public override string Language => LanguageNames.VisualBasic;

        internal override string DocumentName => "Test.vb";

        new public VisualBasicParseOptions ParseOptions { get; private set; }

        new public VisualBasicCompilationOptions CompilationOptions { get; private set; }

        protected override ParseOptions CommonParseOptions => ParseOptions;

        protected override CompilationOptions CommonCompilationOptions => CompilationOptions;

        public static VisualBasicTestOptions Default { get; } = CreateDefault();

        private static VisualBasicTestOptions CreateDefault()
        {
            var parseOptions = VisualBasicParseOptions.Default;

            var compilationOptions = new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            return new VisualBasicTestOptions(
                compilationOptions: compilationOptions,
                parseOptions: parseOptions,
                metadataReferences: RuntimeMetadataReference.DefaultMetadataReferences.Select(f => f.Value).ToImmutableArray(),
                allowedCompilerDiagnosticIds: null,
                allowedCompilerDiagnosticSeverity: DiagnosticSeverity.Info);
        }

        public VisualBasicTestOptions AddMetadataReference(MetadataReference metadataReference)
        {
            return WithMetadataReferences(MetadataReferences.Add(metadataReference));
        }

        protected override TestOptions CommonWithMetadataReferences(IEnumerable<MetadataReference> values)
        {
            return new VisualBasicTestOptions(this) { MetadataReferences = values?.ToImmutableArray() ?? ImmutableArray<MetadataReference>.Empty };
        }

        protected override TestOptions CommonWithAllowedCompilerDiagnosticIds(IEnumerable<string> values)
        {
            return new VisualBasicTestOptions(this) { AllowedCompilerDiagnosticIds = values?.ToImmutableArray() ?? ImmutableArray<string>.Empty };
        }

        protected override TestOptions CommonWithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity value)
        {
            return new VisualBasicTestOptions(this) { AllowedCompilerDiagnosticSeverity = value };
        }

        new public VisualBasicTestOptions WithMetadataReferences(IEnumerable<MetadataReference> values)
        {
            return (VisualBasicTestOptions)base.WithMetadataReferences(values);
        }

        new public VisualBasicTestOptions WithAllowedCompilerDiagnosticIds(IEnumerable<string> values)
        {
            return (VisualBasicTestOptions)base.WithAllowedCompilerDiagnosticIds(values);
        }

        new public VisualBasicTestOptions WithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity value)
        {
            return (VisualBasicTestOptions)base.WithAllowedCompilerDiagnosticSeverity(value);
        }

        public VisualBasicTestOptions WithParseOptions(VisualBasicParseOptions parseOptions)
        {
            return new VisualBasicTestOptions(this) { ParseOptions = parseOptions ?? throw new ArgumentNullException(nameof(parseOptions)) };
        }

        public VisualBasicTestOptions WithCompilationOptions(VisualBasicCompilationOptions compilationOptions)
        {
            return new VisualBasicTestOptions(this) { CompilationOptions = compilationOptions ?? throw new ArgumentNullException(nameof(compilationOptions)) };
        }
    }
}
