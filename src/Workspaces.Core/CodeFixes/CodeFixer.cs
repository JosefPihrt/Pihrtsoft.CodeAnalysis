// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.Formatting;
using Roslynator.Host.Mef;
using static Roslynator.Logger;

namespace Roslynator.CodeFixes
{
    internal class CodeFixer
    {
        private readonly AnalyzerAssemblyList _analyzerAssemblies = new AnalyzerAssemblyList();

        private readonly AnalyzerAssemblyList _analyzerReferences = new AnalyzerAssemblyList();

        public CodeFixer(
            Solution solution,
            IEnumerable<AnalyzerAssembly> analyzerAssemblies = null,
            IFormatProvider formatProvider = null,
            CodeFixerOptions options = null)
        {
            Workspace = solution.Workspace;

            if (analyzerAssemblies != null)
                _analyzerAssemblies.AddRange(analyzerAssemblies);

            FormatProvider = formatProvider;
            Options = options ?? CodeFixerOptions.Default;
        }

        public Workspace Workspace { get; }

        public CodeFixerOptions Options { get; }

        public IFormatProvider FormatProvider { get; }

        private Solution CurrentSolution => Workspace.CurrentSolution;

        public async Task<ImmutableArray<ProjectFixResult>> FixSolutionAsync(Func<Project, bool> predicate, CancellationToken cancellationToken = default)
        {
            ImmutableArray<ProjectId> projects = CurrentSolution
                .GetProjectDependencyGraph()
                .GetTopologicallySortedProjects(cancellationToken)
                .ToImmutableArray();

            var results = new List<ProjectFixResult>();

            Stopwatch stopwatch = Stopwatch.StartNew();

            TimeSpan lastElapsed = TimeSpan.Zero;

            for (int i = 0; i < projects.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Project project = CurrentSolution.GetProject(projects[i]);

                if (predicate == null || predicate(project))
                {
                    WriteLine($"Fix '{project.Name}' {$"{i + 1}/{projects.Length}"}", ConsoleColor.Cyan, Verbosity.Minimal);

                    ProjectFixResult result = await FixProjectAsync(project, cancellationToken).ConfigureAwait(false);

                    results.Add(result);

                    if (result.Kind == ProjectFixKind.CompilerError)
                        break;
                }
                else
                {
                    WriteLine($"Skip '{project.Name}' {$"{i + 1}/{projects.Length}"}", ConsoleColor.DarkGray, Verbosity.Minimal);

                    results.Add(ProjectFixResult.Skipped);
                }

                TimeSpan elapsed = stopwatch.Elapsed;

                WriteLine($"Done fixing '{project.Name}' in {elapsed - lastElapsed:mm\\:ss\\.ff}", Verbosity.Normal);

                lastElapsed = elapsed;
            }

            stopwatch.Stop();

            WriteLine($"Done fixing solution '{CurrentSolution.FilePath}' in {stopwatch.Elapsed:mm\\:ss\\.ff}", Verbosity.Minimal);

            LogHelpers.WriteProjectFixResults(results, Options, FormatProvider);

            return results.ToImmutableArray();
        }

