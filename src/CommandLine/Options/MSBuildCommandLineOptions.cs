// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using CommandLine;

namespace Roslynator.CommandLine
{
    // Files, IgnoredFiles
    public abstract class MSBuildCommandLineOptions : AbstractCommandLineOptions
    {
        [Value(
            index: 0,
            HelpText = "The project or solution file.",
            MetaValue = "<PROJECT|SOLUTION>")]
        public string Path { get; set; }

        [Option(
            longName: ParameterNames.IgnoredProjects,
            HelpText = "Defines projects that should not be analyzed.",
            MetaValue = "<PROJECT_NAME>")]
        public IEnumerable<string> IgnoredProjects { get; set; }

        [Option(
            longName: "language",
            HelpText = "Defines project language. Allowed values are cs [csharp] or vb [visual basic]",
            MetaValue = "<LANGUAGE>")]
        public string Language { get; set; }

        [Option(
            shortName: OptionShortNames.MSBuildPath,
            longName: ParameterNames.MSBuildPath,
            HelpText = "Defines a path to MSBuild. This option must be specified if there are multiple locations of MSBuild (usually multiple installations of Visual Studio).",
            MetaValue = "<MSBUILD_PATH>")]
        public string MSBuildPath { get; set; }

        [Option(
            longName: ParameterNames.Projects,
            HelpText = "Defines projects that should be analyzed.",
            MetaValue = "<PROJECT_NAME>")]
        public IEnumerable<string> Projects { get; set; }

        [Option(
            shortName: OptionShortNames.Properties,
            longName: "properties",
            HelpText = "Defines one or more MSBuild properties.",
            MetaValue = "<NAME=VALUE>")]
        public IEnumerable<string> Properties { get; set; }

        internal bool TryGetLanguage(out string value)
        {
            if (Language != null)
                return ParseHelpers.TryParseLanguage(Language, out value);

            value = null;
            return true;
        }

        internal bool TryGetProjectFilter(out ProjectFilter projectFilter)
        {
            projectFilter = default;

            string language = null;

            if (Language != null
                && !ParseHelpers.TryParseLanguage(Language, out language))
            {
                return false;
            }

            if (Projects?.Any() == true
                && IgnoredProjects?.Any() == true)
            {
                Logger.WriteLine($"Cannot specify both '{ParameterNames.Projects}' and '{ParameterNames.IgnoredProjects}'.", Roslynator.Verbosity.Quiet);
                return false;
            }

            projectFilter = new ProjectFilter(Projects, IgnoredProjects, language);
            return true;
        }
    }
}
