// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

#pragma warning disable RS2008 // Enable analyzer release tracking

namespace Roslynator
{
    internal static class CommonDiagnosticRules
    {
        public static readonly DiagnosticDescriptor AnalyzerOptionIsObsolete = new DiagnosticDescriptor(
            id: "RCS8001",
            title: "Analyzer option is obsolete.",
            messageFormat: "Analyzer option '{0}' is obsolete. Use EditorConfig option '{1} = true' instead.",
            category: DiagnosticCategories.Usage,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: null,
            helpLinkUri: null,
            customTags:  new string[] { WellKnownDiagnosticTags.NotConfigurable });
    }
}