        public async Task<ProjectFixResult> FixProjectAsync(Project project, CancellationToken cancellationToken = default)
        {
            (ImmutableArray<DiagnosticAnalyzer> analyzers, ImmutableArray<CodeFixProvider> fixers) = CodeAnalysisHelpers.GetAnalyzersAndFixers(
                project: project,
                analyzerAssemblies: _analyzerAssemblies,
                analyzerReferences: _analyzerReferences,
                options: Options);

            ProjectFixResult fixResult = await FixProjectAsync(project, analyzers, fixers, cancellationToken).ConfigureAwait(false);

            Compilation compilation = await CurrentSolution.GetProject(project.Id).GetCompilationAsync(cancellationToken).ConfigureAwait(false);

            Dictionary<string, ImmutableArray<CodeFixProvider>> fixersById = fixers
                .SelectMany(f => f.FixableDiagnosticIds.Select(id => (id, fixer: f)))
                .GroupBy(f => f.id)
                .ToDictionary(f => f.Key, g => g.Select(f => f.fixer).Distinct().ToImmutableArray());

            ImmutableArray<Diagnostic> unfixableDiagnostics = await GetDiagnosticsAsync(
                analyzers,
                fixResult.FixedDiagnostics,
                compilation,
                f => !fixersById.ContainsKey(f.id),
                cancellationToken)
                .ConfigureAwait(false);

            ImmutableArray<Diagnostic> unfixedDiagnostics = await GetDiagnosticsAsync(
                analyzers,
                fixResult.FixedDiagnostics.Concat(unfixableDiagnostics),
                compilation,
                f => fixersById.ContainsKey(f.id),
                cancellationToken)
                .ConfigureAwait(false);

            int numberOfAddedFileBanners = 0;

            if (Options.FileBannerLines.Any())
                numberOfAddedFileBanners = await AddFileBannerAsync(CurrentSolution.GetProject(project.Id), Options.FileBannerLines, cancellationToken).ConfigureAwait(false);

            ImmutableArray<DocumentId> formattedDocuments = ImmutableArray<DocumentId>.Empty;

            if (Options.Format)
                formattedDocuments = await FormatProjectAsync(CurrentSolution.GetProject(project.Id), cancellationToken).ConfigureAwait(false);

            var result = new ProjectFixResult(
                kind: fixResult.Kind,
                fixedDiagnostics: fixResult.FixedDiagnostics,
                unfixedDiagnostics: unfixedDiagnostics,
                unfixableDiagnostics: unfixableDiagnostics,
                analyzers: fixResult.Analyzers,
                fixers: fixResult.Fixers,
                numberOfFormattedDocuments: formattedDocuments.Length,
                numberOfAddedFileBanners: numberOfAddedFileBanners);

            LogHelpers.WriteFixSummary(
                result.FixedDiagnostics,
                result.UnfixedDiagnostics,
                result.UnfixableDiagnostics,
                baseDirectoryPath: Path.GetDirectoryName(project.FilePath),
                indentation: "  ",
                formatProvider: FormatProvider,
                verbosity: Verbosity.Detailed);

            return result;
        }

