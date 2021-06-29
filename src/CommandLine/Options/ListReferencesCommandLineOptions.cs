// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;

namespace Roslynator.CommandLine
{
#if DEBUG
    [Verb("list-references", HelpText = "Lists assembly references from the specified project or solution.")]
#endif
    public class ListReferencesCommandLineOptions : MSBuildCommandLineOptions
    {
        [Value(
            index: 0,
            HelpText = "The project or solution file.",
            MetaValue = "<PROJECT|SOLUTION>")]
        public IEnumerable<string> Paths { get; set; }

        [Option(
            longName: "display",
            HelpText = "Defines how the assembly is displayed. Allowed values are path (default), file-name, file-name-without-extension or assembly-name.")]
        public string Display { get; set; }

        [Option(
            longName: ParameterNames.Type,
            HelpText = "Defines a type of a reference. Allowed values are dll and project.")]
        public IEnumerable<string> Type { get; set; }
    }
}
