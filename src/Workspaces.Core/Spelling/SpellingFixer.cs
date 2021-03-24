// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private static readonly Regex _lowercasedSeparatedWithUnderscoresRegex = new Regex(@"\A_*\p{Ll}+(_+\p{Ll}+)+\z");

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
        }

        public async Task<ImmutableArray<SpellingFixResult>> FixProjectAsync(
            Project project,
            CancellationToken cancellationToken = default)
        {
            project = CurrentSolution.GetProject(project.Id);

            ReportDiagnostic reportDiagnostic = SpellingAnalyzer.DiagnosticDescriptor.GetEffectiveSeverity(project.CompilationOptions);

            if (reportDiagnostic == ReportDiagnostic.Suppress)
                return ImmutableArray<SpellingFixResult>.Empty;

            ISpellingService service = MefWorkspaceServices.Default.GetService<ISpellingService>(project.Language);

            if (service == null)
                return ImmutableArray<SpellingFixResult>.Empty;

            ImmutableArray<SpellingFixResult>.Builder results = ImmutableArray.CreateBuilder<SpellingFixResult>();

            var commentsFixed = false;

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

                if (!diagnostics.Any())
                    break;

                var spellingDiagnostics = new List<SpellingDiagnostic>();

                foreach (Diagnostic diagnostic in diagnostics)
                {
                    Debug.Assert(diagnostic.Id == SpellingAnalyzer.DiagnosticId, diagnostic.Id);

                    if (diagnostic.Id != SpellingAnalyzer.DiagnosticId)
                        continue;

                    SpellingDiagnostic spellingDiagnostic = service.CreateSpellingDiagnostic(diagnostic);

                    Debug.Assert(spellingDiagnostic != null);

                    if (spellingDiagnostic != null)
                        spellingDiagnostics.Add(spellingDiagnostic);
                }

                if (!commentsFixed)
                {
                    List<SpellingFixResult> commentResults = await FixCommentsAsync(project, spellingDiagnostics, cancellationToken).ConfigureAwait(false);
                    results.AddRange(commentResults);
                    commentsFixed = true;
                }

                List<SpellingFixResult> symbolResults = await FixSymbolsAsync(project, spellingDiagnostics, service.SyntaxFacts, cancellationToken).ConfigureAwait(false);
                results.AddRange(symbolResults);

                if (Options.DryRun)
                    break;

                project = CurrentSolution.GetProject(project.Id);
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
                .GroupBy(f => f.Location.SourceTree))
            {
                cancellationToken.ThrowIfCancellationRequested();

                Document document = project.GetDocument(grouping.Key);

                SourceText sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

                List<TextChange> textChanges = null;

                foreach (SpellingDiagnostic diagnostic in grouping.OrderBy(f => f.Location.SourceSpan.Start))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (SpellingData.IgnoreList.Contains(diagnostic.Value))
                        continue;

                    LogHelpers.WriteSpellingDiagnostic(diagnostic, Options, sourceText, Path.GetDirectoryName(project.FilePath), "    ", Verbosity.Normal);

                    SpellingFix fix = GetFix(diagnostic, cancellationToken);

                    Debug.Assert(fix.Value != "");

                    if (!fix.IsDefault
                        && !string.Equals(fix.Value, diagnostic.Value, StringComparison.Ordinal))
                    {
                        if (!Options.DryRun)
                            (textChanges ??= new List<TextChange>()).Add(new TextChange(diagnostic.Location.SourceSpan, fix.Value));

                        results.Add(new SpellingFixResult(
                            diagnostic.Value,
                            fix.Value,
                            diagnostic.Location.GetMappedLineSpan()));

                        ProcessFix(diagnostic, fix);
                    }
                    else
                    {
                        SpellingData = SpellingData.AddIgnoredValue(diagnostic.Value);
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

        private async Task<List<SpellingFixResult>> FixSymbolsAsync(
            Project project,
            List<SpellingDiagnostic> spellingDiagnostics,
            ISyntaxFactsService syntaxFacts,
            CancellationToken cancellationToken)
        {
            var results = new List<SpellingFixResult>();

            List<(SyntaxToken identifier, List<SpellingDiagnostic> diagnostics, DocumentId documentId)> symbolDiagnostics = spellingDiagnostics
                .Where(f => f.IsSymbol)
                .GroupBy(f => f.Identifier)
                .Select(f => (
                    identifier: f.Key,
                    diagnostics: f.OrderBy(f => f.Location.SourceSpan.Start).ToList(),
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

                Debug.Assert(document != null, identifier.GetLocation().ToString());

                if (document == null)
                {
                    WriteLine($"    Cannot find document for'{identifier.ValueText}'", ConsoleColor.Yellow, Verbosity.Detailed);
                    SpellingData = SpellingData.AddIgnoredValues(diagnostics);
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

                Debug.Assert(symbol != null, identifier.ToString());

                if (symbol == null)
                {
                    WriteLine($"    Cannot find symbol for '{identifier.ValueText}'", ConsoleColor.Yellow, Verbosity.Detailed);
                    SpellingData = SpellingData.AddIgnoredValues(diagnostics);
                    continue;
                }

                if (!symbol.IsKind(SymbolKind.Namespace, SymbolKind.Alias)
                    && !symbol.IsVisible(Options.SymbolVisibility))
                {
                    continue;
                }

                var fixes = new List<(SpellingDiagnostic diagnostic, SpellingFix fix)>();
                string newName = identifier.ValueText;
                int indexOffset = 0;

                for (int j = 0; j < diagnostics.Count; j++)
                {
                    SpellingDiagnostic diagnostic = diagnostics[j];

                    if (diagnostic == null)
                        continue;

                    LogHelpers.WriteSpellingDiagnostic(diagnostic, Options, sourceText, Path.GetDirectoryName(project.FilePath), "    ", Verbosity.Normal);

                    SpellingFix fix = GetFix(diagnostic, cancellationToken);
                    fixes.Add((diagnostic, fix));

                    if (!fix.IsDefault)
                    {
                        newName = TextUtility.ReplaceRange(newName, fix.Value, diagnostic.Index + indexOffset, diagnostic.Length);

                        indexOffset += fix.Value.Length - diagnostic.Length;
                    }
                    else
                    {
                        SpellingData = SpellingData.AddIgnoredValue(diagnostic.Value);

                        for (int k = 0; k < symbolDiagnostics.Count; k++)
                        {
                            List<SpellingDiagnostic> diagnostics2 = symbolDiagnostics[k].diagnostics;

                            for (int l = 0; l < diagnostics2.Count; l++)
                            {
                                if (SpellingData.IgnoreList.Comparer.Equals(diagnostics2[l]?.Value, diagnostic.Value))
                                    diagnostics2[l] = null;
                            }
                        }
                    }
                }

                if (string.Equals(identifier.Text, newName, StringComparison.Ordinal))
                    continue;

                Solution newSolution = null;
                if (!Options.DryRun)
                {
                    WriteLine($"    Rename '{identifier.ValueText}' to '{newName}'", Verbosity.Minimal);

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
                    catch (InvalidOperationException ex)
                    {
                        WriteLine($"    Cannot rename '{symbol.Name}'", ConsoleColor.Yellow, Verbosity.Normal);
#if DEBUG
                        WriteLine(document.FilePath);
                        WriteLine(identifier.ValueText);
                        WriteLine(ex.ToString());
#endif
                        SpellingData = SpellingData.AddIgnoredValues(diagnostics);
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
                        WriteLine($"    Cannot apply changes to solution '{newSolution.FilePath}'", ConsoleColor.Yellow, Verbosity.Diagnostic);

                        SpellingData = SpellingData.AddIgnoredValues(diagnostics);
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

            return results;
        }

        private SpellingFix GetFix(SpellingDiagnostic diagnostic, CancellationToken cancellationToken)
        {
            string value = diagnostic.Value;
            string containingValue = diagnostic.ContainingValue;

            if (Options.AutoFix
                && diagnostic.IsContained
                && SpellingData.FixList.TryGetValue(containingValue, out ImmutableHashSet<SpellingFix> fixes2))
            {
                SpellingFix fix2 = fixes2.SingleOrDefault(
                    f => f.Kind == SpellingFixKind.Predefined && diagnostic.IsApplicableFix(f.Value),
                    shouldThrow: false);

                if (string.Equals(containingValue, fix2.Value, StringComparison.Ordinal))
                {
                    WriteFix(diagnostic, fix2);
                    return fix2;
                }
            }

            SpellingFix fix = default;
            var fixes = new List<SpellingFix>();

            TextCasing textCasing = TextUtility.GetTextCasing(value);

            if (SpellingData.FixList.TryGetValue(value, out fixes2))
            {
                if (Options.AutoFix
                    && textCasing != TextCasing.Undefined)
                {
                    fix = fixes2.SingleOrDefault(
                        f => TextUtility.GetTextCasing(f.Value) != TextCasing.Undefined
                            && diagnostic.IsApplicableFix(f.Value),
                        shouldThrow: false);

                    if (!fix.IsDefault)
                        fix = fix.WithValue(TextUtility.SetTextCasing(fix.Value, textCasing));
                }

                if (fix.IsDefault)
                {
                    fixes = fixes2
                        .Where(f => TextUtility.GetTextCasing(f.Value) != TextCasing.Undefined
                            && diagnostic.IsApplicableFix(f.Value))
                        .Select(f => f.WithValue(TextUtility.SetTextCasing(f.Value, textCasing)))
                        .ToList();
                }
            }

            if (fix.IsDefault
                && Options.Interactive)
            {
                if (textCasing != TextCasing.Undefined)
                {
                    if (fixes.Count == 0)
                        AddPossibleFixes(diagnostic, ref fixes, cancellationToken);

                    fix = ChooseFix(diagnostic, fixes);
                }

                if (fix.IsDefault)
                    fix = GetUserFix();
            }

            if (!fix.IsDefault)
                WriteFix(diagnostic, fix);

            return fix;
        }

        private SpellingFix ChooseFix(
            SpellingDiagnostic diagnostic,
            List<SpellingFix> fixes)
        {
            //TODO: set max number of suggestions
            fixes = fixes
                .Distinct(SpellingFixComparer.CurrentCulture)
                .Where(f =>
                {
                    return f.Kind == SpellingFixKind.Predefined
                        || diagnostic.IsApplicableFix(f.Value);
                })
                .Select(fix =>
                {
                    if (TextUtility.GetTextCasing(fix.Value) != TextCasing.Undefined)
                        return fix.WithValue(TextUtility.SetTextCasing(fix.Value, diagnostic.Casing));

                    return fix;
                })
                .OrderBy(f => f.Kind)
                .Take(9)
                .ToList();

            if (fixes.Count > 0)
            {
                for (int i = 0; i < fixes.Count; i++)
                    WriteSuggestion(diagnostic, fixes[i], i);

                if (TryReadSuggestion(out int index)
                    && index < fixes.Count)
                {
                    return fixes[index];
                }
            }

            return default;
        }

        private void AddPossibleFixes(
            SpellingDiagnostic diagnostic,
            ref List<SpellingFix> fixes,
            CancellationToken cancellationToken)
        {
            Debug.WriteLine($"find possible fix for '{diagnostic.Value}'");

            string value = diagnostic.Value;

            ImmutableArray<string> matches = SpellingFixProvider.SwapMatches(
                diagnostic.ValueLower,
                SpellingData);

            foreach (string match in matches)
                fixes.Add(new SpellingFix(match, SpellingFixKind.Swap));

            if (fixes.Count == 0
                && (diagnostic.Casing == TextCasing.Lower
                    || diagnostic.Casing == TextCasing.FirstUpper))
            {
                foreach (int splitIndex in GetSplitIndexes(diagnostic, SpellingData))
                {
                    //TODO: foofooBar > fooBar
                    //if (value.Length - splitIndex >= splitIndex
                    //    && string.Compare(value, 0, value, splitIndex, splitIndex, StringComparison.Ordinal) == 0)
                    //{
                    //    fixes.Add(new SpellingFix(value.Remove(splitIndex, splitIndex), SpellingFixKind.Split));
                    //}

                    bool? canInsertUnderscore = null;

                    if (!diagnostic.IsSymbol
                        || !(canInsertUnderscore ??= _lowercasedSeparatedWithUnderscoresRegex.IsMatch(value)))
                    {
                        // foobar > fooBar
                        // Tvalue > TValue
                        fixes.Add(new SpellingFix(
                            TextUtility.ReplaceRange(value, char.ToUpperInvariant(value[splitIndex]).ToString(), splitIndex, 1),
                            SpellingFixKind.Split));
                    }

                    if (diagnostic.IsSymbol)
                    {
                        // foobar > foo_bar
                        if (canInsertUnderscore == true)
                            fixes.Add(new SpellingFix(value.Insert(splitIndex, "_"), SpellingFixKind.Split));
                    }
                    else if (splitIndex > 1)
                    {
                        // foobar > foo bar
                        fixes.Add(new SpellingFix(value.Insert(splitIndex, " "), SpellingFixKind.Split));
                    }
                }
            }

            //TODO: 
            //if (matches.Length == 0)
            //{
            //    matches = SpellingFixProvider.FuzzyMatches(
            //        diagnostic.ValueLower,
            //        SpellingData,
            //        cancellationToken);

            //    foreach (string match in matches)
            //        fixes.Add(new SpellingFix(match, SpellingFixKind.Fuzzy));
            //}
        }

        private SpellingFix GetUserFix()
        {
            Write("    Enter fix: ");

            string fix = Console.ReadLine()?.Trim();

            return (!string.IsNullOrEmpty(fix))
                ? new SpellingFix(fix, SpellingFixKind.User)
                : default;
        }

        private static void WriteFix(SpellingDiagnostic diagnostic, SpellingFix fix)
        {
            WriteLine($"    Replace '{diagnostic.Value}' with '{fix.Value}'", ConsoleColor.Green, Verbosity.Minimal);
        }

        private void WriteSuggestion(
            SpellingDiagnostic diagnostic,
            SpellingFix fix,
            int index)
        {
            string value = diagnostic.Value;
            string containingValue = diagnostic.ContainingValue;

            if (index == 0)
            {
                Write("    Replace  '");

                if (diagnostic.IsContained)
                {
                    Write(containingValue.Remove(diagnostic.Index));
                    Write(value);
                    Write(containingValue.Substring(diagnostic.EndIndex, containingValue.Length - diagnostic.EndIndex));
                }
                else
                {
                    Write(value, ConsoleColor.Cyan);
                }

                WriteLine("'");
            }

            Write("    ");

            if (Options.Interactive)
            {
                Write($"({index + 1}) ");
            }
            else
            {
                Write("   ");
            }

            Write("with '");

            if (diagnostic.IsContained)
            {
                Write(containingValue.Remove(diagnostic.Index));
                Write(fix.Value, ConsoleColor.Cyan);
                Write(containingValue.Substring(diagnostic.EndIndex, containingValue.Length - diagnostic.EndIndex));
            }
            else
            {
                Write(fix.Value, ConsoleColor.Cyan);
            }

            Write("'");

            if (Options.Interactive)
                Write($" ({index + 1})");

            WriteLine();
        }

        private static bool TryReadSuggestion(out int index)
        {
            Write("    Enter number of a suggestion: ");

            string text = Console.ReadLine()?.Trim();

            if (text?.Length == 1)
            {
                int num = text[0];

                if (num >= 97
                    && num <= 122)
                {
                    index = num - 97;
                    return true;
                }
            }

            if (int.TryParse(
                text,
                NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                CultureInfo.CurrentCulture,
                out index)
                && index > 0)
            {
                index--;
                return true;
            }

            index = -1;
            return false;
        }

        private void ProcessFix(SpellingDiagnostic diagnostic, SpellingFix spellingFix)
        {
            if (spellingFix.Kind != SpellingFixKind.Predefined
                && (spellingFix.Kind != SpellingFixKind.User
                    || TextUtility.TextCasingEquals(diagnostic.Value, spellingFix.Value)))
            {
                SpellingData = SpellingData.AddFix(diagnostic.Value, spellingFix);
            }

            SpellingData = SpellingData.AddWord(spellingFix.Value);
        }

        private static ImmutableArray<int> GetSplitIndexes(
            SpellingDiagnostic diagnostic,
            SpellingData spellingData,
            CancellationToken cancellationToken = default)
        {
            string value = diagnostic.Value;
            int length = value.Length;

            ImmutableArray<int>.Builder splitIndexes = null;

            if (length >= 4)
            {
                char ch = value[0];

                // Tvalue > TValue
                // Ienumerable > IEnumerable
                if ((ch == 'I' || ch == 'T')
                    && diagnostic.Casing == TextCasing.FirstUpper
                    && spellingData.List.Contains(value.Substring(1)))
                {
                    (splitIndexes ??= ImmutableArray.CreateBuilder<int>()).Add(1);
                }
            }

            if (length < 6)
                return splitIndexes?.ToImmutableArray() ?? ImmutableArray<int>.Empty;

            value = diagnostic.ValueLower;

            WordCharMap map = spellingData.List.CharIndexMap;

            ImmutableHashSet<string> values = ImmutableHashSet<string>.Empty;

            for (int i = 0; i < length - 3; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!map.TryGetValue(value, i, out ImmutableHashSet<string> values2))
                    break;

                values = (i == 0) ? values2 : values.Intersect(values2);

                if (values.Count == 0)
                    break;

                if (i < 2)
                    continue;

                foreach (string value2 in values)
                {
                    if (value2.Length != i + 1)
                        continue;

                    ImmutableHashSet<string> values3 = ImmutableHashSet<string>.Empty;

                    for (int j = i + 1; j < length; j++)
                    {
                        if (!map.TryGetValue(value[j], j - i - 1, out ImmutableHashSet<string> values4))
                            break;

                        values3 = (j == i + 1) ? values4 : values3.Intersect(values4);

                        if (values3.Count == 0)
                            break;
                    }

                    foreach (string value3 in values3)
                    {
                        if (value3.Length != length - i - 1)
                            continue;

                        (splitIndexes ??= ImmutableArray.CreateBuilder<int>()).Add(i + 1);
                        break;
                    }

                    break;
                }
            }

            return splitIndexes?.ToImmutableArray() ?? ImmutableArray<int>.Empty;
        }
    }
}
