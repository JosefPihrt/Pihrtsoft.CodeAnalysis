// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;

namespace Roslynator.CommandLine
{
    [Verb("spellcheck", HelpText = "Searches the specified project or solution for possible misspellings or typos.")]
    public sealed class SpellcheckCommandLineOptions : MSBuildCommandLineOptions
    {
        [Value(
            index: 0,
            HelpText = "Path to one or more project/solution files.",
            MetaValue = "<PROJECT|SOLUTION>")]
        public IEnumerable<string> Paths { get; set; }

        [Option(
            longName: "case-sensitive",
            HelpText = "Specifies case-sensitive matching.")]
        public bool CaseSensitive { get; set; }

        [Option(
            longName: "culture",
            HelpText = "Defines culture that should be used to display diagnostic message.",
            MetaValue = "<CULTURE_ID>")]
        public string Culture { get; set; }

        [Option(
            shortName: OptionShortNames.DryRun,
            longName: "dry-run",
            HelpText = "Display misspellings and typos but do not save changes to a disk.")]
        public bool DryRun { get; set; }

        [Option(
            longName: ParameterNames.IgnoredScope,
            HelpText = "Defines syntax that should not be analyzed. Allowed values are comment, type, member, local, parameter, non-symbol and symbol.",
            MetaValue = "<SCOPE>")]
        public IEnumerable<string> IgnoredScope { get; set; }

        [Option(
            longName: "include-generated-code",
            HelpText = "Indicates whether generated code should be spellchecked.")]
        public bool IncludeGeneratedCode { get; set; }

        [Option(
            longName: "interactive",
            HelpText = "Enable editing of a replacement.")]
        public bool Interactive { get; set; }

        [Option(
            longName: "min-word-length",
            Default = 3,
            HelpText = "Specifies minimal word length to be checked. Default value is 3.")]
        public int MinWordLength { get; set; }

        [Option(
            longName: ParameterNames.Scope,
            HelpText = "Defines syntax that should be analyzed. Allowed values are comment, type, member, local, parameter, non-symbol and symbol.",
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
            HelpText = "Specified path to file and/or directory that contains list of allowed words.",
            MetaValue = "<PATH>")]
        public IEnumerable<string> Words { get; set; } = null!;
    }
}
