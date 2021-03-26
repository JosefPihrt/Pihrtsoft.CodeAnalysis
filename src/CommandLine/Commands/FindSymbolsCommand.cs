// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.FindSymbols;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal class FindSymbolsCommand : MSBuildWorkspaceCommand
    {
        private static readonly SymbolDisplayFormat _nameAndContainingTypesSymbolDisplayFormat = SymbolDisplayFormat.CSharpErrorMessageFormat.Update(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
                | SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName,
            parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut
                | SymbolDisplayParameterOptions.IncludeType
                | SymbolDisplayParameterOptions.IncludeName
                | SymbolDisplayParameterOptions.IncludeDefaultValue);

        public FindSymbolsCommand(
            FindSymbolsCommandLineOptions options,
            SymbolFinderOptions symbolFinderOptions,
            in ProjectFilter projectFilter) : base(projectFilter)
        {
            Options = options;
            SymbolFinderOptions = symbolFinderOptions;
        }

        public FindSymbolsCommandLineOptions Options { get; }

        public SymbolFinderOptions SymbolFinderOptions { get; }

        public override async Task<CommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            AssemblyResolver.Register();

            HashSet<string> ignoredSymbolIds = (Options.IgnoredSymbolIds.Any())
                ? new HashSet<string>(Options.IgnoredSymbolIds)
                : null;

            var progress = new FindSymbolsProgress();

            ImmutableArray<ISymbol> allSymbols;

            if (projectOrSolution.IsProject)
            {
                Project project = projectOrSolution.AsProject();

                WriteLine($"Analyze '{project.Name}'", Verbosity.Minimal);

                allSymbols = await AnalyzeProject(project, SymbolFinderOptions, progress, cancellationToken);
            }
            else
            {
                Solution solution = projectOrSolution.AsSolution();

                WriteLine($"Analyze solution '{solution.FilePath}'", Verbosity.Minimal);

                ImmutableArray<ISymbol>.Builder symbols = null;

                Stopwatch stopwatch = Stopwatch.StartNew();

                foreach (Project project in FilterProjects(
                    solution,
                    s => s
                        .GetProjectDependencyGraph()
                        .GetTopologicallySortedProjects(cancellationToken)
                        .ToImmutableArray()))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    WriteLine($"  Analyze '{project.Name}'", Verbosity.Minimal);

                    ImmutableArray<ISymbol> projectSymbols = await AnalyzeProject(project, SymbolFinderOptions, progress, cancellationToken);

                    if (!projectSymbols.Any())
                        continue;

                    if (ignoredSymbolIds?.Count > 0)
                    {
                        Compilation compilation = await project.GetCompilationAsync(cancellationToken);

                        ImmutableDictionary<string, ISymbol> symbolsById = ignoredSymbolIds
                            .Select(f => (id: f, symbol: DocumentationCommentId.GetFirstSymbolForDeclarationId(f, compilation)))
                            .Where(f => f.id != null)
                            .ToImmutableDictionary(f => f.id, f => f.symbol);

                        ignoredSymbolIds.ExceptWith(symbolsById.Select(f => f.Key));

                        projectSymbols = projectSymbols.Except(symbolsById.Select(f => f.Value)).ToImmutableArray();

                        if (!projectSymbols.Any())
                            continue;
                    }

                    int maxKindLength = projectSymbols
                        .Select(f => f.GetSymbolGroup())
                        .Distinct()
                        .Max(f => f.ToString().Length);

                    foreach (ISymbol symbol in projectSymbols.OrderBy(f => f, SymbolDefinitionComparer.SystemFirst))
                    {
                        WriteSymbol(symbol, Verbosity.Normal, indentation: "    ", addCommentId: true, padding: maxKindLength);
                    }

                    (symbols ??= ImmutableArray.CreateBuilder<ISymbol>()).AddRange(projectSymbols);
                }

                stopwatch.Stop();

                allSymbols = symbols?.ToImmutableArray() ?? ImmutableArray<ISymbol>.Empty;

                WriteLine($"Done analyzing solution '{solution.FilePath}' in {stopwatch.Elapsed:mm\\:ss\\.ff}", Verbosity.Minimal);
            }

            if (allSymbols.Any())
            {
                Dictionary<SymbolGroup, int> countByGroup = allSymbols
                    .GroupBy(f => f.GetSymbolGroup())
                    .OrderByDescending(f => f.Count())
                    .ThenBy(f => f.Key)
                    .ToDictionary(f => f.Key, f => f.Count());

                int maxKindLength = countByGroup.Max(f => f.Key.ToString().Length);

                int maxCountLength = countByGroup.Max(f => f.Value.ToString().Length);

                WriteLine(Verbosity.Normal);

                foreach (ISymbol symbol in allSymbols.OrderBy(f => f, SymbolDefinitionComparer.SystemFirst))
                {
                    WriteSymbol(symbol, Verbosity.Normal, colorNamespace: true, padding: maxKindLength);
                }

                WriteLine(Verbosity.Normal);

                foreach (KeyValuePair<SymbolGroup, int> kvp in countByGroup)
                {
                    WriteLine($"{kvp.Value.ToString().PadLeft(maxCountLength)} {kvp.Key.ToString().ToLowerInvariant()} symbols", Verbosity.Normal);
                }
            }

            WriteLine(Verbosity.Minimal);
            WriteLine($"{allSymbols.Length} {((allSymbols.Length == 1) ? "symbol" : "symbols")} found", ConsoleColor.Green, Verbosity.Minimal);

            return (allSymbols.Length > 0) ? CommandResult.Success : CommandResult.NotSuccess;
        }

        private static Task<ImmutableArray<ISymbol>> AnalyzeProject(
            Project project,
            SymbolFinderOptions options,
            IFindSymbolsProgress progress,
            CancellationToken cancellationToken)
        {
            return SymbolFinder.FindSymbolsAsync(project, options, progress, cancellationToken);
        }

        protected override void OperationCanceled(OperationCanceledException ex)
        {
            WriteLine("Analysis was canceled.", Verbosity.Quiet);
        }

        private static void WriteSymbol(
            ISymbol symbol,
            Verbosity verbosity,
            string indentation = "",
            bool addCommentId = false,
            bool colorNamespace = false,
            int padding = 0)
        {
            if (!ShouldWrite(verbosity))
                return;

            bool isObsolete = symbol.HasAttribute(MetadataNames.System_ObsoleteAttribute);

            Write(indentation, verbosity);

            string kindText = symbol.GetSymbolGroup().ToString().ToLowerInvariant();

            if (isObsolete)
            {
                Write(kindText, ConsoleColor.DarkGray, verbosity);
            }
            else
            {
                Write(kindText, verbosity);
            }

            Write(' ', padding - kindText.Length + 1, verbosity);

            string namespaceText = symbol.ContainingNamespace.ToDisplayString();

            if (namespaceText.Length > 0)
            {
                if (colorNamespace || isObsolete)
                {
                    Write(namespaceText, ConsoleColor.DarkGray, verbosity);
                    Write(".", ConsoleColor.DarkGray, verbosity);
                }
                else
                {
                    Write(namespaceText, verbosity);
                    Write(".", verbosity);
                }
            }

            string nameText = symbol.ToDisplayString(_nameAndContainingTypesSymbolDisplayFormat);

            if (isObsolete)
            {
                Write(nameText, ConsoleColor.DarkGray, verbosity);
            }
            else
            {
                Write(nameText, verbosity);
            }

            if (addCommentId
                && ShouldWrite(Verbosity.Diagnostic))
            {
                WriteLine(Verbosity.Diagnostic);
                Write(indentation, Verbosity.Diagnostic);
                Write("ID:", ConsoleColor.DarkGray, Verbosity.Diagnostic);
                Write(' ', padding - 2, Verbosity.Diagnostic);
                Write(symbol.GetDocumentationCommentId(), ConsoleColor.DarkGray, Verbosity.Diagnostic);
            }

            WriteLine(verbosity);
        }

        private class FindSymbolsProgress : IFindSymbolsProgress
        {
            public void OnSymbolFound(ISymbol symbol)
            {
            }
        }
    }
}
