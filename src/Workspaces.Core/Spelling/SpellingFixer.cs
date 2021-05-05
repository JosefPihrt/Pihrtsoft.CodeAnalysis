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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Host.Mef;
using static Roslynator.Logger;

namespace Roslynator.Spelling
{
    internal class SpellingFixer
    {
        public SpellingFixer(
            Solution solution,
            SpellingData spellingData,
            IFormatProvider formatProvider = null,
            SpellingFixerOptions options = null)
        {
            Workspace = solution.Workspace;

            SpellingData = spellingData;
            FormatProvider = formatProvider;
            Options = options ?? SpellingFixerOptions.Default;
        }

        public Workspace Workspace { get; }

        public SpellingData SpellingData { get; private set; }

        public IFormatProvider FormatProvider { get; }

        public SpellingFixerOptions Options { get; }

        public List<NewWord> NewWords { get; } = new List<NewWord>();

        private Solution CurrentSolution => Workspace.CurrentSolution;

        public async Task FixSolutionAsync(Func<Project, bool> predicate, CancellationToken cancellationToken = default)
        {
            ImmutableArray<ProjectId> projects = CurrentSolution
                .GetProjectDependencyGraph()
                .GetTopologicallySortedProjects(cancellationToken)
                .ToImmutableArray();

            var results = new List<ImmutableArray<SpellingFixResult>>();

            Stopwatch stopwatch = Stopwatch.StartNew();

            TimeSpan lastElapsed = TimeSpan.Zero;

            for (int i = 0; i < projects.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Project project = CurrentSolution.GetProject(projects[i]);

                if (predicate == null || predicate(project))
                {
                    WriteLine($"Fix '{project.Name}' {$"{i + 1}/{projects.Length}"}", ConsoleColor.Cyan, Verbosity.Minimal);

                    ImmutableArray<SpellingFixResult> results2 = await FixProjectAsync(project, cancellationToken).ConfigureAwait(false);

                    results.Add(results2);
                }
                else
                {
                    WriteLine($"Skip '{project.Name}' {$"{i + 1}/{projects.Length}"}", ConsoleColor.DarkGray, Verbosity.Minimal);
                }

                TimeSpan elapsed = stopwatch.Elapsed;

                WriteLine($"Done fixing '{project.Name}' in {elapsed - lastElapsed:mm\\:ss\\.ff}", Verbosity.Normal);

                lastElapsed = elapsed;
            }

            stopwatch.Stop();

            WriteLine($"Done fixing solution '{CurrentSolution.FilePath}' in {stopwatch.Elapsed:mm\\:ss\\.ff}", Verbosity.Minimal);
#if DEBUG
            WriteLine(Verbosity.Normal);

            foreach (IGrouping<SpellingFixResult, SpellingFixResult> grouping in results
                .SelectMany(f => f)
                .OrderBy(f => f.OldValue)
                .ThenBy(f => f.NewValue)
                .GroupBy(f => f, SpellingFixResultEqualityComparer.OldValueAndNewValue))
            {
                WriteLine($"{grouping.Key.OldValue} = {grouping.Key.NewValue}", Verbosity.Normal);

                foreach (SpellingFixResult result in grouping)
                {
                    if (result.IsSymbol)
                        WriteLine($"  {result.OldIdentifier}  {result.NewIdentifier}", Verbosity.Normal);
                }
            }
#endif
        }

