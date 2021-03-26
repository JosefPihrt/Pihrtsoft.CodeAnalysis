// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;

namespace Roslynator.CommandLine
{
    [Verb("migrate", HelpText = "Migrates analyzers to a new version.")]
    internal sealed class MigrateCommandLineOptions : AbstractCommandLineOptions
    {
        [Value(
            index: 0,
            HelpText = "A path to a directory, project file or a ruleset file.",
            MetaName = "<PATH>")]
        public IEnumerable<string> Path { get; set; }

        [Option(
            shortName: OptionShortNames.DryRun,
            longName: "dry-run",
            HelpText = "Migrate analyzers to a new version but do not save changes to a disk.")]
        public bool DryRun { get; set; }

        [Option(
            longName: "identifier",
            Required = true,
            HelpText = "Identifier of a package to be migrated.",
            MetaValue = "<IDENTIFIER>")]
        public string Identifier { get; set; }

        [Option(
            longName: ParameterNames.TargetVersion,
            Required = true,
            HelpText = "A package version to migrate to.",
            MetaValue = "<VERSION>")]
        public string Version { get; set; }
    }
}
