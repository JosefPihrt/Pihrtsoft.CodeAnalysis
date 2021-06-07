// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Roslynator.CodeFixes
{
    internal class CodeFixerOptions : CodeAnalysisOptions
    {
        private ImmutableArray<string> _fileBannerLines;

        public static CodeFixerOptions Default { get; } = new CodeFixerOptions();

        public CodeFixerOptions(
            DiagnosticSeverity severityLevel = DiagnosticSeverity.Info,
            bool ignoreCompilerErrors = false,
            bool ignoreAnalyzerReferences = false,
            bool concurrentAnalysis = true,
            IEnumerable<string> supportedDiagnosticIds = null,
            IEnumerable<string> ignoredDiagnosticIds = null,
            IEnumerable<string> ignoredCompilerDiagnosticIds = null,
            IEnumerable<string> diagnosticIdsFixableOneByOne = null,
            IEnumerable<KeyValuePair<string, string>> diagnosticFixMap = null,
            IEnumerable<KeyValuePair<string, string>> diagnosticFixerMap = null,
            FixAllScope fixAllScope = FixAllScope.Project,
            string fileBanner = null,
            int maxIterations = -1,
            int batchSize = -1,
            bool format = false) : base(
                severityLevel: severityLevel,
                ignoreAnalyzerReferences: ignoreAnalyzerReferences,
                concurrentAnalysis: concurrentAnalysis,
                supportedDiagnosticIds: supportedDiagnosticIds,
                ignoredDiagnosticIds: ignoredDiagnosticIds)
        {
            IgnoreCompilerErrors = ignoreCompilerErrors;
            IgnoredCompilerDiagnosticIds = ignoredCompilerDiagnosticIds?.ToImmutableHashSet() ?? ImmutableHashSet<string>.Empty;
            DiagnosticIdsFixableOneByOne = diagnosticIdsFixableOneByOne?.ToImmutableHashSet() ?? ImmutableHashSet<string>.Empty;

            if (diagnosticFixMap != null)
            {
                DiagnosticFixMap = diagnosticFixMap
                    .GroupBy(kvp => kvp.Key)
                    .ToImmutableDictionary(g => g.Key, g => g.Select(kvp => kvp.Value).ToImmutableArray());
            }
            else
            {
                DiagnosticFixMap = ImmutableDictionary<string, ImmutableArray<string>>.Empty;
            }

            DiagnosticFixerMap = diagnosticFixerMap?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;

            if (FixAllScope != FixAllScope.Document
                && FixAllScope != FixAllScope.Project)
            {
                throw new ArgumentException("", nameof(fixAllScope));
            }

            FixAllScope = fixAllScope;
            FileBanner = fileBanner;
            MaxIterations = maxIterations;
            BatchSize = batchSize;
            Format = format;
        }

        public bool IgnoreCompilerErrors { get; }

        public ImmutableHashSet<string> IgnoredCompilerDiagnosticIds { get; }

        public string FileBanner { get; }

        public ImmutableArray<string> FileBannerLines
        {
            get
            {
                if (_fileBannerLines.IsDefault)
                {
                    if (!string.IsNullOrEmpty(FileBanner))
                    {
                        ImmutableArray<string>.Builder lines = ImmutableArray.CreateBuilder<string>();

                        using (var sr = new StringReader(FileBanner))
                        {
                            string line = null;
                            while ((line = sr.ReadLine()) != null)
                            {
                                lines.Add(line);
                            }
                        }

                        _fileBannerLines = lines.ToImmutableArray();
                    }
                    else
                    {
                        _fileBannerLines = ImmutableArray<string>.Empty;
                    }
                }

                return _fileBannerLines;
            }
        }

        public int MaxIterations { get; }

        public int BatchSize { get; }

        public bool Format { get; }

        public ImmutableHashSet<string> DiagnosticIdsFixableOneByOne { get; }

        public ImmutableDictionary<string, ImmutableArray<string>> DiagnosticFixMap { get; }

        public ImmutableDictionary<string, string> DiagnosticFixerMap { get; }

        public FixAllScope FixAllScope { get; }
    }
}
