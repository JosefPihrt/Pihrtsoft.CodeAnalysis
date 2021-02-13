﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    public class CSharpProjectOptions : ProjectOptions
    {
        private static CSharpProjectOptions _default_CSharp5;
        private static CSharpProjectOptions _default_CSharp6;
        private static CSharpProjectOptions _default_CSharp7;
        private static CSharpProjectOptions _default_CSharp7_3;
        private static CSharpProjectOptions _default_NullableReferenceTypes;

        public CSharpProjectOptions(
            CSharpCompilationOptions compilationOptions,
            CSharpParseOptions parseOptions,
            IEnumerable<MetadataReference> metadataReferences)
            : base(metadataReferences)
        {
            CompilationOptions = compilationOptions;
            ParseOptions = parseOptions;
        }

        public override string Language => LanguageNames.CSharp;

        public override string DefaultDocumentName => "Test.cs";

        /// <summary>
        /// Gets a parse options that should be used to parse tested source code.
        /// </summary>
        new public CSharpParseOptions ParseOptions { get; }

        /// <summary>
        /// Gets a compilation options that should be used to compile test project.
        /// </summary>
        new public CSharpCompilationOptions CompilationOptions { get; }

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
        public static CSharpProjectOptions Default { get; } = CreateDefault();

        private static CSharpProjectOptions CreateDefault()
        {
            CSharpParseOptions parseOptions = null;
            CSharpCompilationOptions compilationOptions = null;

            using (var workspace = new AdhocWorkspace())
            {
                Project project = workspace
                    .CurrentSolution
                    .AddProject("TestProject", "TestProject", LanguageNames.CSharp);

                compilationOptions = ((CSharpCompilationOptions)project.CompilationOptions)
                    .WithOutputKind(OutputKind.DynamicallyLinkedLibrary);

                parseOptions = ((CSharpParseOptions)project.ParseOptions);

                parseOptions = parseOptions
                    .WithLanguageVersion(LanguageVersion.LatestMajor)
                    .WithPreprocessorSymbols(parseOptions.PreprocessorSymbolNames.Concat(new[] { "DEBUG" }));
            }

            return new CSharpProjectOptions(
                compilationOptions: compilationOptions,
                parseOptions: parseOptions,
                metadataReferences: RuntimeMetadataReference.MetadataReferences.Select(f => f.Value).ToImmutableArray()
            );
        }

        internal static CSharpProjectOptions Default_CSharp5
        {
            get
            {
                if (_default_CSharp5 == null)
                    Interlocked.CompareExchange(ref _default_CSharp5, Create(), null);

                return _default_CSharp5;

                static CSharpProjectOptions Create() => Default.WithParseOptions(Default.ParseOptions.WithLanguageVersion(LanguageVersion.CSharp5));
            }
        }

        internal static CSharpProjectOptions Default_CSharp6
        {
            get
            {
                if (_default_CSharp6 == null)
                    Interlocked.CompareExchange(ref _default_CSharp6, Create(), null);

                return _default_CSharp6;

                static CSharpProjectOptions Create() => Default.WithParseOptions(Default.ParseOptions.WithLanguageVersion(LanguageVersion.CSharp6));
            }
        }

        internal static CSharpProjectOptions Default_CSharp7
        {
            get
            {
                if (_default_CSharp7 == null)
                    Interlocked.CompareExchange(ref _default_CSharp7, Create(), null);

                return _default_CSharp7;

                static CSharpProjectOptions Create() => Default.WithParseOptions(Default.ParseOptions.WithLanguageVersion(LanguageVersion.CSharp7));
            }
        }

        internal static CSharpProjectOptions Default_CSharp7_3
        {
            get
            {
                if (_default_CSharp7_3 == null)
                    Interlocked.CompareExchange(ref _default_CSharp7_3, Create(), null);

                return _default_CSharp7_3;

                static CSharpProjectOptions Create() => Default.WithParseOptions(Default.ParseOptions.WithLanguageVersion(LanguageVersion.CSharp7_3));
            }
        }

        internal static CSharpProjectOptions Default_NullableReferenceTypes
        {
            get
            {
                if (_default_NullableReferenceTypes == null)
                    Interlocked.CompareExchange(ref _default_NullableReferenceTypes, Create(), null);

                return _default_NullableReferenceTypes;

                static CSharpProjectOptions Create() => Default.WithCompilationOptions(Default.CompilationOptions.WithNullableContextOptions(NullableContextOptions.Enable));
            }
        }

        ///// <summary>
        ///// Adds specified compiler diagnostic ID to the list of allowed compiler diagnostic IDs.
        ///// </summary>
        ///// <param name="diagnosticId"></param>
        //public CSharpProjectOptions AddAllowedCompilerDiagnosticId(string diagnosticId)
        //{
        //    return WithAllowedCompilerDiagnosticIds(AllowedCompilerDiagnosticIds.Add(diagnosticId));
        //}

        ///// <summary>
        ///// Adds a list of specified compiler diagnostic IDs to the list of allowed compiler diagnostic IDs.
        ///// </summary>
        ///// <param name="diagnosticIds"></param>
        //public CSharpProjectOptions AddAllowedCompilerDiagnosticIds(IEnumerable<string> diagnosticIds)
        //{
        //    return WithAllowedCompilerDiagnosticIds(AllowedCompilerDiagnosticIds.AddRange(diagnosticIds));
        //}

        /// <summary>
        /// Adds specified assembly name to the list of assembly names.
        /// </summary>
        /// <param name="metadataReference"></param>
        public CSharpProjectOptions AddMetadataReferences(MetadataReference metadataReference)
        {
            return WithMetadataReferences(MetadataReferences.Add(metadataReference));
        }

#pragma warning disable CS1591

        public CSharpProjectOptions WithParseOptions(CSharpParseOptions parseOptions)
        {
            return new CSharpProjectOptions(
                compilationOptions: CompilationOptions,
                parseOptions: parseOptions,
                metadataReferences: MetadataReferences);
        }

        public CSharpProjectOptions WithCompilationOptions(CSharpCompilationOptions compilationOptions)
        {
            return new CSharpProjectOptions(
                compilationOptions: compilationOptions,
                parseOptions: ParseOptions,
                metadataReferences: MetadataReferences);
        }

        public CSharpProjectOptions WithMetadataReferences(IEnumerable<MetadataReference> metadataReferences)
        {
            return new CSharpProjectOptions(
                compilationOptions: CompilationOptions,
                parseOptions: ParseOptions,
                metadataReferences: metadataReferences);
        }
#pragma warning restore CS1591
    }
}
