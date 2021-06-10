// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal class DiagnosticDescriptorFactory
    {
        private const string IdSuffix = "FadeOut";

        public static DiagnosticDescriptorFactory Default { get; } = new DiagnosticDescriptorFactory(AnalyzerRules.Default);

        public AnalyzerRules Rules { get; }

        public DiagnosticDescriptorFactory(AnalyzerRules rules)
        {
            Rules = rules;
        }

        public DiagnosticDescriptor Create(
            string id,
            string title,
            string messageFormat,
            string category,
            DiagnosticSeverity defaultSeverity,
            bool isEnabledByDefault,
            string description = null,
            string helpLinkUri = null,
            params string[] customTags)
        {
            return new DiagnosticDescriptor(
                id: id,
                title: title,
                messageFormat: messageFormat,
                category: category,
                defaultSeverity: Rules.GetDiagnosticSeverityOrDefault(id, defaultSeverity),
                isEnabledByDefault: Rules.IsDiagnosticEnabledOrDefault(id, isEnabledByDefault),
                description: description,
                helpLinkUri: DiagnosticDescriptorUtility.GetHelpLinkUri(helpLinkUri),
                customTags: customTags);
        }

        public static DiagnosticDescriptor CreateFadeOut(DiagnosticDescriptor descriptor)
        {
            return new DiagnosticDescriptor(
                descriptor.Id + IdSuffix,
                descriptor.Title,
                descriptor.MessageFormat,
                DiagnosticCategories.FadeOut,
                DiagnosticSeverity.Hidden,
                isEnabledByDefault: true,
                customTags: new string[] { WellKnownDiagnosticTags.Unnecessary, WellKnownDiagnosticTags.NotConfigurable });
        }
    }
}
