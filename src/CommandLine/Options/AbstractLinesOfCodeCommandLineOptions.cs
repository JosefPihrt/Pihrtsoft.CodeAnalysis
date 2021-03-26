// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CommandLine;

namespace Roslynator.CommandLine
{
    public class AbstractLinesOfCodeCommandLineOptions : MSBuildCommandLineOptions
    {
        [Option(
            shortName: OptionShortNames.IncludeGeneratedCode,
            longName: "include-generated-code",
            HelpText = "Indicates whether generated code should be included.")]
        public bool IncludeGeneratedCode { get; set; }
    }
}
