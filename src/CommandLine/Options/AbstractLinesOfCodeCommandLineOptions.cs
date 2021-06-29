// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;

namespace Roslynator.CommandLine
{
    public class AbstractLinesOfCodeCommandLineOptions : MSBuildCommandLineOptions
    {
        [Value(
            index: 0,
            HelpText = "Path to one or more project/solution files.",
            MetaValue = "<PROJECT|SOLUTION>")]
        public IEnumerable<string> Paths { get; set; }

        [Option(
            shortName: OptionShortNames.IncludeGeneratedCode,
            longName: "include-generated-code",
            HelpText = "Indicates whether generated code should be included.")]
        public bool IncludeGeneratedCode { get; set; }
    }
}
