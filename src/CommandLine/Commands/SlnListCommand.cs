// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal class SlnListCommand : MSBuildWorkspaceCommand<CommandResult>
    {
        public SlnListCommand(SlnListCommandLineOptions options, in ProjectFilter projectFilter) : base(projectFilter)
        {
            Options = options;
        }

        public SlnListCommandLineOptions Options { get; }

        public override Task<CommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CommandResults.Success);
        }

        protected override async Task<CommandResult> ExecuteAsync(
            string path,
            MSBuildWorkspace workspace,
            IProgress<ProjectLoadProgress> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (!string.Equals(Path.GetExtension(path), ".sln", StringComparison.OrdinalIgnoreCase))
            {
                WriteLine($"File is not a solution file: '{path}'.", Verbosity.Quiet);
                return CommandResults.Fail;
            }

            workspace.LoadMetadataForReferencedProjects = true;

            var consoleProgress = new ConsoleProgressReporter(shouldSaveProgress: true);

            var loader = new MSBuildProjectLoader(workspace);

            WriteLine($"Load solution '{path}'", Verbosity.Minimal);

            SolutionInfo solutionInfo = await loader.LoadSolutionInfoAsync(path, consoleProgress, cancellationToken: cancellationToken);

            string solutionDirectory = Path.GetDirectoryName(solutionInfo.FilePath);

            Dictionary<string, ImmutableArray<ProjectInfo>> projectInfos = solutionInfo.Projects
                .GroupBy(f => f.FilePath)
                .ToDictionary(f => f.Key, f => f.ToImmutableArray());

            Dictionary<string, List<string>> projects = consoleProgress.Projects;

            int nameMaxLength = projects.Max(f => Path.GetFileNameWithoutExtension(f.Key).Length);

            int targetFrameworksMaxLength = projects.Max(f =>
            {
                List<string> frameworks = f.Value;

                return (frameworks != null) ? $"({string.Join(", ", frameworks)})".Length : 0;
            });

            bool anyHasTargetFrameworks = projects.Any(f => f.Value != null);

            WriteLine();
            WriteLine($"{projects.Count} {((projects.Count == 1) ? "project" : "projects")} found in solution '{Path.GetFileNameWithoutExtension(solutionInfo.FilePath)}' [{solutionInfo.FilePath}]", ConsoleColor.Green, Verbosity.Minimal);

            foreach (KeyValuePair<string, List<string>> kvp in projects
                .OrderBy(f => Path.GetFileName(f.Key)))
            {
                string projectPath = kvp.Key;
                List<string> targetFrameworks = kvp.Value;

                ProjectInfo projectInfo = projectInfos[projectPath][0];

                string projectName = Path.GetFileNameWithoutExtension(projectPath);

                Write($"  {projectName.PadRight(nameMaxLength)}  {projectInfo.Language}", Verbosity.Normal);

                if (anyHasTargetFrameworks)
                    Write("  ", Verbosity.Normal);

                if (targetFrameworks != null)
                {
                    string targetFrameworksText = $"({string.Join(", ", targetFrameworks.OrderBy(f => f))})";
                    Write(targetFrameworksText.PadRight(targetFrameworksMaxLength), Verbosity.Normal);
                }
                else
                {
                    Write(new string(' ', targetFrameworksMaxLength), Verbosity.Normal);
                }

                WriteLine($"  [{PathUtilities.TrimStart(projectPath, solutionDirectory)}]", Verbosity.Normal);
            }

            WriteLine();

            return (projects.Count > 0) ? CommandResults.Success : CommandResults.NotSuccess;
        }
    }
}
