// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Documentation;
using Roslynator.Documentation.Markdown;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal class GenerateDocCommand : MSBuildWorkspaceCommand<BaseCommandResult>
    {
        private static readonly Encoding _defaultEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        public GenerateDocCommand(
            GenerateDocCommandLineOptions options,
            DocumentationDepth depth,
            RootDocumentationParts ignoredRootParts,
            NamespaceDocumentationParts ignoredNamespaceParts,
            TypeDocumentationParts ignoredTypeParts,
            MemberDocumentationParts ignoredMemberParts,
            OmitMemberParts omitMemberParts,
            IncludeContainingNamespaceFilter includeContainingNamespaceFilter,
            Visibility visibility,
            in ProjectFilter projectFilter) : base(projectFilter)
        {
            Options = options;
            Depth = depth;
            IgnoredRootParts = ignoredRootParts;
            IgnoredNamespaceParts = ignoredNamespaceParts;
            IgnoredTypeParts = ignoredTypeParts;
            IgnoredMemberParts = ignoredMemberParts;
            OmitMemberParts = omitMemberParts;
            IncludeContainingNamespaceFilter = includeContainingNamespaceFilter;
            Visibility = visibility;
        }

        public GenerateDocCommandLineOptions Options { get; }

        public DocumentationDepth Depth { get; }

        public RootDocumentationParts IgnoredRootParts { get; }

        public NamespaceDocumentationParts IgnoredNamespaceParts { get; }

        public TypeDocumentationParts IgnoredTypeParts { get; }

        public MemberDocumentationParts IgnoredMemberParts { get; }

        public OmitMemberParts OmitMemberParts { get; }

        public IncludeContainingNamespaceFilter IncludeContainingNamespaceFilter { get; }

        public Visibility Visibility { get; }

        public override async Task<BaseCommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            AssemblyResolver.Register();

            var documentationOptions = new DocumentationOptions(
                ignoredNames: Options.IgnoredNames,
                preferredCultureName: Options.PreferredCulture,
                maxDerivedTypes: Options.MaxDerivedTypes,
                placeSystemNamespaceFirst: !Options.NoPrecedenceForSystem,
                wrapDeclarationBaseTypes: !Options.NoWrapBaseTypes,
                wrapDeclarationConstraints: !Options.NoWrapConstraints,
                markObsolete: !Options.NoMarkObsolete,
                includeMemberInheritedFrom: (OmitMemberParts & OmitMemberParts.InheritedFrom) == 0,
                includeMemberOverrides: (OmitMemberParts & OmitMemberParts.Overrides) == 0,
                includeMemberImplements: (OmitMemberParts & OmitMemberParts.Implements) == 0,
                includeMemberConstantValue: (OmitMemberParts & OmitMemberParts.ConstantValue) == 0,
                includeInheritedInterfaceMembers: Options.IncludeInheritedInterfaceMembers,
                includeAllDerivedTypes: Options.IncludeAllDerivedTypes,
                includeAttributeArguments: !Options.OmitAttributeArguments,
                includeInheritedAttributes: !Options.OmitInheritedAttributes,
                omitIEnumerable: !Options.IncludeIEnumerable,
                depth: Depth,
                inheritanceStyle: Options.InheritanceStyle,
                ignoredRootParts: IgnoredRootParts,
                ignoredNamespaceParts: IgnoredNamespaceParts,
                ignoredTypeParts: IgnoredTypeParts,
                ignoredMemberParts: IgnoredMemberParts,
                includeContainingNamespaceFilter: IncludeContainingNamespaceFilter,
                scrollToContent: Options.ScrollToContent);

            ImmutableArray<Compilation> compilations = await GetCompilationsAsync(projectOrSolution, cancellationToken);

            var documentationModel = new DocumentationModel(compilations, DocumentationFilterOptions.Instance, Options.AdditionalXmlDocumentation);
#if DEBUG
            SourceReferenceProvider sourceReferenceProvider = (Options.SourceReferences.Any())
                ? SourceReferenceProvider.Load(Options.SourceReferences)
                : null;

            var generator = new MarkdownDocumentationGenerator(documentationModel, WellKnownUrlProviders.GitHub, documentationOptions, sourceReferenceProvider);
#else
            var generator = new MarkdownDocumentationGenerator(documentationModel, WellKnownUrlProviders.GitHub, documentationOptions);
#endif
            string directoryPath = Options.Output;

            if (!Options.NoDelete
                && Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.Delete(directoryPath, recursive: true);
                }
                catch (IOException ex)
                {
                    WriteError(ex);
                    return BaseCommandResult.Fail;
                }
            }

            WriteLine($"Generate documentation to '{Options.Output}'", Verbosity.Minimal);

            var success = false;

            foreach (DocumentationGeneratorResult documentationFile in generator.Generate(heading: Options.Heading, cancellationToken))
            {
                string path = Path.Combine(directoryPath, documentationFile.FilePath);

                Directory.CreateDirectory(Path.GetDirectoryName(path));

                WriteLine($"  Save '{path}'", ConsoleColor.DarkGray, Verbosity.Detailed);

                File.WriteAllText(path, documentationFile.Content, _defaultEncoding);

                success = true;
            }

            WriteLine($"Documentation successfully generated to '{Options.Output}'.", Verbosity.Minimal);

            return (success) ? BaseCommandResult.Success : BaseCommandResult.NotSuccess;
        }
    }
}