        private async Task<ProjectFixResult> FixProjectAsync(
            Project project,
            ImmutableArray<DiagnosticAnalyzer> analyzers,
            ImmutableArray<CodeFixProvider> fixers,
            CancellationToken cancellationToken)
        {
            if (!fixers.Any())
            {
                WriteLine($"  No fixers found to fix '{project.Name}'", ConsoleColor.DarkGray, Verbosity.Normal);
                return new ProjectFixResult(ProjectFixKind.NoFixers, analyzers: analyzers, fixers: fixers);
            }

            Dictionary<string, ImmutableArray<CodeFixProvider>> fixersById = fixers
                .SelectMany(f => f.FixableDiagnosticIds.Select(id => (id, fixer: f)))
                .GroupBy(f => f.id)
                .ToDictionary(f => f.Key, g => g.Select(f => f.fixer).Distinct().ToImmutableArray());

            analyzers = analyzers
                .Where(analyzer => analyzer.SupportedDiagnostics.Any(descriptor => fixersById.ContainsKey(descriptor.Id)))
                .ToImmutableArray();

            Dictionary<string, ImmutableArray<DiagnosticAnalyzer>> analyzersById = analyzers
                .SelectMany(f => f.SupportedDiagnostics.Select(d => (id: d.Id, analyzer: f)))
                .GroupBy(f => f.id, f => f.analyzer)
                .ToDictionary(g => g.Key, g => g.Select(analyzer => analyzer).Distinct().ToImmutableArray());

            LogHelpers.WriteUsedAnalyzers(analyzers, f => fixersById.ContainsKey(f.Id), Options, ConsoleColor.DarkGray, Verbosity.Diagnostic);
            LogHelpers.WriteUsedFixers(fixers, Options, ConsoleColor.DarkGray, Verbosity.Diagnostic);

            ImmutableArray<Diagnostic>.Builder fixedDiagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            ImmutableArray<Diagnostic> previousDiagnostics = ImmutableArray<Diagnostic>.Empty;
            ImmutableArray<Diagnostic> previousPreviousDiagnostics = ImmutableArray<Diagnostic>.Empty;

            var fixKind = ProjectFixKind.Success;

            for (int iterationCount = 1; ; iterationCount++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                project = CurrentSolution.GetProject(project.Id);

                WriteLine($"  Compile '{project.Name}'{((iterationCount > 1) ? $" iteration {iterationCount}" : "")}", Verbosity.Normal);

                Compilation compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                if (!VerifyCompilerDiagnostics(compilerDiagnostics, project))
                    return new ProjectFixResult(ProjectFixKind.CompilerError, fixedDiagnostics, analyzers: analyzers, fixers: fixers);

                WriteLine($"  Analyze '{project.Name}'", Verbosity.Normal);

                ImmutableArray<Diagnostic> diagnostics = ImmutableArray<Diagnostic>.Empty;

                if (analyzers.Any())
                {
                    diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(analyzers, Options.CompilationWithAnalyzersOptions, cancellationToken).ConfigureAwait(false);
                    LogHelpers.WriteAnalyzerExceptionDiagnostics(diagnostics);
                }

                diagnostics = GetFixableDiagnostics(diagnostics, compilerDiagnostics);

                int length = diagnostics.Length;

                if (length == 0)
                    break;

                if (length == previousDiagnostics.Length
                    && !diagnostics.Except(previousDiagnostics, DiagnosticDeepEqualityComparer.Instance).Any())
                {
                    break;
                }

                if (length == previousPreviousDiagnostics.Length
                    && !diagnostics.Except(previousPreviousDiagnostics, DiagnosticDeepEqualityComparer.Instance).Any())
                {
                    LogHelpers.WriteInfiniteLoopSummary(diagnostics, previousDiagnostics, project, FormatProvider);

                    fixKind = ProjectFixKind.InfiniteLoop;
                    break;
                }

                WriteLine($"  Found {length} {((length == 1) ? "diagnostic" : "diagnostics")} in '{project.Name}'", Verbosity.Normal);

                foreach (DiagnosticDescriptor descriptor in GetSortedDescriptors(diagnostics))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string diagnosticId = descriptor.Id;

                    DiagnosticFixResult result = await FixDiagnosticsAsync(
                        descriptor,
                        (descriptor.CustomTags.Contains(WellKnownDiagnosticTags.Compiler))
                            ? default
                            : analyzersById[diagnosticId],
                        fixersById[diagnosticId],
                        CurrentSolution.GetProject(project.Id),
                        cancellationToken)
                        .ConfigureAwait(false);

                    if (result.Kind == DiagnosticFixKind.Success)
                    {
                        fixedDiagnostics.AddRange(result.FixedDiagnostics);
                    }
                    else if (result.Kind == DiagnosticFixKind.CompilerError)
                    {
                        return new ProjectFixResult(ProjectFixKind.CompilerError, fixedDiagnostics, analyzers: analyzers, fixers: fixers);
                    }
                }

                if (iterationCount == Options.MaxIterations)
                    break;

                previousPreviousDiagnostics = previousDiagnostics;
                previousDiagnostics = diagnostics;
            }

            return new ProjectFixResult(fixKind, fixedDiagnostics, analyzers: analyzers, fixers: fixers);

            ImmutableArray<Diagnostic> GetFixableDiagnostics(
                ImmutableArray<Diagnostic> diagnostics,
                ImmutableArray<Diagnostic> compilerDiagnostics)
            {
                IEnumerable<Diagnostic> fixableCompilerDiagnostics = compilerDiagnostics
                    .Where(f => f.Severity != DiagnosticSeverity.Error
                        && !Options.IgnoredCompilerDiagnosticIds.Contains(f.Id)
                        && fixersById.ContainsKey(f.Id));

                return diagnostics
                    .Where(f => f.IsEffective(Options, project.CompilationOptions)
                        && analyzersById.ContainsKey(f.Id)
                        && fixersById.ContainsKey(f.Id))
                    .Concat(fixableCompilerDiagnostics)
                    .ToImmutableArray();
            }

            IEnumerable<DiagnosticDescriptor> GetSortedDescriptors(
                ImmutableArray<Diagnostic> diagnostics)
            {
                Dictionary<DiagnosticDescriptor, int> countByDescriptor = diagnostics
                    .GroupBy(f => f.Descriptor, DiagnosticDescriptorComparer.Id)
                    .ToDictionary(f => f.Key, f => f.Count());

                return countByDescriptor
                    .Select(f => f.Key)
                    .OrderBy(f => f, new DiagnosticDescriptorFixComparer(countByDescriptor, fixersById));
            }
        }

