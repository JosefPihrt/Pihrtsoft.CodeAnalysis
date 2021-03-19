// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Roslynator.Metadata
{
    public class AnalyzerMetadata
    {
        private IReadOnlyList<AnalyzerMetadata> _optionAnalyzers;

        public AnalyzerMetadata(
            string id,
            string identifier,
            string title,
            string messageFormat,
            string category,
            string defaultSeverity,
            bool isEnabledByDefault,
            bool isObsolete,
            bool supportsFadeOut,
            bool supportsFadeOutAnalyzer,
            string minLanguageVersion,
            string summary,
            string remarks,
            IEnumerable<SampleMetadata> samples,
            IEnumerable<LinkMetadata> links,
            IEnumerable<AnalyzerOptionMetadata> options,
            AnalyzerOptionKind kind,
            AnalyzerMetadata parent)
        {
            Id = id;
            Identifier = identifier;
            Title = title;
            MessageFormat = messageFormat;
            Category = category;
            DefaultSeverity = defaultSeverity;
            IsEnabledByDefault = isEnabledByDefault;
            IsObsolete = isObsolete;
            SupportsFadeOut = supportsFadeOut;
            SupportsFadeOutAnalyzer = supportsFadeOutAnalyzer;
            MinLanguageVersion = minLanguageVersion;
            Summary = summary;
            Remarks = remarks;
            Samples = new ReadOnlyCollection<SampleMetadata>(samples?.ToArray() ?? Array.Empty<SampleMetadata>());
            Links = new ReadOnlyCollection<LinkMetadata>(links?.ToArray() ?? Array.Empty<LinkMetadata>());
            Options = new ReadOnlyCollection<AnalyzerOptionMetadata>(options?.ToArray() ?? Array.Empty<AnalyzerOptionMetadata>());
            Kind = kind;
            Parent = parent;

            if (Parent != null)
                _optionAnalyzers = new ReadOnlyCollection<AnalyzerMetadata>(new List<AnalyzerMetadata>());
        }

        public string Id { get; }

        public string Identifier { get; }

        public string Title { get; }

        public string MessageFormat { get; }

        public string Category { get; }

        public string DefaultSeverity { get; }

        public bool IsEnabledByDefault { get; }

        public bool IsObsolete { get; }

        public bool SupportsFadeOut { get; }

        public bool SupportsFadeOutAnalyzer { get; }

        public string MinLanguageVersion { get; }

        public string Summary { get; }

        public string Remarks { get; }

        public IReadOnlyList<SampleMetadata> Samples { get; }

        public IReadOnlyList<LinkMetadata> Links { get; }

        public IReadOnlyList<AnalyzerOptionMetadata> Options { get; }

        public IReadOnlyList<AnalyzerMetadata> OptionAnalyzers
        {
            get
            {
                if (_optionAnalyzers == null)
                {
                    Interlocked.CompareExchange(
                        ref _optionAnalyzers,
                        new ReadOnlyCollection<AnalyzerMetadata>(Options.Select(f => f.CreateAnalyzerMetadata(this)).ToList()),
                        null);
                }

                return _optionAnalyzers;
            }
        }

        public AnalyzerOptionKind Kind { get; }

        public AnalyzerMetadata Parent { get; }
    }
}
