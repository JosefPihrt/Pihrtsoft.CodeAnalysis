// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Documentation;
using Roslynator.Documentation.Markdown;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal class GenerateDocRootCommand : MSBuildWorkspaceCommand<BaseCommandResult>
    {
        private static readonly Encoding _defaultEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        public GenerateDocRootCommand(
            GenerateDocRootCommandLineOptions options,
            DocumentationDepth depth,
            RootDocumentationParts ignoredParts,
            IncludeContainingNamespaceFilter includeContainingNamespaceFilter,
            Visibility visibility,
            in ProjectFilter projectFilter) : base(projectFilter)
        {
            Options = options;
            Depth = depth;
            IgnoredParts = ignoredParts;
            IncludeContainingNamespaceFilter = includeContainingNamespaceFilter;
            Visibility = visibility;
        }

        public GenerateDocRootCommandLineOptions Options { get; }

        public DocumentationDepth Depth { get; }

        public RootDocumentationParts IgnoredParts { get; }

        public IncludeContainingNamespaceFilter IncludeContainingNamespaceFilter { get; }

        public Visibility Visibility { get; }

        public override async Task<BaseCommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            AssemblyResolver.Register();

            var documentationOptions = new DocumentationOptions(
                ignoredNames: Options.IgnoredNames,
                rootDirectoryUrl: Options.RootDirectoryUrl,
                placeSystemNamespaceFirst: !Options.NoPrecedenceForSystem,
                markObsolete: !Options.NoMarkObsolete,
                depth: Depth,
                ignoredRootParts: IgnoredParts,
                includeContainingNamespaceFilter: IncludeContainingNamespaceFilter,
                includeSystemNamespace: Options.IncludeSystemNamespace,
                scrollToContent: Options.ScrollToContent);

            ImmutableArray<Compilation> compilations = await GetCompilationsAsync(projectOrSolution, cancellationToken);

            var documentationModel = new DocumentationModel(compilations, DocumentationFilterOptions.Instance);

            var generator = new MarkdownDocumentationGenerator(documentationModel, WellKnownUrlProviders.GitHub, documentationOptions);

            string path = Options.Output;

            WriteLine($"Generate documentation root to '{path}'.", Verbosity.Minimal);

            string heading = Options.Heading;

            if (string.IsNullOrEmpty(heading))
            {
                string fileName = Path.GetFileName(Options.Output);

                heading = (fileName.EndsWith(".dll", StringComparison.Ordinal))
                    ? Path.GetFileNameWithoutExtension(fileName)
                    : fileName;
            }

            DocumentationGeneratorResult result = generator.GenerateRoot(heading);

            File.WriteAllText(path, result.Content, _defaultEncoding);

            WriteLine($"Documentation root successfully generated to '{path}'.", Verbosity.Minimal);

            return BaseCommandResult.Success;
        }
    }
}
