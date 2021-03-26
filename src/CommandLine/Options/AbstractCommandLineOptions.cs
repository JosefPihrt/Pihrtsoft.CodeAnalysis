// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CommandLine;

namespace Roslynator.CommandLine
{
    public abstract class AbstractCommandLineOptions
    {
        [Option(longName: "file-log")]
        public string FileLog { get; set; }

        [Option(
            longName: "file-log-verbosity",
            HelpText = "Defines the amount of information to display in the file log. Allowed values are q [quiet], m [minimal], n [normal], d [detailed] and diag [diagnostic].",
            MetaValue = "<LEVEL>")]
        public string FileLogVerbosity { get; set; }

        [Option(
            shortName: OptionShortNames.Verbosity,
            longName: "verbosity",
            HelpText = "Defines the amount of information to display in the log. Allowed values are q [quiet], m [minimal], n [normal], d [detailed] and diag [diagnostic].",
            MetaValue = "<LEVEL>")]
        public string Verbosity { get; set; }
    }
}
