// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal abstract class MSBuildWorkspaceCommand<TCommandResult> where TCommandResult : BaseCommandResult
    {
        protected MSBuildWorkspaceCommand(in ProjectFilter projectFilter)
        {
            ProjectFilter = projectFilter;
        }

        public string Language
        {
            get { return ProjectFilter.Language; }
        }

        public ProjectFilter ProjectFilter { get; }

        public abstract Task<TCommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default);

        public async Task<CommandStatus> ExecuteAsync(IEnumerable<string> paths, string msbuildPath = null, IEnumerable<string> properties = null)
        {
            if (paths == null)
                throw new ArgumentNullException(nameof(paths));

            if (!paths.Any())
                throw new ArgumentException("", nameof(paths));

            MSBuildWorkspace workspace = null;

            try
            {
                workspace = CreateMSBuildWorkspace(msbuildPath, properties);

                if (workspace == null)
                    return CommandStatus.Fail;

                workspace.WorkspaceFailed += (sender, args) => WorkspaceFailed(sender, args);

                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                CancellationToken cancellationToken = cts.Token;

                try
                {
                    var status = CommandStatus.NotSuccess;
                    var results = new List<TCommandResult>();

                    foreach (string path in paths)
                    {
                        TCommandResult result = await ExecuteAsync(path, workspace, cancellationToken);

                        results.Add(result);

                        if (result.Status != CommandStatus.NotSuccess)
                            status = result.Status;

                        if (status == CommandStatus.Fail
                            || status == CommandStatus.Canceled)
                        {
                            break;
                        }

                        workspace.CloseSolution();
                    }

                    if (results.Count > 1)
                        ProcessResults(results);

                    return status;
                }
                catch (OperationCanceledException ex)
                {
                    OperationCanceled(ex);
                }
                catch (AggregateException ex)
                {
                    OperationCanceledException operationCanceledException = ex.GetOperationCanceledException();

                    if (operationCanceledException != null)
                    {
                        OperationCanceled(operationCanceledException);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            finally
            {
                workspace?.Dispose();
            }

            return CommandStatus.Canceled;
        }

        private async Task<TCommandResult> ExecuteAsync(string path, MSBuildWorkspace workspace, CancellationToken cancellationToken)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Project or solution file not found: {path}");

            TCommandResult result = await ExecuteAsync(path, workspace, ConsoleProgressReporter.Default, cancellationToken);

            if (result != null)
                return result;

            ProjectOrSolution projectOrSolution = await OpenProjectOrSolutionAsync(path, workspace, ConsoleProgressReporter.Default, cancellationToken);

            Solution solution = projectOrSolution.AsSolution();

            if (solution != null
                && !VerifyProjectNames(solution))
            {
                return null;
            }

            return await ExecuteAsync(projectOrSolution, cancellationToken);
        }

        private bool VerifyProjectNames(Solution solution)
        {
            List<ProjectName> projectNames = solution.Projects.Select(f => ProjectName.Create(f.Name)).ToList();

            if (ShouldWrite(Verbosity.Detailed))
            {
                WriteLine("List of projects:", Verbosity.Detailed);

                foreach (IGrouping<string, ProjectName> grouping in projectNames
                    .OrderBy(f => f.NameWithoutMoniker)
                    .GroupBy(f => f.NameWithoutMoniker))
                {
                    WriteLine($"  {grouping.Key}", Verbosity.Detailed);

                    foreach (string moniker in grouping
                        .Select(f => f.Moniker)
                        .Where(f => f != null)
                        .OrderBy(f => f))
                    {
                        WriteLine($"    {moniker}", Verbosity.Detailed);
                    }
                }
            }

            ImmutableHashSet<ProjectName> values = (ProjectFilter.Names.Count > 0)
                ? ProjectFilter.Names
                : ProjectFilter.IgnoredNames;

            foreach (ProjectName value in values)
            {
                if (!projectNames.Any(f => string.Equals(f.Name, value.Name, StringComparison.Ordinal))
                    && !projectNames.Any(f => string.Equals(f.NameWithoutMoniker, value.NameWithoutMoniker, StringComparison.Ordinal)))
                {
                    WriteLine($"Project '{value}' does not exist.", Verbosity.Quiet);
                    return false;
                }
            }

            return true;
        }

        protected virtual void ProcessResults(IEnumerable<TCommandResult> results)
        {
        }

        protected virtual void OperationCanceled(OperationCanceledException ex)
        {
            WriteLine("Operation was canceled.", Verbosity.Quiet);
        }

        protected virtual void WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            WriteLine($"  {e.Diagnostic.Message}", e.Diagnostic.Kind.GetColor(), Verbosity.Detailed);
        }

        protected virtual Task<TCommandResult> ExecuteAsync(
            string path,
            MSBuildWorkspace workspace,
            IProgress<ProjectLoadProgress> progress = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(TCommandResult));
        }

        private async Task<ProjectOrSolution> OpenProjectOrSolutionAsync(
            string path,
            MSBuildWorkspace workspace,
            IProgress<ProjectLoadProgress> progress = null,
            CancellationToken cancellationToken = default)
        {
            bool isSolution = string.Equals(Path.GetExtension(path), ".sln", StringComparison.OrdinalIgnoreCase);

            WriteLine($"Load {((isSolution) ? "solution" : "project")} '{path}'", Verbosity.Minimal);

            ProjectOrSolution projectOrSolution;

            if (isSolution)
            {
                projectOrSolution = await workspace.OpenSolutionAsync(path, progress, cancellationToken);
            }
            else
            {
                projectOrSolution = await workspace.OpenProjectAsync(path, progress, cancellationToken);
            }

            WriteLine($"Done loading {((projectOrSolution.IsSolution) ? "solution" : "project")} '{projectOrSolution.FilePath}'", Verbosity.Minimal);

            return projectOrSolution;
        }

        private static MSBuildWorkspace CreateMSBuildWorkspace(string msbuildPath, IEnumerable<string> rawProperties)
        {
            if (msbuildPath != null)
            {
                MSBuildLocator.RegisterMSBuildPath(msbuildPath);
            }
            else if (TryGetSingleInstance(out VisualStudioInstance instance))
            {
                MSBuildLocator.RegisterInstance(instance);
                msbuildPath = instance.MSBuildPath;
            }
            else
            {
                return null;
            }

            WriteLine($"MSBuild location is '{msbuildPath}'", Verbosity.Diagnostic);

            if (!ParseHelpers.TryParseMSBuildProperties(rawProperties, out Dictionary<string, string> properties))
                return null;

            if (properties == null)
                properties = new Dictionary<string, string>();

            // https://github.com/Microsoft/MSBuildLocator/issues/16
            if (!properties.ContainsKey("AlwaysCompileMarkupFilesInSeparateDomain"))
                properties["AlwaysCompileMarkupFilesInSeparateDomain"] = bool.FalseString;

            return MSBuildWorkspace.Create(properties);
        }

        private static bool TryGetSingleInstance(out VisualStudioInstance instance)
        {
            using (IEnumerator<VisualStudioInstance> en = MSBuildLocator.QueryVisualStudioInstances()
                .Distinct(VisualStudioInstanceComparer.MSBuildPath)
                .GetEnumerator())
            {
                if (!en.MoveNext())
                {
                    WriteLine($"MSBuild location not found. Use option '--{ParameterNames.MSBuildPath}' to specify MSBuild location", Verbosity.Quiet);
                    instance = null;
                    return false;
                }

                VisualStudioInstance firstInstance = en.Current;

                if (en.MoveNext())
                {
                    WriteLine("Multiple MSBuild locations found:", Verbosity.Quiet);

                    WriteLine($"  {firstInstance.MSBuildPath}", Verbosity.Quiet);

                    do
                    {
                        WriteLine($"  {en.Current.MSBuildPath}", Verbosity.Quiet);

                    } while (en.MoveNext());

                    WriteLine($"Use option '--{ParameterNames.MSBuildPath}' to specify MSBuild location", Verbosity.Quiet);
                    instance = null;
                    return false;
                }

                instance = firstInstance;
                return true;
            }
        }

        private protected IEnumerable<Project> FilterProjects(
            ProjectOrSolution projectOrSolution,
            Func<Solution, ImmutableArray<ProjectId>> getProjects = null)
        {
            if (projectOrSolution.IsProject)
            {
                yield return projectOrSolution.AsProject();
            }
            else if (projectOrSolution.IsSolution)
            {
                foreach (Project project in FilterProjects(projectOrSolution.AsSolution(), getProjects))
                    yield return project;
            }
        }

        private protected IEnumerable<Project> FilterProjects(
            Solution solution,
            Func<Solution, ImmutableArray<ProjectId>> getProjects = null)
        {
            Workspace workspace = solution.Workspace;

            foreach (ProjectId projectId in (getProjects != null) ? getProjects(solution) : solution.ProjectIds)
            {
                Project project = workspace.CurrentSolution.GetProject(projectId);

                if (ProjectFilter.IsMatch(project))
                {
                    yield return project;
                }
                else
                {
                    WriteLine($"  Skip '{project.Name}'", ConsoleColor.DarkGray, Verbosity.Normal);
                }
            }
        }

        private protected async Task<ImmutableArray<Compilation>> GetCompilationsAsync(
            ProjectOrSolution projectOrSolution,
            CancellationToken cancellationToken)
        {
            if (projectOrSolution.IsProject)
            {
                Project project = projectOrSolution.AsProject();

                WriteLine($"Compile '{project.Name}'", Verbosity.Minimal);

                Compilation compilation = await project.GetCompilationAsync(cancellationToken);

                return ImmutableArray.Create(compilation);
            }
            else
            {
                ImmutableArray<Compilation>.Builder compilations = ImmutableArray.CreateBuilder<Compilation>();

                Solution solution = projectOrSolution.AsSolution();

                WriteLine($"Compile solution '{solution.FilePath}'", Verbosity.Minimal);

                Stopwatch stopwatch = Stopwatch.StartNew();

                foreach (Project project in FilterProjects(
                    solution,
                    s => s
                        .GetProjectDependencyGraph()
                        .GetTopologicallySortedProjects(cancellationToken)
                        .ToImmutableArray()))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    WriteLine($"  Compile '{project.Name}'", Verbosity.Minimal);

                    Compilation compilation = await project.GetCompilationAsync(cancellationToken);

                    compilations.Add(compilation);
                }

                stopwatch.Stop();

                WriteLine($"Done compiling solution '{solution.FilePath}' in {stopwatch.Elapsed:mm\\:ss\\.ff}", Verbosity.Minimal);

                return compilations.ToImmutableArray();
            }
        }

        protected class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public static ConsoleProgressReporter Default { get; } = new ConsoleProgressReporter();

            public Dictionary<string, List<string>> Projects { get; }

            public ConsoleProgressReporter(bool shouldSaveProgress = false)
            {
                if (shouldSaveProgress)
                    Projects = new Dictionary<string, List<string>>();
            }

            public void Report(ProjectLoadProgress value)
            {
                string text = Path.GetFileName(value.FilePath);

                ProjectLoadOperation operation = value.Operation;

                if (operation == ProjectLoadOperation.Resolve)
                {
                    string targetFramework = value.TargetFramework;

                    if (targetFramework != null)
                        text += $" ({targetFramework})";

                    if (Projects != null)
                    {
                        if (!Projects.TryGetValue(value.FilePath, out List<string> targetFrameworks))
                        {
                            if (targetFramework != null)
                                targetFrameworks = new List<string>();

                            Projects[value.FilePath] = targetFrameworks;
                        }

                        if (targetFramework != null)
                            targetFrameworks.Add(targetFramework);
                    }
                }

                text = $"  {operation,-9} {value.ElapsedTime:mm\\:ss\\.ff}  {text}";

                if (operation == ProjectLoadOperation.Resolve)
                {
                    WriteLine(text, Verbosity.Detailed);
                }
                else
                {
                    WriteLine(text, ConsoleColor.DarkGray, Verbosity.Diagnostic);
                }
            }
        }
    }
}
