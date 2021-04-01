// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;

namespace Roslynator.CommandLine
{
    [Verb("spellcheck", HelpText = "Searches the specified project or solution for possible misspellings or typos.")]
    public class SpellcheckCommandLineOptions : MSBuildCommandLineOptions
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
            longName: "include-generated-code",
            HelpText = "Indicates whether generated code should be formatted.")]
        public bool IncludeGeneratedCode { get; set; }

        [Option(
            longName: "interactive",
            HelpText = "")]
        public bool Interactive { get; set; }

        [Option(
            longName: "new-words",
            HelpText = "",
            MetaValue = "<PATH>")]
        public string NewWords { get; set; }

        [Option(
            longName: "new-fixes",
            HelpText = "",
            MetaValue = "<PATH>")]
        public string NewFixes { get; set; }

        [Option(
            longName: "words",
            HelpText = "",
            MetaValue = "<PATH>")]
        public IEnumerable<string> Words { get; set; } = null!;
    }
}
