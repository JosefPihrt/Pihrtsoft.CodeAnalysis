// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Roslynator.Metadata
{
    public class AnalyzerOptionMetadata
    {
        public AnalyzerOptionMetadata(
            string identifier,
            string id,
            string parentId,
            string optionKey,
            string optionValue,
            AnalyzerOptionKind kind,
            string title,
            bool isEnabledByDefault,
            bool supportsFadeOut,
            string minLanguageVersion,
            string summary,
            IEnumerable<SampleMetadata> samples,
            bool isObsolete)
        {
            Identifier = identifier;
            Id = id;
            ParentId = parentId;
            OptionKey = optionKey;
            OptionValue = optionValue;
            Kind = kind;
            Title = title;
            IsEnabledByDefault = isEnabledByDefault;
            SupportsFadeOut = supportsFadeOut;
            MinLanguageVersion = minLanguageVersion;
            Summary = summary;
            Samples = new ReadOnlyCollection<SampleMetadata>(samples?.ToArray() ?? Array.Empty<SampleMetadata>());
            IsObsolete = isObsolete;
        }

        public AnalyzerMetadata CreateAnalyzerMetadata(AnalyzerMetadata parent)
        {
            return new AnalyzerMetadata(
                id: (Id != null) ? parent.Id + Id : null,
                identifier: Identifier,
                title: Title,
                messageFormat: Title,
                category: "AnalyzerOption",
                defaultSeverity: parent.DefaultSeverity,
                isEnabledByDefault: IsEnabledByDefault,
                isObsolete: parent.IsObsolete || IsObsolete,
                supportsFadeOut: SupportsFadeOut,
                supportsFadeOutAnalyzer: false,
                minLanguageVersion: MinLanguageVersion ?? parent.MinLanguageVersion,
                summary: Summary,
                remarks: null,
                samples: Samples,
                links: null,
                options: null,
                kind: Kind,
                parent: parent);
        }

        public string Identifier { get; }

        public string Id { get; }

        public string ParentId { get; }

        public string OptionKey { get; }

        public string OptionValue { get; }

        public AnalyzerOptionKind Kind { get; }

        public string Title { get; }

        public bool IsEnabledByDefault { get; }

        public bool SupportsFadeOut { get; }

        public string MinLanguageVersion { get; }

        public string Summary { get; }

        public IReadOnlyList<SampleMetadata> Samples { get; }

        public bool IsObsolete { get; }
    }
}
