// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;
using Roslynator.Documentation;

namespace Roslynator.CommandLine
{
    // OmitAssemblies, PreferredCulture
    [Verb("list-symbols", HelpText = "Lists symbols from the specified project or solution.")]
    public class ListSymbolsCommandLineOptions : MSBuildCommandLineOptions
    {
        [Value(
            index: 0,
            HelpText = "The project or solution file.",
            MetaValue = "<PROJECT|SOLUTION>")]
        public IEnumerable<string> Paths { get; set; }

        [Option(
            longName: ParameterNames.Depth,
            HelpText = "Defines a depth of a list of symbols. Allowed values are member, type or namespace. Default value is member.",
            MetaValue = "<DEPTH>")]
        public string Depth { get; set; }

        [Option(
            longName: "empty-line-between-members",
            HelpText = "Indicates whether an empty line should be added between two member definitions.")]
        public bool EmptyLineBetweenMembers { get; set; }

        [Option(
            longName: ParameterNames.WrapList,
            HelpText = "Specifies syntax lists that should be wrapped. Allowed values are attributes, parameters, base-types and constraints.")]
        public IEnumerable<string> WrapList { get; set; }

        [Option(
            longName: "group-by-assembly",
            HelpText = "Indicates whether symbols should be grouped by assembly.")]
        public bool GroupByAssembly { get; set; }

        [Option(
            longName: "ignored-attributes",
            HelpText = "Defines a list of attributes that should be ignored.",
            MetaValue = "<FULLY_QUALIFIED_METADATA_NAME>")]
        public IEnumerable<string> IgnoredAttributes { get; set; }

        [Option(
            longName: ParameterNames.IgnoredParts,
            HelpText = "Defines parts of a symbol definition that should be excluded. Allowed values are assemblies, containing-namespace, containing-namespace-in-type-hierarchy, attributes, assembly-attributes, attribute-arguments, accessibility, modifiers, parameter-name, parameter-default-value, base-type, base-interfaces, constraints, trailing-semicolon, trailing-comma.",
            MetaValue = "<IGNORED_PARTS>")]
        public IEnumerable<string> IgnoredParts { get; set; }

        [Option(
            longName: "ignored-symbols",
            HelpText = "Defines a list of symbols that should be ignored. Namespace of types can be specified.",
            MetaValue = "<FULLY_QUALIFIED_METADATA_NAME>")]
        public IEnumerable<string> IgnoredSymbols { get; set; }

        [Option(
            longName: "documentation",
            HelpText = "Indicates whether a documentation should be included.")]
        public bool Documentation { get; set; }

        [Option(
            longName: "indent-chars",
            Default = DefinitionListFormat.DefaultValues.IndentChars,
            HelpText = "Defines characters that should be used for indentation. Default value is two spaces.",
            MetaValue = "<INDENT_CHARS>")]
        public string IndentChars { get; set; }

        [Option(
            longName: "hierarchy-root",
            HelpText = "Defines symbol that should be used as a root of a type hierarchy.",
            MetaValue = "<FULLY_QUALIFIED_METADATA_NAME>")]
        public string HierarchyRoot { get; set; }

        [Option(
            longName: ParameterNames.Layout,
            HelpText = "Defines layout of a list of symbol definitions. Allowed values are namespace-list, namespace-hierarchy or type-hierarchy. Default value is namespace-list.")]
        public string Layout { get; set; }

        [Option(
            shortName: OptionShortNames.Output,
            longName: "output",
            HelpText = "Defines path to file(s) that will store a list of symbol definitions.",
            MetaValue = "<OUTPUT_FILE>")]
        public IEnumerable<string> Output { get; set; }

        [Option(
            longName: "external-assemblies",
            HelpText = "Defines file name/path to external assemblies that should be included.",
            MetaValue = "<ASSEMBLY_FILE>")]
        public IEnumerable<string> ExternalAssemblies { get; set; }

#if DEBUG
        [Option(longName: "source-references")]
        public string SourceReferences { get; set; }
#endif

        [Option(
            longName: ParameterNames.Visibility,
            Default = new string[] { nameof(Roslynator.Visibility.Public) },
            HelpText = "Defines one or more visibility of a type or a member. Allowed values are public, internal or private.",
            MetaValue = "<VISIBILITY>")]
        public IEnumerable<string> Visibility { get; set; }

        //[Option(longName: "include-ienumerable",
        //    HelpText = "Indicates whether interface System.Collections.IEnumerable should be included in a documentation if a type also implements interface System.Collections.Generic.IEnumerable<T>.")]
        //public bool IncludeIEnumerable { get; set; }

        //[Option(longName: "no-precedence-for-system",
        //    HelpText = "Indicates whether symbols contained in 'System' namespace should be ordered as any other symbols and not before other symbols.")]
        //public bool NoPrecedenceForSystem { get; set; }
    }
}
