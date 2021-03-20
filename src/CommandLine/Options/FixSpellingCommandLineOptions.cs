// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CommandLine;

namespace Roslynator.CommandLine
{
    [Verb("fix-spelling", HelpText = "Fixes typos and misspelled words in the specified project or solution.")]
    public class FixSpellingCommandLineOptions : MSBuildCommandLineOptions
    {
        [Option(
            longName: "culture",
            HelpText = "Defines culture that should be used to display diagnostic message.",
            MetaValue = "<CULTURE_ID>")]
        public string Culture { get; set; }

        [Option(
            longName: "include-generated-code",
            HelpText = "Indicates whether generated code should be formatted.")]
        public bool IncludeGeneratedCode { get; set; }

        [Option(
            longName: "interactive",
            HelpText = "")]
        public bool Interactive { get; set; }
    }
}
