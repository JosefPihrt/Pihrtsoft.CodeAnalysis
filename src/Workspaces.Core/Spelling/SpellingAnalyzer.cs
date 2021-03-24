// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Host.Mef;
using System.Diagnostics.CodeAnalysis;

namespace Roslynator.Spelling
{
    internal sealed class SpellingAnalyzer
    {
        public const string DiagnosticId = "RCS8000";

        [SuppressMessage("MicrosoftCodeAnalysisReleaseTracking", "RS2008:Enable analyzer release tracking")]
        public static readonly DiagnosticDescriptor DiagnosticDescriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Possible misspelling or typo.",
            messageFormat: "Possible misspelling or typo in '{0}'.",
            category: "Spelling",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: null,
            helpLinkUri: null,
            customTags: Array.Empty<string>());

        public static async Task<ImmutableArray<Diagnostic>> AnalyzeSpellingAsync(
            Project project,
            SpellingData spellingData,
            SpellingFixerOptions options = null,
            CancellationToken cancellationToken = default)
        {
            ISpellingService service = MefWorkspaceServices.Default.GetService<ISpellingService>(project.Language);

            if (service == null)
                return ImmutableArray<Diagnostic>.Empty;

            ImmutableArray<Diagnostic>.Builder diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            foreach (Document document in project.Documents)
            {
                if (!document.SupportsSyntaxTree)
                    continue;

                ImmutableArray<Diagnostic> diagnostics2 = await AnalyzeSpellingAsync(
                    service,
                    document,
                    spellingData,
                    options,
                    cancellationToken)
                    .ConfigureAwait(false);

                diagnostics.AddRange(diagnostics2);
            }

            return diagnostics.ToImmutableArray();
        }

        public static async Task<ImmutableArray<Diagnostic>> AnalyzeSpellingAsync(
            ISpellingService service,
            Document document,
            SpellingData spellingData,
            SpellingFixerOptions options = null,
            CancellationToken cancellationToken = default)
        {
            SyntaxTree tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

            if (tree == null)
                return ImmutableArray<Diagnostic>.Empty;

            if (!options.IncludeGeneratedCode
                && GeneratedCodeUtility.IsGeneratedCode(tree, f => service.SyntaxFacts.IsComment(f), cancellationToken))
            {
                return ImmutableArray<Diagnostic>.Empty;
            }

            SyntaxNode root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            return service.AnalyzeSpelling(root, spellingData, options, cancellationToken);
        }
    }
}