        public async Task<ImmutableArray<SpellingFixResult>> FixProjectAsync(
            Project project,
            CancellationToken cancellationToken = default)
        {
            project = CurrentSolution.GetProject(project.Id);

            ISpellingService service = MefWorkspaceServices.Default.GetService<ISpellingService>(project.Language);

            if (service == null)
                return ImmutableArray<SpellingFixResult>.Empty;

            ImmutableArray<Diagnostic> previousDiagnostics = ImmutableArray<Diagnostic>.Empty;
            ImmutableArray<Diagnostic> previousPreviousDiagnostics = ImmutableArray<Diagnostic>.Empty;

            ImmutableArray<SpellingFixResult>.Builder results = ImmutableArray.CreateBuilder<SpellingFixResult>();

            bool commentsFixed = (Options.ScopeFilter & SpellingScopeFilter.NonSymbol) == 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var compilationWithAnalyzersOptions = new CompilationWithAnalyzersOptions(
                    options: default(AnalyzerOptions),
                    onAnalyzerException: default(Action<Exception, DiagnosticAnalyzer, Diagnostic>),
                    concurrentAnalysis: true,
                    logAnalyzerExecutionTime: false,
                    reportSuppressedDiagnostics: false);

                Compilation compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

                DiagnosticAnalyzer analyzer = service.CreateAnalyzer(SpellingData, Options);

                var compilationWithAnalyzers = new CompilationWithAnalyzers(
                    compilation,
                    ImmutableArray.Create(analyzer),
                    compilationWithAnalyzersOptions);

                ImmutableArray<Diagnostic> diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken).ConfigureAwait(false);
#if DEBUG
                foreach (IGrouping<Diagnostic, Diagnostic> grouping in diagnostics
                    .GroupBy(f => f, DiagnosticDeepEqualityComparer.Instance))
                {
                    using (IEnumerator<Diagnostic> en = grouping.GetEnumerator())
                    {
                        if (en.MoveNext()
                            && en.MoveNext())
                        {
                            Debug.Fail(DiagnosticFormatter.FormatDiagnostic(en.Current));
                        }
                    }
                }
#endif
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
                    break;
                }

                var spellingDiagnostics = new List<SpellingDiagnostic>();

                foreach (Diagnostic diagnostic in diagnostics)
                {
                    Debug.Assert(diagnostic.Id == SpellingAnalyzer.DiagnosticId, diagnostic.Id);

                    if (diagnostic.Id != SpellingAnalyzer.DiagnosticId)
                    {
                        if (diagnostic.IsAnalyzerExceptionDiagnostic())
                            LogHelpers.WriteDiagnostic(diagnostic, baseDirectoryPath: Path.GetDirectoryName(project.FilePath), formatProvider: FormatProvider, indentation: "  ", verbosity: Verbosity.Detailed);

                        continue;
                    }

                    SpellingDiagnostic spellingDiagnostic = service.CreateSpellingDiagnostic(diagnostic);

                    spellingDiagnostics.Add(spellingDiagnostic);
                }

                if (!commentsFixed)
                {
                    List<SpellingFixResult> commentResults = await FixCommentsAsync(project, spellingDiagnostics, cancellationToken).ConfigureAwait(false);
                    results.AddRange(commentResults);
                    commentsFixed = true;
                }

                if ((Options.ScopeFilter & SpellingScopeFilter.Symbol) == 0)
                    break;

                (List<SpellingFixResult> symbolResults, bool allIgnored) = await FixSymbolsAsync(
                    project,
                    spellingDiagnostics,
                    service.SyntaxFacts,
                    cancellationToken)
                    .ConfigureAwait(false);

                results.AddRange(symbolResults);

                if (allIgnored)
                    break;

                if (Options.DryRun)
                    break;

                project = CurrentSolution.GetProject(project.Id);

