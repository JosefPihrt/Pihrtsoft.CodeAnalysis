// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Roslynator.CodeFixes;
using Roslynator.CSharp;
using Roslynator.Diagnostics;
using Roslynator.Documentation;
using Roslynator.FindSymbols;
using Roslynator.Spelling;
using static Roslynator.CommandLine.ParseHelpers;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    //TODO: banner/ruleset add, change, remove
    internal static class Program
    {
        private static int Main(string[] args)
        {
            WriteLine(
                $"Roslynator Command Line Tool version {typeof(Program).GetTypeInfo().Assembly.GetName().Version} "
                    + $"(Roslyn version {typeof(Accessibility).GetTypeInfo().Assembly.GetName().Version})",
                Verbosity.Quiet);

            WriteLine("Copyright (c) Josef Pihrt. All rights reserved.", Verbosity.Quiet);
            WriteLine(Verbosity.Quiet);

            try
            {
                ParserResult<object> parserResult = Parser.Default.ParseArguments<
                    MigrateCommandLineOptions,
#if DEBUG
                    AnalyzeAssemblyCommandLineOptions,
                    FindSymbolsCommandLineOptions,
                    SlnListCommandLineOptions,
                    ListVisualStudioCommandLineOptions,
                    GenerateSourceReferencesCommandLineOptions,
                    ListReferencesCommandLineOptions,
#endif
                    FixCommandLineOptions,
                    AnalyzeCommandLineOptions,
                    ListSymbolsCommandLineOptions,
                    FormatCommandLineOptions,
                    SpellcheckCommandLineOptions,
                    PhysicalLinesOfCodeCommandLineOptions,
                    LogicalLinesOfCodeCommandLineOptions,
                    GenerateDocCommandLineOptions,
                    GenerateDocRootCommandLineOptions
                    >(args);

                var verbosityParsed = false;

                parserResult.WithParsed<AbstractCommandLineOptions>(options =>
                {
                    var defaultVerbosity = Verbosity.Normal;

                    if (options.Verbosity == null
                        || TryParseVerbosity(options.Verbosity, out defaultVerbosity))
                    {
                        ConsoleOut.Verbosity = defaultVerbosity;

                        Verbosity fileLogVerbosity = defaultVerbosity;

                        if (options.FileLogVerbosity == null
                            || TryParseVerbosity(options.FileLogVerbosity, out fileLogVerbosity))
                        {
                            if (options.FileLog != null)
                            {
                                var fs = new FileStream(options.FileLog, FileMode.Create, FileAccess.Write, FileShare.Read);
                                var sw = new StreamWriter(fs, Encoding.UTF8, bufferSize: 4096, leaveOpen: false);
                                Out = new TextWriterWithVerbosity(sw) { Verbosity = fileLogVerbosity };
                            }

                            verbosityParsed = true;
                        }
                    }
                });

                if (!verbosityParsed)
                    return ExitCodes.Error;

                return parserResult.MapResult(
#if DEBUG
                    (AnalyzeAssemblyCommandLineOptions options) => AnalyzeAssembly(options),
                    (FindSymbolsCommandLineOptions options) => FindSymbolsAsync(options).Result,
                    (SlnListCommandLineOptions options) => SlnListAsync(options).Result,
                    (ListVisualStudioCommandLineOptions options) => ListVisualStudio(options),
                    (GenerateSourceReferencesCommandLineOptions options) => GenerateSourceReferencesAsync(options).Result,
                    (ListReferencesCommandLineOptions options) => ListReferencesAsync(options).Result,
#endif
                    (FixCommandLineOptions options) => FixAsync(options).Result,
                    (AnalyzeCommandLineOptions options) => AnalyzeAsync(options).Result,
                    (ListSymbolsCommandLineOptions options) => ListSymbolsAsync(options).Result,
                    (FormatCommandLineOptions options) => FormatAsync(options).Result,
                    (SpellcheckCommandLineOptions options) => SpellcheckAsync(options).Result,
                    (PhysicalLinesOfCodeCommandLineOptions options) => PhysicalLinesOfCodeAsync(options).Result,
                    (LogicalLinesOfCodeCommandLineOptions options) => LogicalLinesOrCodeAsync(options).Result,
                    (GenerateDocCommandLineOptions options) => GenerateDocAsync(options).Result,
                    (GenerateDocRootCommandLineOptions options) => GenerateDocRootAsync(options).Result,
                    (MigrateCommandLineOptions options) => Migrate(options),
                    _ => ExitCodes.Error);
            }
            catch (Exception ex)
            {
                if (ex is AggregateException aggregateException)
                {
                    foreach (Exception innerException in aggregateException.InnerExceptions)
                    {
                        WriteError(innerException);
                    }
                }
                else if (ex is FileNotFoundException
                    || ex is InvalidOperationException)
                {
                    WriteError(ex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                Out?.Dispose();
                Out = null;
            }

            return ExitCodes.Error;
        }

        private static async Task<int> FixAsync(FixCommandLineOptions options)
        {
            if (!options.TryParseDiagnosticSeverity(CodeFixerOptions.Default.SeverityLevel, out DiagnosticSeverity severityLevel))
                return ExitCodes.Error;

            if (!TryParseKeyValuePairs(options.DiagnosticFixMap, out List<KeyValuePair<string, string>> diagnosticFixMap))
                return ExitCodes.Error;

            if (!TryParseKeyValuePairs(options.DiagnosticFixerMap, out List<KeyValuePair<string, string>> diagnosticFixerMap))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnum(options.FixScope, ParameterNames.FixScope, out FixAllScope fixAllScope, FixAllScope.Project))
                return ExitCodes.Error;

            if (!options.TryGetProjectFilter(out ProjectFilter projectFilter))
                return ExitCodes.Error;

            var command = new FixCommand(
                options: options,
                severityLevel: severityLevel,
                diagnosticFixMap: diagnosticFixMap,
                diagnosticFixerMap: diagnosticFixerMap,
                fixAllScope: fixAllScope,
                projectFilter: projectFilter);

            CommandResult result = await command.ExecuteAsync(options.Path, options.MSBuildPath, options.Properties);

            return GetExitCode(result);
        }

        private static async Task<int> AnalyzeAsync(AnalyzeCommandLineOptions options)
        {
            if (!options.TryParseDiagnosticSeverity(CodeAnalyzerOptions.Default.SeverityLevel, out DiagnosticSeverity severityLevel))
                return ExitCodes.Error;

            if (!options.TryGetProjectFilter(out ProjectFilter projectFilter))
                return ExitCodes.Error;

            var command = new AnalyzeCommand(options, severityLevel, projectFilter);

            CommandResult result = await command.ExecuteAsync(options.Path, options.MSBuildPath, options.Properties);

            return GetExitCode(result);
        }

        private static int AnalyzeAssembly(AnalyzeAssemblyCommandLineOptions options)
        {
            string language = null;

            if (options.Language != null
                && !TryParseLanguage(options.Language, out language))
            {
                return ExitCodes.Error;
            }

            var command = new AnalyzeAssemblyCommand(language);

            CommandResult result = command.Execute(options);

            return GetExitCode(result);
        }

        private static async Task<int> FindSymbolsAsync(FindSymbolsCommandLineOptions options)
        {
            if (!options.TryGetProjectFilter(out ProjectFilter projectFilter))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.SymbolGroups, ParameterNames.SymbolGroups, out SymbolGroupFilter symbolGroups, SymbolFinderOptions.Default.SymbolGroups))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.Visibility, ParameterNames.Visibility, out VisibilityFilter visibility, SymbolFinderOptions.Default.Visibility))
                return ExitCodes.Error;

            if (!TryParseMetadataNames(options.WithAttributes, out ImmutableArray<MetadataName> withAttributes))
                return ExitCodes.Error;

            if (!TryParseMetadataNames(options.WithoutAttributes, out ImmutableArray<MetadataName> withoutAttributes))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.WithFlags, ParameterNames.WithFlags, out SymbolFlags withFlags, SymbolFlags.None))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.WithoutFlags, ParameterNames.WithoutFlags, out SymbolFlags withoutFlags, SymbolFlags.None))
                return ExitCodes.Error;

            ImmutableArray<SymbolFilterRule>.Builder rules = ImmutableArray.CreateBuilder<SymbolFilterRule>();

            if (withAttributes.Any())
                rules.Add(new WithAttributeFilterRule(withAttributes));

            if (withoutAttributes.Any())
                rules.Add(new WithoutAttributeFilterRule(withoutAttributes));

            if (withFlags != SymbolFlags.None)
                rules.AddRange(SymbolFilterRuleFactory.FromFlags(withFlags));

            if (withoutFlags != SymbolFlags.None)
                rules.AddRange(SymbolFilterRuleFactory.FromFlags(withoutFlags, invert: true));

            var symbolFinderOptions = new SymbolFinderOptions(
                visibility: visibility,
                symbolGroups: symbolGroups,
                rules: rules,
                ignoreGeneratedCode: options.IgnoreGeneratedCode,
                unusedOnly: options.UnusedOnly);

            var command = new FindSymbolsCommand(
                options: options,
                symbolFinderOptions: symbolFinderOptions,
                projectFilter: projectFilter);

            CommandResult result = await command.ExecuteAsync(options.Path, options.MSBuildPath, options.Properties);

            return GetExitCode(result);
        }

        private static async Task<int> ListSymbolsAsync(ListSymbolsCommandLineOptions options)
        {
            if (!options.TryGetProjectFilter(out ProjectFilter projectFilter))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnum(options.Depth, ParameterNames.Depth, out DocumentationDepth depth, DocumentationDepth.Member))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.WrapList, ParameterNames.WrapList, out WrapListOptions wrapListOptions))
                return ExitCodes.Error;

            if (!TryParseMetadataNames(options.IgnoredAttributes, out ImmutableArray<MetadataName> ignoredAttributes))
                return ExitCodes.Error;

            if (!TryParseMetadataNames(options.IgnoredSymbols, out ImmutableArray<MetadataName> ignoredSymbols))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.IgnoredParts, ParameterNames.IgnoredParts, out SymbolDefinitionPartFilter ignoredParts))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnum(options.Layout, ParameterNames.Layout, out SymbolDefinitionListLayout layout, SymbolDefinitionListLayout.NamespaceList))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.Visibility, ParameterNames.Visibility, out VisibilityFilter visibilityFilter, SymbolFilterOptions.Default.Visibility))
                return ExitCodes.Error;

            ImmutableArray<SymbolFilterRule> rules = (ignoredSymbols.Any())
                ? ImmutableArray.Create<SymbolFilterRule>(new IgnoredNameSymbolFilterRule(ignoredSymbols))
                : ImmutableArray<SymbolFilterRule>.Empty;

            ImmutableArray<AttributeFilterRule> attributeRules = ImmutableArray.Create<AttributeFilterRule>(
                IgnoredAttributeNameFilterRule.Default,
                new IgnoredAttributeNameFilterRule(ignoredAttributes));

            var symbolFilterOptions = new SymbolFilterOptions(
                visibility: visibilityFilter,
                symbolGroups: GetSymbolGroupFilter(),
                rules: rules,
                attributeRules: attributeRules);

            var command = new ListSymbolsCommand(
                options: options,
                symbolFilterOptions: symbolFilterOptions,
                wrapListOptions: wrapListOptions,
                layout: layout,
                ignoredParts: ignoredParts,
                projectFilter: projectFilter);

            CommandResult result = await command.ExecuteAsync(options.Path, options.MSBuildPath, options.Properties);

            return GetExitCode(result);

            SymbolGroupFilter GetSymbolGroupFilter()
            {
                switch (depth)
                {
                    case DocumentationDepth.Member:
                        return SymbolGroupFilter.TypeOrMember;
                    case DocumentationDepth.Type:
                        return SymbolGroupFilter.Type;
                    case DocumentationDepth.Namespace:
                        return SymbolGroupFilter.None;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private static async Task<int> FormatAsync(FormatCommandLineOptions options)
        {
            if (!options.TryGetProjectFilter(out ProjectFilter projectFilter))
                return ExitCodes.Error;

            string endOfLine = options.EndOfLine;

            if (endOfLine != null
                && endOfLine != "lf"
                && endOfLine != "crlf")
            {
                WriteLine($"Unknown end of line '{endOfLine}'.", Verbosity.Quiet);
                return ExitCodes.Error;
            }

            var command = new FormatCommand(options, projectFilter);

            IEnumerable<string> properties = options.Properties;

            CommandResult result = await command.ExecuteAsync(options.Path, options.MSBuildPath, properties);

            return GetExitCode(result);
        }

        private static async Task<int> SpellcheckAsync(SpellcheckCommandLineOptions options)
        {
            if (!TryParseOptionValueAsEnum(options.Visibility, ParameterNames.Visibility, out Visibility visibility))
                return ExitCodes.Error;

            if (!options.TryGetProjectFilter(out ProjectFilter projectFilter))
                return ExitCodes.Error;

            string assemblyPath = typeof(SpellcheckCommandLineOptions).Assembly.Location;

            IEnumerable<string> wordListPaths = Enumerable.Empty<string>();
            IEnumerable<string> fixListPaths = Enumerable.Empty<string>();

            if (!string.IsNullOrEmpty(assemblyPath))
            {
                wordListPaths = Directory.EnumerateFiles(
                    Path.Combine(Path.GetDirectoryName(assemblyPath), "Spelling", "words"),
                    "*.txt",
                    SearchOption.AllDirectories);

                fixListPaths = Directory.EnumerateFiles(
                    Path.Combine(Path.GetDirectoryName(assemblyPath), "Spelling", "fixes"),
                    "*.txt",
                    SearchOption.AllDirectories);
            }

            if (!TryEnsureFullPath(options.Words, out ImmutableArray<string> wordListPaths2))
                return ExitCodes.Error;

            if (!TryEnsureFullPath(options.Fixes, out ImmutableArray<string> fixListPaths2))
                return ExitCodes.Error;

            string newWordsPath = null;

            if (options.NewWords != null
                && !TryEnsureFullPath(options.NewWords, out newWordsPath))
            {
                return ExitCodes.Error;
            }

            string newFixesPath = null;

            if (options.NewFixes != null
                && !TryEnsureFullPath(options.NewFixes, out newFixesPath))
            {
                return ExitCodes.Error;
            }
#if DEBUG
            foreach (string filePath in Directory.EnumerateFiles(@"..\..\..\Spelling", "*.txt", SearchOption.AllDirectories))
            {
                WordList.Normalize(filePath);
            }
#endif
            WordListLoaderResult loaderResult = WordListLoader.Load(wordListPaths.Concat(wordListPaths2));

            FixList fixes = FixList.Load(fixListPaths.Concat(fixListPaths2));

            var data = new SpellingData(loaderResult.List, loaderResult.CaseSensitiveList, fixes);

            var command = new SpellcheckCommand(options, projectFilter, data, visibility, newWordsPath, newFixesPath);

            IEnumerable<string> properties = options.Properties;
#if DEBUG
            Console.WriteLine("Processing word lists");
            WordListHelpers.ProcessWordLists();
#endif
            CommandResult result = await command.ExecuteAsync(options.Path, options.MSBuildPath, properties);

            return GetExitCode(result);
        }

        private static async Task<int> SlnListAsync(SlnListCommandLineOptions options)
        {
            if (!options.TryGetProjectFilter(out ProjectFilter projectFilter))
                return ExitCodes.Error;

            var command = new SlnListCommand(options, projectFilter);

            CommandResult result = await command.ExecuteAsync(options.Path, options.MSBuildPath, options.Properties);

            return GetExitCode(result);
        }

        private static int ListVisualStudio(ListVisualStudioCommandLineOptions options)
        {
            var command = new ListVisualStudioCommand(options);

            CommandResult result = command.Execute();

            return GetExitCode(result);
        }

        private static async Task<int> PhysicalLinesOfCodeAsync(PhysicalLinesOfCodeCommandLineOptions options)
        {
            if (!options.TryGetProjectFilter(out ProjectFilter projectFilter))
                return ExitCodes.Error;

            var command = new PhysicalLinesOfCodeCommand(options, projectFilter);

            CommandResult result = await command.ExecuteAsync(options.Path, options.MSBuildPath, options.Properties);

            return GetExitCode(result);
        }

        private static async Task<int> LogicalLinesOrCodeAsync(LogicalLinesOfCodeCommandLineOptions options)
        {
            if (!options.TryGetProjectFilter(out ProjectFilter projectFilter))
                return ExitCodes.Error;

            var command = new LogicalLinesOfCodeCommand(options, projectFilter);

            CommandResult result = await command.ExecuteAsync(options.Path, options.MSBuildPath, options.Properties);

            return GetExitCode(result);
        }

        private static async Task<int> GenerateDocAsync(GenerateDocCommandLineOptions options)
        {
            if (options.MaxDerivedTypes < 0)
            {
                WriteLine("Maximum number of derived items must be equal or greater than 0.", Verbosity.Quiet);
                return ExitCodes.Error;
            }

            if (!TryParseOptionValueAsEnum(options.Depth, ParameterNames.Depth, out DocumentationDepth depth, DocumentationOptions.Default.Depth))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.IgnoredRootParts, ParameterNames.IgnoredRootParts, out RootDocumentationParts ignoredRootParts, DocumentationOptions.Default.IgnoredRootParts))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.IgnoredNamespaceParts, ParameterNames.IgnoredNamespaceParts, out NamespaceDocumentationParts ignoredNamespaceParts, DocumentationOptions.Default.IgnoredNamespaceParts))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.IgnoredTypeParts, ParameterNames.IgnoredTypeParts, out TypeDocumentationParts ignoredTypeParts, DocumentationOptions.Default.IgnoredTypeParts))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.IgnoredMemberParts, ParameterNames.IgnoredMemberParts, out MemberDocumentationParts ignoredMemberParts, DocumentationOptions.Default.IgnoredMemberParts))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.IncludeContainingNamespace, ParameterNames.IncludeContainingNamespace, out IncludeContainingNamespaceFilter includeContainingNamespaceFilter, DocumentationOptions.Default.IncludeContainingNamespaceFilter))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.OmitMemberParts, ParameterNames.OmitMemberParts, out OmitMemberParts omitMemberParts, OmitMemberParts.None))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnum(options.Visibility, ParameterNames.Visibility, out Visibility visibility))
                return ExitCodes.Error;

            if (!options.TryGetProjectFilter(out ProjectFilter projectFilter))
                return ExitCodes.Error;

            var command = new GenerateDocCommand(
                options,
                depth,
                ignoredRootParts,
                ignoredNamespaceParts,
                ignoredTypeParts,
                ignoredMemberParts,
                omitMemberParts,
                includeContainingNamespaceFilter,
                visibility,
                projectFilter);

            CommandResult result = await command.ExecuteAsync(options.Path, options.MSBuildPath, options.Properties);

            return GetExitCode(result);
        }

        private static async Task<int> GenerateDocRootAsync(GenerateDocRootCommandLineOptions options)
        {
            if (!TryParseOptionValueAsEnumFlags(options.IncludeContainingNamespace, ParameterNames.IncludeContainingNamespace, out IncludeContainingNamespaceFilter includeContainingNamespaceFilter, DocumentationOptions.Default.IncludeContainingNamespaceFilter))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnum(options.Visibility, ParameterNames.Visibility, out Visibility visibility))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnum(options.Depth, ParameterNames.Depth, out DocumentationDepth depth, DocumentationOptions.Default.Depth))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.IgnoredParts, ParameterNames.IgnoredRootParts, out RootDocumentationParts ignoredParts, DocumentationOptions.Default.IgnoredRootParts))
                return ExitCodes.Error;

            if (!options.TryGetProjectFilter(out ProjectFilter projectFilter))
                return ExitCodes.Error;

            var command = new GenerateDocRootCommand(
                options,
                depth,
                ignoredParts,
                includeContainingNamespaceFilter: includeContainingNamespaceFilter,
                visibility,
                projectFilter);

            CommandResult result = await command.ExecuteAsync(options.Path, options.MSBuildPath, options.Properties);

            return GetExitCode(result);
        }

        private static async Task<int> GenerateSourceReferencesAsync(GenerateSourceReferencesCommandLineOptions options)
        {
            if (!TryParseOptionValueAsEnum(options.Depth, ParameterNames.Depth, out DocumentationDepth depth, DocumentationOptions.Default.Depth))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnum(options.Visibility, ParameterNames.Visibility, out Visibility visibility))
                return ExitCodes.Error;

            if (!options.TryGetProjectFilter(out ProjectFilter projectFilter))
                return ExitCodes.Error;

            var command = new GenerateSourceReferencesCommand(
                options,
                depth,
                visibility,
                projectFilter);

            CommandResult result = await command.ExecuteAsync(options.Path, options.MSBuildPath, options.Properties);

            return GetExitCode(result);
        }

        private static int Migrate(MigrateCommandLineOptions options)
        {
            if (!string.Equals(options.Identifier, "roslynator.analyzers", StringComparison.Ordinal))
            {
                WriteLine($"Unknown identifier '{options.Identifier}'.", Verbosity.Quiet);
                return ExitCodes.Error;
            }

            if (!TryParsePaths(options.Path, out ImmutableArray<string> paths))
                return ExitCodes.Error;

            if (!TryParseVersion(options.Version, out Version version))
                return ExitCodes.Error;

            version = new Version(
                version.Major,
                version.Minor,
                Math.Max(version.Build, 0));

            if (version != Versions.Version_3_0_0)
            {
                WriteLine($"Unknown target version '{version}'.", Verbosity.Quiet);
                return ExitCodes.Error;
            }

            var command = new MigrateCommand(
                paths,
                options.Identifier,
                version,
                options.DryRun);

            CommandResult result = command.Execute();

            return GetExitCode(result);
        }

        private static async Task<int> ListReferencesAsync(ListReferencesCommandLineOptions options)
        {
            if (!TryParseOptionValueAsEnum(options.Display, ParameterNames.Display, out MetadataReferenceDisplay display, MetadataReferenceDisplay.Path))
                return ExitCodes.Error;

            if (!TryParseOptionValueAsEnumFlags(options.Type, ParameterNames.Type, out MetadataReferenceFilter metadataReferenceFilter, MetadataReferenceFilter.Dll | MetadataReferenceFilter.Project))
                return ExitCodes.Error;

            if (!options.TryGetProjectFilter(out ProjectFilter projectFilter))
                return ExitCodes.Error;

            var command = new ListReferencesCommand(
                options,
                display,
                metadataReferenceFilter,
                projectFilter);

            CommandResult result = await command.ExecuteAsync(options.Path, options.MSBuildPath, options.Properties);

            return GetExitCode(result);
        }

        private static int GetExitCode(CommandResult result)
        {
            switch (result)
            {
                case CommandResult.Success:
                    return ExitCodes.Success;
                case CommandResult.NotSuccess:
                    return ExitCodes.NotSuccess;
                case CommandResult.Fail:
                case CommandResult.Canceled:
                    return ExitCodes.Error;
                default:
                    throw new InvalidOperationException($"Unknown enum value '{result}'.");
            }
        }

        private static class ExitCodes
        {
            public const int Success = 0;
            public const int NotSuccess = 1;
            public const int Error = 2;
        }
    }
}
