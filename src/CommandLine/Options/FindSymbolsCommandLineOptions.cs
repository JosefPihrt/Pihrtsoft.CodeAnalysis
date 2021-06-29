// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;

namespace Roslynator.CommandLine
{
#if DEBUG
    [Verb("find-symbols", HelpText = "Finds symbols in the specified project or solution.")]
#endif
    public class FindSymbolsCommandLineOptions : MSBuildCommandLineOptions
    {
        [Value(
            index: 0,
            HelpText = "Path to one or more project/solution files.",
            MetaValue = "<PROJECT|SOLUTION>")]
        public IEnumerable<string> Paths { get; set; }

        [Option(longName: "ignored-symbol-ids")]
        public IEnumerable<string> IgnoredSymbolIds { get; set; }

        [Option(longName: "ignore-generated-code")]
        public bool IgnoreGeneratedCode { get; set; }

        [Option(longName: ParameterNames.SymbolGroups)]
        public IEnumerable<string> SymbolGroups { get; set; }

        [Option(longName: "unused-only")]
        public bool UnusedOnly { get; set; }

        [Option(longName: ParameterNames.Visibility)]
        public IEnumerable<string> Visibility { get; set; }

        [Option(longName: "with-attributes")]
        public IEnumerable<string> WithAttributes { get; set; }

        [Option(longName: "without-attributes")]
        public IEnumerable<string> WithoutAttributes { get; set; }

        [Option(longName: ParameterNames.WithFlags)]
        public IEnumerable<string> WithFlags { get; set; }

        [Option(longName: ParameterNames.WithoutFlags)]
        public IEnumerable<string> WithoutFlags { get; set; }
    }
}
