// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Roslynator.Documentation
{
    internal class DocumentationCommentGeneratorSettings
    {
        public DocumentationCommentGeneratorSettings(
            IEnumerable<string> summary = null,
            IEnumerable<string> ignoredTags = null,
            string indentation = null,
            bool singleLineSummary = false)
        {
            Summary = (summary != null) ? ImmutableArray.CreateRange(summary) : ImmutableArray<string>.Empty;
            IgnoredTags = ignoredTags?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
            Indentation = indentation ?? "";
            SingleLineSummary = singleLineSummary;
        }

        public static DocumentationCommentGeneratorSettings Default { get; } = new DocumentationCommentGeneratorSettings();

        public ImmutableArray<string> IgnoredTags { get; }

        public ImmutableArray<string> Summary { get; }

        public string Indentation { get; }

        public bool SingleLineSummary { get; }

        public bool IsTagIgnored(string tag)
        {
            return IgnoredTags.Contains(tag);
        }

        public DocumentationCommentGeneratorSettings WithIndentation(string indentation)
        {
            return new DocumentationCommentGeneratorSettings(
                summary: Summary,
                ignoredTags: IgnoredTags,
                indentation: indentation,
                singleLineSummary: SingleLineSummary);
        }
    }
}