        private async Task<DiagnosticFixResult> FixDiagnosticsAsync(
            DiagnosticDescriptor descriptor,
            ImmutableArray<DiagnosticAnalyzer> analyzers,
            ImmutableArray<CodeFixProvider> fixers,
            Project project,
            CancellationToken cancellationToken)
        {
            ImmutableArray<Diagnostic>.Builder fixedDiagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            ImmutableArray<Diagnostic> diagnostics = ImmutableArray<Diagnostic>.Empty;
            ImmutableArray<Diagnostic> previousDiagnostics = ImmutableArray<Diagnostic>.Empty;
            ImmutableArray<Diagnostic> previousDiagnosticsToFix = ImmutableArray<Diagnostic>.Empty;

            int length = 0;
            var fixKind = DiagnosticFixKind.NotFixed;

            while (true)
            {
                Compilation compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                if (!VerifyCompilerDiagnostics(compilerDiagnostics, project))
                {
                    fixKind = DiagnosticFixKind.CompilerError;

                    if (!previousDiagnostics.Any())
                        break;
                }

                if (analyzers.IsDefault)
                {
                    diagnostics = compilerDiagnostics;
                }
                else
                {
                    diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(analyzers, Options.CompilationWithAnalyzersOptions, cancellationToken).ConfigureAwait(false);
                }

                diagnostics = diagnostics
                    .Where(f => f.Id == descriptor.Id && f.Severity >= Options.SeverityLevel)
                    .ToImmutableArray();

                if (fixKind == DiagnosticFixKind.CompilerError)
                {
                    break;
                }
                else if (fixKind == DiagnosticFixKind.Success)
                {
                    if (Options.BatchSize <= 0
                        || length <= Options.BatchSize)
                    {
                        break;
                    }
                }
                else if (previousDiagnostics.Any()
                    && fixKind != DiagnosticFixKind.PartiallyFixed)
                {
                    break;
                }

                length = diagnostics.Length;

                if (length == 0)
                    break;

                if (length == previousDiagnostics.Length
                    && !diagnostics.Except(previousDiagnostics, DiagnosticDeepEqualityComparer.Instance).Any())
                {
                    break;
                }

                fixedDiagnostics.AddRange(previousDiagnosticsToFix.Except(diagnostics, DiagnosticDeepEqualityComparer.Instance));

                previousDiagnostics = diagnostics;

                if (Options.FixAllScope == FixAllScope.Document)
                {
                    foreach (IGrouping<SyntaxTree, Diagnostic> grouping in diagnostics
                        .GroupBy(f => f.Location.SourceTree)
                        .OrderByDescending(f => f.Count()))
                    {
                        IEnumerable<Diagnostic> syntaxTreeDiagnostics = grouping.AsEnumerable();

                        if (Options.BatchSize > 0)
                            syntaxTreeDiagnostics = syntaxTreeDiagnostics.Take(Options.BatchSize);

                        ImmutableArray<Diagnostic> diagnosticsCandidate = syntaxTreeDiagnostics.ToImmutableArray();

                        DiagnosticFixKind fixKindCandidate = await FixDiagnosticsAsync(diagnosticsCandidate, descriptor, fixers, project, cancellationToken).ConfigureAwait(false);

                        if (fixKindCandidate == DiagnosticFixKind.Success
                            || fixKindCandidate == DiagnosticFixKind.PartiallyFixed)
                        {
                            diagnostics = diagnosticsCandidate;
                            fixKind = fixKindCandidate;
                            break;
                        }
                    }
                }
                else
                {
                    if (Options.BatchSize > 0
                        && length > Options.BatchSize)
                    {
                        diagnostics = ImmutableArray.CreateRange(diagnostics, 0, Options.BatchSize, f => f);
                    }

                    fixKind = await FixDiagnosticsAsync(diagnostics, descriptor, fixers, project, cancellationToken).ConfigureAwait(false);
                }

                previousDiagnosticsToFix = diagnostics;

                project = CurrentSolution.GetProject(project.Id);
            }

            fixedDiagnostics.AddRange(previousDiagnosticsToFix.Except(diagnostics, DiagnosticDeepEqualityComparer.Instance));

            return new DiagnosticFixResult(fixKind, fixedDiagnostics.ToImmutableArray());
        }

