// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;

namespace Roslynator.CommandLine
{
    [Verb("spellcheck", HelpText = "Searches the specified project or solution for possible misspellings or typos.")]
    public sealed class SpellcheckCommandLineOptions : MSBuildCommandLineOptions
    {
        [Option(
            longName: "culture",
            HelpText = "Defines culture that should be used to display diagnostic message.",
            MetaValue = "<CULTURE_ID>")]
        public string Culture { get; set; }

        [Option(
            shortName: OptionShortNames.DryRun,
            longName: "dry-run",
            HelpText = "")]
        public bool DryRun { get; set; }

        [Option(
            longName: "fixes",
            HelpText = "",
            MetaValue = "<PATH>")]
        public IEnumerable<string> Fixes { get; set; } = null!;

        [Option(
            longName: ParameterNames.IgnoredScope,
            HelpText = "",
            MetaValue = "<SCOPE>")]
        public IEnumerable<string> IgnoredScope { get; set; }

        [Option(
            longName: "include-generated-code",
            HelpText = "Indicates whether generated code should be spellchecked.")]
        public bool IncludeGeneratedCode { get; set; }

        [Option(
            longName: "ignore-case",
            HelpText = "")]
        public bool IgnoreCase { get; set; }

        [Option(
            longName: "interactive",
            HelpText = "")]
        public bool Interactive { get; set; }

        [Option(
            longName: "min-word-length",
            Default = 3,
            HelpText = "")]
        public int MinWordLength { get; set; }

        [Option(
            longName: ParameterNames.Scope,
            HelpText = "",
            MetaValue = "<SCOPE>")]
        public IEnumerable<string> Scope { get; set; }

        [Option(
            longName: ParameterNames.Visibility,
            Default = nameof(Roslynator.Visibility.Public),
            HelpText = "Defines a  maximal visibility of a symbol to be fixable. Allowed values are public, internal or private. Default value is public.",
            MetaValue = "<VISIBILITY>")]
        public string Visibility { get; set; }

        [Option(
            longName: "words",
            Required = true,
            HelpText = "",
            MetaValue = "<PATH>")]
        public IEnumerable<string> Words { get; set; } = null!;
    }
}