                previousPreviousDiagnostics = previousDiagnostics;
                previousDiagnostics = diagnostics;
            }

            return results.ToImmutableArray();
        }

        private async Task<List<SpellingFixResult>> FixCommentsAsync(
            Project project,
            List<SpellingDiagnostic> diagnostics,
            CancellationToken cancellationToken)
        {
            var results = new List<SpellingFixResult>();

            List<SpellingDiagnostic> commentDiagnostics = diagnostics.Where(f => !f.IsSymbol).ToList();

            var applyChanges = false;

            project = CurrentSolution.GetProject(project.Id);

            foreach (IGrouping<SyntaxTree, SpellingDiagnostic> grouping in commentDiagnostics
                .GroupBy(f => f.SyntaxTree))
            {
                cancellationToken.ThrowIfCancellationRequested();

                Document document = project.GetDocument(grouping.Key);

                SourceText sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

                List<TextChange> textChanges = null;

                foreach (SpellingDiagnostic diagnostic in grouping.OrderBy(f => f.Span.Start))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (SpellingData.IgnoredValues.Contains(diagnostic.Value))
                        continue;

                    LogHelpers.WriteSpellingDiagnostic(diagnostic, Options, sourceText, Path.GetDirectoryName(project.FilePath), "    ", Verbosity.Normal);

                    SpellingFix fix = GetFix(diagnostic);

                    if (!fix.IsDefault)
                    {
                        if (!string.Equals(diagnostic.Value, fix.Value, StringComparison.Ordinal))
                        {
                            WriteFix(diagnostic, fix, ConsoleColor.Green);

                            if (!Options.DryRun)
                                (textChanges ??= new List<TextChange>()).Add(new TextChange(diagnostic.Span, fix.Value));

                            results.Add(new SpellingFixResult(
                                diagnostic.Value,
                                fix.Value,
                                diagnostic.Location.GetMappedLineSpan()));

                            ProcessFix(diagnostic, fix);
                        }
                        else
                        {
                            AddIgnoredValue(diagnostic);
                            AddNewWord(diagnostic, sourceText);
                        }
                    }
                    else
                    {
                        AddNewWord(diagnostic, sourceText);
                    }
                }

                if (textChanges != null)
                {
                    document = await document.WithTextChangesAsync(textChanges, cancellationToken).ConfigureAwait(false);
                    project = document.Project;

                    applyChanges = true;
                }
            }

            if (applyChanges
                && !Workspace.TryApplyChanges(project.Solution))
            {
                Debug.Fail($"Cannot apply changes to solution '{project.Solution.FilePath}'");
                WriteLine($"    Cannot apply changes to solution '{project.Solution.FilePath}'", ConsoleColor.Yellow, Verbosity.Diagnostic);
            }

            return results;
        }

        private async Task<(List<SpellingFixResult>, bool allIgnored)> FixSymbolsAsync(
            Project project,
            List<SpellingDiagnostic> spellingDiagnostics,
            ISyntaxFactsService syntaxFacts,
            CancellationToken cancellationToken)
        {
            var results = new List<SpellingFixResult>();
            var allIgnored = true;

            List<(SyntaxToken identifier, List<SpellingDiagnostic> diagnostics, DocumentId documentId)> symbolDiagnostics = spellingDiagnostics
                .Where(f => f.IsSymbol)
                .GroupBy(f => f.Identifier)
                .Select(f => (
                    identifier: f.Key,
                    diagnostics: f.OrderBy(f => f.Span.Start).ToList(),
                    documentId: project.GetDocument(f.Key.SyntaxTree).Id))
                .OrderBy(f => f.documentId.Id)
                .ThenByDescending(f => f.identifier.SpanStart)
                .ToList();

            for (int i = 0; i < symbolDiagnostics.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                (SyntaxToken identifier, List<SpellingDiagnostic> diagnostics, DocumentId documentId) = symbolDiagnostics[i];

                SyntaxNode node = null;

                foreach (SpellingDiagnostic diagnostic in diagnostics)
                {
                    if (diagnostic != null)
                    {
                        node = syntaxFacts.GetSymbolDeclaration(diagnostic.Identifier);
                        break;
                    }
                }

                if (node == null)
                    continue;

                Document document = project.GetDocument(documentId);

                if (document == null)
                {
                    Debug.Fail(identifier.GetLocation().ToString());

                    WriteLine($"    Cannot find document for'{identifier.ValueText}'", ConsoleColor.Yellow, Verbosity.Detailed);
                    continue;
                }

                SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

                if (identifier.SyntaxTree != root.SyntaxTree)
                {
                    SyntaxToken identifier2 = root.FindToken(identifier.SpanStart, findInsideTrivia: false);

                    if (identifier.Span != identifier2.Span
                        || identifier.RawKind != identifier2.RawKind
                        || !string.Equals(identifier2.ValueText, identifier2.ValueText, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    SyntaxNode node2 = identifier2.Parent;

                    SyntaxNode n = identifier.Parent;
                    while (n != node)
                    {
                        node2 = node2.Parent;
                        n = n.Parent;
                    }

                    identifier = identifier2;
                    node = node2;
                }

                SourceText sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

                SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                ISymbol symbol = semanticModel.GetDeclaredSymbol(node, cancellationToken)
                    ?? semanticModel.GetSymbol(node, cancellationToken);

                if (symbol == null)
                {
                    // 8925 - C# tuple element
                    Debug.Assert(identifier.Parent.RawKind == 8925, identifier.ToString());

                    if (ShouldWrite(Verbosity.Detailed))
                    {
                        string message = $"    Cannot find symbol for '{identifier.ValueText}'";

                        string locationText = GetLocationText(identifier.GetLocation(), project);

                        if (locationText != null)
                            message += $" at {locationText}";

                        WriteLine(message, ConsoleColor.Yellow, Verbosity.Detailed);
                    }

                    continue;
                }

                if (!symbol.IsKind(SymbolKind.Namespace, SymbolKind.Alias)
                    && !symbol.IsVisible(Options.SymbolVisibility))
                {
                    continue;
                }

                allIgnored = false;

                var fixes = new List<(SpellingDiagnostic diagnostic, SpellingFix fix)>();
                string newName = identifier.ValueText;
                int indexOffset = 0;

                for (int j = 0; j < diagnostics.Count; j++)
                {
                    SpellingDiagnostic diagnostic = diagnostics[j];

                    if (diagnostic == null)
                        continue;

                    LogHelpers.WriteSpellingDiagnostic(diagnostic, Options, sourceText, Path.GetDirectoryName(project.FilePath), "    ", Verbosity.Normal);

                    SpellingFix fix = GetFix(diagnostic);
                    fixes.Add((diagnostic, fix));

                    if (!fix.IsDefault)
                    {
                        if (!string.Equals(diagnostic.Value, fix.Value, StringComparison.Ordinal))
                        {
                            WriteFix(diagnostic, fix);
                            newName = TextUtility.ReplaceRange(newName, fix.Value, diagnostic.Offset + indexOffset, diagnostic.Length);

                            indexOffset += fix.Value.Length - diagnostic.Length;
                        }
                        else
                        {
                            for (int k = 0; k < symbolDiagnostics.Count; k++)
                            {
                                List<SpellingDiagnostic> diagnostics2 = symbolDiagnostics[k].diagnostics;

                                for (int l = 0; l < diagnostics2.Count; l++)
                                {
                                    if (SpellingData.IgnoredValues.KeyComparer.Equals(diagnostics2[l]?.Value, diagnostic.Value))
                                        diagnostics2[l] = null;
                                }
                            }

                            AddIgnoredValue(diagnostic);
                            AddNewWord(diagnostic, sourceText);
                        }
                    }
                    else
                    {
                        AddNewWord(diagnostic, sourceText);
                    }
                }

                if (string.Equals(identifier.Text, newName, StringComparison.Ordinal))
                    continue;

                Solution newSolution = null;
                if (!Options.DryRun)
                {
                    WriteLine($"    Rename '{identifier.ValueText}' to '{newName}'", ConsoleColor.Green, Verbosity.Minimal);

                    try
                    {
                        //TODO: detect name conflict
                        newSolution = await Microsoft.CodeAnalysis.Rename.Renamer.RenameSymbolAsync(
                            CurrentSolution,
                            symbol,
                            newName,
                            default(Microsoft.CodeAnalysis.Options.OptionSet),
                            cancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (InvalidOperationException
#if DEBUG
                        ex
#endif
                    )
                    {
                        WriteLine($"    Cannot rename '{symbol.Name}'", ConsoleColor.Yellow, Verbosity.Normal);
#if DEBUG
                        WriteLine(document.FilePath);
                        WriteLine(identifier.ValueText);
                        WriteLine(ex.ToString());
#endif
                        continue;
                    }
                }

                if (newSolution != null)
                {
                    if (Workspace.TryApplyChanges(newSolution))
                    {
                        project = CurrentSolution.GetProject(project.Id);
                    }
                    else
                    {
                        Debug.Fail($"Cannot apply changes to solution '{newSolution.FilePath}'");
                        WriteLine($"    Cannot apply changes to solution '{newSolution.FilePath}'", ConsoleColor.Yellow, Verbosity.Normal);
                        continue;
                    }
                }

                foreach ((SpellingDiagnostic diagnostic, SpellingFix fix) in fixes)
                {
                    if (fix.IsDefault)
                        continue;

                    results.Add(new SpellingFixResult(
                        diagnostic.Value,
                        fix.Value,
                        diagnostic.Identifier.ValueText,
                        newName,
                        diagnostic.Index,
                        diagnostic.Location.GetMappedLineSpan()));

                    ProcessFix(diagnostic, fix);
                }
            }

            return (results, allIgnored);
        }

        private SpellingFix GetParentFix(SpellingDiagnostic diagnostic)
        {
            string containingValue = diagnostic.Parent;

            if (Options.AutoFix
                && containingValue != null
                && SpellingData.Fixes.TryGetKey(containingValue, out string actualKey)
                && string.Equals(containingValue, actualKey, StringComparison.Ordinal))
            {
                SpellingFix fix = SpellingData.Fixes.Items[containingValue].SingleOrDefault(
                    f => f.Kind == SpellingFixKind.Predefined && diagnostic.IsApplicableFix(f.Value),
                    shouldThrow: false);

                if (!fix.IsDefault)
                    return fix;
            }

            return default;
        }

        private SpellingFix GetFix(SpellingDiagnostic diagnostic)
        {
            string value = diagnostic.Value;

            if (Options.AutoFix)
            {
                TextCasing textCasing = TextUtility.GetTextCasing(value);

                if (textCasing != TextCasing.Undefined
                    && SpellingData.Fixes.TryGetValue(value, out ImmutableHashSet<SpellingFix> possibleFixes))
                {
                    SpellingFix fix = possibleFixes.SingleOrDefault(
                        f => (TextUtility.GetTextCasing(f.Value) != TextCasing.Undefined
                            || string.Equals(value, f.Value, StringComparison.OrdinalIgnoreCase))
                            && diagnostic.IsApplicableFix(f.Value),
                        shouldThrow: false);

                    if (!fix.IsDefault)
                    {
                        if (!string.Equals(value, fix.Value, StringComparison.OrdinalIgnoreCase))
                            fix = fix.WithValue(TextUtility.SetTextCasing(fix.Value, textCasing));

                        return fix;
                    }
                }
            }

            if (Options.Interactive)
            {
                while (true)
                {
                    string replacement = ConsoleUtility.ReadUserInput(value, "    Replacement: ");

                    if (string.Equals(value, replacement, StringComparison.Ordinal))
                        return new SpellingFix(replacement, SpellingFixKind.User);

                    if (diagnostic.IsApplicableFix(replacement))
                    {
                        return new SpellingFix(replacement, SpellingFixKind.User);
                    }
                    else
                    {
                        Console.WriteLine("    Replacement is invalid.");
                    }
                }
            }

            return default;
        }

        private static void WriteFix(SpellingDiagnostic diagnostic, SpellingFix fix, ConsoleColor? color = null)
        {
            string message = $"    Replace '{diagnostic.Value}' with '{fix.Value}'";

            if (color != null)
            {
                WriteLine(message, color.Value, Verbosity.Minimal);
            }
            else
            {
                WriteLine(message, Verbosity.Minimal);
            }
        }

        private void ProcessFix(SpellingDiagnostic diagnostic, SpellingFix spellingFix)
        {
            if (spellingFix.Kind != SpellingFixKind.Predefined)
            {
                if (string.Equals(diagnostic.Value, spellingFix.Value, StringComparison.OrdinalIgnoreCase)
                    || TextUtility.TextCasingEquals(diagnostic.Value, spellingFix.Value))
                {
                    SpellingData = SpellingData.AddFix(diagnostic.Value, spellingFix);
                }
            }

            SpellingData = SpellingData.AddWord(spellingFix.Value);
        }

        private void AddNewWord(SpellingDiagnostic diagnostic, SourceText sourceText)
        {
            var newWord = new NewWord(
                diagnostic.Value,
                sourceText.Lines.GetLineFromPosition(diagnostic.Index).ToString(),
                diagnostic.Location.GetLineSpan(),
                diagnostic.Parent);

            NewWords.Add(newWord);
        }

        private void AddIgnoredValue(SpellingDiagnostic diagnostic)
        {
            SpellingData = SpellingData.AddIgnoredValue(diagnostic.Value);
        }

        private string GetLocationText(Location location, Project project)
        {
            if (location.Kind == LocationKind.SourceFile
                || location.Kind == LocationKind.XmlFile
                || location.Kind == LocationKind.ExternalFile)
            {
                FileLinePositionSpan span = location.GetMappedLineSpan();

                if (span.IsValid)
                {
                    return $"{PathUtilities.TrimStart(span.Path, Path.GetDirectoryName(project.FilePath))}({span.StartLinePosition.Line + 1},{span.StartLinePosition.Character + 1})";
                }
            }

            return null;
        }
    }
}
