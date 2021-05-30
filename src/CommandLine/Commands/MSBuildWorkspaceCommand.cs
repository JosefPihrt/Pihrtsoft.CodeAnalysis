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
    internal abstract class MSBuildWorkspaceCommand
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

        public abstract Task<CommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default);

        public async Task<CommandResult> ExecuteAsync(string path, string msbuildPath = null, IEnumerable<string> properties = null)
        {
            MSBuildWorkspace workspace = null;

            try
            {
                workspace = CreateMSBuildWorkspace(msbuildPath, properties);

                if (workspace == null)
                    return CommandResult.Fail;

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
                    if (string.IsNullOrEmpty(path))
                    {
                        path = FindProjectOrSolutionFile(Environment.CurrentDirectory);
                    }
                    else
                    {
                        if (!Path.IsPathRooted(path))
                            path = Path.GetFullPath(path);

                        if (!File.Exists(path))
                            throw new FileNotFoundException($"Project or solution file not found: {path}");
                    }

                    CommandResult? result = await ExecuteAsync(path, workspace, ConsoleProgressReporter.Default, cancellationToken);

                    if (result != null)
                        return result.Value;

                    ProjectOrSolution projectOrSolution = await OpenProjectOrSolutionAsync(path, workspace, ConsoleProgressReporter.Default, cancellationToken);

                    if (!projectOrSolution.IsDefault)
                    {
                        Solution solution = projectOrSolution.AsSolution();

                        if (solution != null)
                        {
                            foreach (string name in ProjectFilter.Names)
                            {
                                if (!solution.ContainsProject(name))
                                {
                                    WriteLine($"Project '{name}' does not exist.", Verbosity.Quiet);
                                    return CommandResult.Fail;
                                }
                            }

                            foreach (string name in ProjectFilter.IgnoredNames)
                            {
                                if (!solution.ContainsProject(name))
                                {
                                    WriteLine($"Project '{name}' does not exist.", Verbosity.Quiet);
                                    return CommandResult.Fail;
                                }
                            }
                        }

                        return await ExecuteAsync(projectOrSolution, cancellationToken);
                    }
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

            return CommandResult.Canceled;
        }

        protected virtual void OperationCanceled(OperationCanceledException ex)
        {
            WriteLine("Operation was canceled.", Verbosity.Quiet);
        }

        protected virtual void WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            WriteLine($"  {e.Diagnostic.Message}", e.Diagnostic.Kind.GetColor(), Verbosity.Detailed);
        }

        protected virtual Task<CommandResult?> ExecuteAsync(
            string path,
            MSBuildWorkspace workspace,
            IProgress<ProjectLoadProgress> progress = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(CommandResult?));
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

        public static string FindProjectOrSolutionFile(string directoryPath)
        {
            string solutionPath = FindFile(
                Directory.EnumerateFiles(directoryPath, "*.sln", SearchOption.TopDirectoryOnly),
                $"Multiple MSBuild solution files found in '{directoryPath}'");

            string projectPath = FindFile(
                Directory.EnumerateFiles(directoryPath, "*.*proj", SearchOption.TopDirectoryOnly)
                    .Where(f => !string.Equals(".xproj", Path.GetExtension(f), StringComparison.OrdinalIgnoreCase)),
                $"Multiple MSBuild projects files found in '{directoryPath}'");

            if (solutionPath != null
                && projectPath != null)
            {
                throw new FileNotFoundException($"Both MSBuild project file and solution file found in '{directoryPath}'");
            }

            if (solutionPath == null
                && projectPath == null)
            {
                throw new FileNotFoundException($"Could not find MSBuild project or solution file in '{directoryPath}'");
            }

            return solutionPath ?? projectPath;

            static string FindFile(IEnumerable<string> files, string errorMessage)
            {
                using (IEnumerator<string> en = files.GetEnumerator())
                {
                    if (en.MoveNext())
                    {
                        string file = en.Current;

                        if (en.MoveNext())
                            throw new FileNotFoundException(errorMessage);

                        return file;
                    }
                    else
                    {
                        return null;
                    }
                }
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