        private async Task<DiagnosticFixKind> FixDiagnosticsAsync(
            ImmutableArray<Diagnostic> diagnostics,
            DiagnosticDescriptor descriptor,
            ImmutableArray<CodeFixProvider> fixers,
            Project project,
            CancellationToken cancellationToken)
        {
            WriteLine($"  Fix {diagnostics.Length} {descriptor.Id} '{descriptor.Title}'", diagnostics[0].Severity.GetColor(), Verbosity.Normal);

            LogHelpers.WriteDiagnostics(diagnostics, baseDirectoryPath: Path.GetDirectoryName(project.FilePath), formatProvider: FormatProvider, indentation: "    ", verbosity: Verbosity.Detailed);

            DiagnosticFix diagnosticFix = await DiagnosticFixProvider.GetFixAsync(
                diagnostics,
                descriptor,
                fixers,
                project,
                Options,
                FormatProvider,
                cancellationToken)
                .ConfigureAwait(false);

            if (diagnosticFix.FixProvider2 != null)
                return DiagnosticFixKind.MultipleFixers;

            CodeAction fix = diagnosticFix.CodeAction;

            if (fix != null)
            {
                ImmutableArray<CodeActionOperation> operations = await fix.GetOperationsAsync(cancellationToken).ConfigureAwait(false);

                if (operations.Length == 1)
                {
                    operations[0].Apply(Workspace, cancellationToken);

                    return (diagnostics.Length != 1 && diagnosticFix.FixProvider.GetFixAllProvider() == null)
                        ? DiagnosticFixKind.PartiallyFixed
                        : DiagnosticFixKind.Success;
                }
                else if (operations.Length > 1)
                {
                    LogHelpers.WriteMultipleOperationsSummary(fix);
                }
            }

            return DiagnosticFixKind.NotFixed;
        }

        private bool VerifyCompilerDiagnostics(ImmutableArray<Diagnostic> diagnostics, Project project)
        {
            const string indentation = "    ";

            using (IEnumerator<Diagnostic> en = diagnostics
                .Where(f => f.Severity == DiagnosticSeverity.Error
                    && !Options.IgnoredCompilerDiagnosticIds.Contains(f.Id))
                .GetEnumerator())
            {
                if (en.MoveNext())
                {
                    Write(indentation);
                    WriteLine("Compilation errors:");

                    string baseDirectoryPath = Path.GetDirectoryName(project.FilePath);

                    const int maxCount = 10;

                    int count = 0;

                    do
                    {
                        count++;

                        if (count <= maxCount)
                        {
                            LogHelpers.WriteDiagnostic(
                                en.Current,
                                baseDirectoryPath: baseDirectoryPath,
                                formatProvider: FormatProvider,
                                indentation: indentation,
                                verbosity: Verbosity.Normal);
                        }
                        else
                        {
                            break;
                        }

                    } while (en.MoveNext());

                    count = 0;

                    var plus = false;

                    while (en.MoveNext())
                    {
                        count++;

                        if (count == 1000)
                        {
                            plus = true;
                            break;
                        }
                    }

                    if (count > maxCount)
                    {
                        Write(indentation);
                        WriteLine($"and {count}{((plus) ? "+" : "")} more errors", verbosity: Verbosity.Normal);
                    }

                    if (!Options.IgnoreCompilerErrors)
                    {
#if DEBUG
                        Console.Write("Stop (Y/N)? ");

                        if (char.ToUpperInvariant((char)Console.Read()) == 'Y')
                            return false;
#else
                        return false;
#endif
                    }
                }
            }

            return true;
        }

        private async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(
            ImmutableArray<DiagnosticAnalyzer> analyzers,
            IEnumerable<Diagnostic> except,
            Compilation compilation,
            Func<(string id, DiagnosticAnalyzer analyzer), bool> predicate,
            CancellationToken cancellationToken)
        {
            Dictionary<string, ImmutableArray<DiagnosticAnalyzer>> analyzersById = analyzers
                .SelectMany(f => f.SupportedDiagnostics.Select(d => (id: d.Id, analyzer: f)))
                .Where(predicate)
                .GroupBy(f => f.id, f => f.analyzer)
                .ToDictionary(g => g.Key, g => g.Select(analyzer => analyzer).Distinct().ToImmutableArray());

            analyzers = analyzersById
                .SelectMany(f => f.Value)
                .Distinct()
                .ToImmutableArray();

            if (!analyzers.Any())
                return ImmutableArray<Diagnostic>.Empty;

            ImmutableArray<Diagnostic> diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(analyzers, Options.CompilationWithAnalyzersOptions, cancellationToken).ConfigureAwait(false);

            return diagnostics
                .Where(f => f.IsEffective(Options, compilation.Options)
                    && analyzersById.ContainsKey(f.Id))
                .Except(except, DiagnosticDeepEqualityComparer.Instance)
                .ToImmutableArray();
        }

        private async Task<int> AddFileBannerAsync(
            Project project,
            ImmutableArray<string> banner,
            CancellationToken cancellationToken)
        {
            int count = 0;

            string solutionDirectory = Path.GetDirectoryName(project.Solution.FilePath);

            foreach (DocumentId documentId in project.DocumentIds)
            {
                Document document = project.GetDocument(documentId);

                if (GeneratedCodeUtility.IsGeneratedCodeFile(document.FilePath))
                    continue;

                SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

                ISyntaxFactsService syntaxFacts = MefWorkspaceServices.Default.GetService<ISyntaxFactsService>(project.Language);

                if (syntaxFacts.BeginsWithAutoGeneratedComment(root))
                    continue;

                if (syntaxFacts.BeginsWithBanner(root, banner))
                    continue;

                SyntaxTriviaList leading = root.GetLeadingTrivia();

                SyntaxTriviaList newLeading = leading.InsertRange(0, banner.SelectMany(f => syntaxFacts.ParseLeadingTrivia(syntaxFacts.SingleLineCommentStart + f + Environment.NewLine)));

                if (!syntaxFacts.IsEndOfLineTrivia(leading.LastOrDefault()))
                    newLeading = newLeading.AddRange(syntaxFacts.ParseLeadingTrivia(Environment.NewLine));

                SyntaxNode newRoot = root.WithLeadingTrivia(newLeading);

                Document newDocument = document.WithSyntaxRoot(newRoot);

                WriteLine($"  Add banner to '{PathUtilities.TrimStart(document.FilePath, solutionDirectory)}'", ConsoleColor.DarkGray, Verbosity.Detailed);

                project = newDocument.Project;

                count++;
            }

            if (count > 0
                && !Workspace.TryApplyChanges(project.Solution))
            {
                Debug.Fail($"Cannot apply changes to solution '{project.Solution.FilePath}'");
                WriteLine($"Cannot apply changes to solution '{project.Solution.FilePath}'", ConsoleColor.Yellow, Verbosity.Diagnostic);
            }

            return count;
        }

        private async Task<ImmutableArray<DocumentId>> FormatProjectAsync(Project project, CancellationToken cancellationToken)
        {
            WriteLine($"  Format  '{project.Name}'", Verbosity.Normal);

            ISyntaxFactsService syntaxFacts = MefWorkspaceServices.Default.GetService<ISyntaxFactsService>(project.Language);

            Project newProject = await CodeFormatter.FormatProjectAsync(project, syntaxFacts, cancellationToken).ConfigureAwait(false);

            string solutionDirectory = Path.GetDirectoryName(project.Solution.FilePath);

            ImmutableArray<DocumentId> formattedDocuments = await CodeFormatter.GetFormattedDocumentsAsync(project, newProject, syntaxFacts).ConfigureAwait(false);

            LogHelpers.WriteFormattedDocuments(formattedDocuments, project, solutionDirectory);

            if (formattedDocuments.Length > 0
                && !Workspace.TryApplyChanges(newProject.Solution))
            {
                Debug.Fail($"Cannot apply changes to solution '{newProject.Solution.FilePath}'");
                WriteLine($"Cannot apply changes to solution '{newProject.Solution.FilePath}'", ConsoleColor.Yellow, Verbosity.Diagnostic);
            }

            return formattedDocuments;
        }
    }
}
