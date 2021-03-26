// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CommandLine.Xml;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal class AnalyzeAssemblyCommand
    {
        public AnalyzeAssemblyCommand(string language = null)
        {
            Language = language;
        }

        public string Language { get; }

        public CommandResult Execute(AnalyzeAssemblyCommandLineOptions options)
        {
            var assemblies = new HashSet<Assembly>();

            AnalyzerAssemblyInfo[] analyzerAssemblies = options.GetPaths()
                .SelectMany(path => AnalyzerAssemblyLoader.LoadFrom(
                    path: path,
                    searchPattern: options.FileNamePattern ?? AnalyzerAssemblyLoader.DefaultSearchPattern,
                    loadAnalyzers: !options.NoAnalyzers,
                    loadFixers: !options.NoFixers,
                    language: Language))
                .OrderBy(f => f.AnalyzerAssembly.Assembly.GetName().Name)
                .ThenBy(f => f.FilePath)
                .ToArray();

            for (int i = 0; i < analyzerAssemblies.Length; i++)
            {
                AnalyzerAssembly analyzerAssembly = analyzerAssemblies[i].AnalyzerAssembly;

                if (assemblies.Add(analyzerAssembly.Assembly))
                {
                    WriteLine(analyzerAssembly.FullName, ConsoleColor.Cyan, Verbosity.Minimal);

                    if (ShouldWrite(Verbosity.Normal))
                    {
                        WriteAnalyzerAssembly(analyzerAssemblies[i], DiagnosticMap.Create(analyzerAssembly));

                        if (i < analyzerAssemblies.Length - 1)
                            WriteLine(Verbosity.Normal);
                    }
                }
                else
                {
                    Write(analyzerAssembly.FullName, ConsoleColor.DarkGray, Verbosity.Minimal);
                    WriteLine($" [{analyzerAssemblies[i].FilePath}]", ConsoleColor.DarkGray, Verbosity.Minimal);
                }
            }

            if (ShouldWrite(Verbosity.Detailed)
                && analyzerAssemblies.Length > 1)
            {
                DiagnosticMap map = DiagnosticMap.Create(analyzerAssemblies.Select(f => f.AnalyzerAssembly));

                WriteLine(Verbosity.Detailed);
                WriteDiagnostics(map, allProperties: true, useAssemblyQualifiedName: true);
            }

            WriteLine(Verbosity.Minimal);
            WriteLine($"{assemblies.Count} analyzer {((assemblies.Count == 1) ? "assembly" : "assemblies")} found", ConsoleColor.Green, Verbosity.Minimal);

            if (analyzerAssemblies.Length > 0)
            {
                CultureInfo culture = (options.Culture != null) ? CultureInfo.GetCultureInfo(options.Culture) : null;

                foreach (string path in options.Output)
                {
                    WriteLine($"Save '{path}'", ConsoleColor.DarkGray, Verbosity.Diagnostic);

                    string extension = Path.GetExtension(path);

                    if (string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        AnalyzerAssemblyXmlSerializer.Serialize(path, analyzerAssemblies, culture);
                    }
                    else if (string.Equals(extension, ".ruleset", StringComparison.OrdinalIgnoreCase))
                    {
                        WriteLine($"Save ruleset to '{path}'", ConsoleColor.DarkGray, Verbosity.Diagnostic);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        using (XmlWriter xmlWriter = XmlWriter.Create(fileStream, new XmlWriterSettings() { Indent = true, IndentChars = "  ", CloseOutput = false }))
                        {
                            RuleSetUtility.WriteXml(
                                writer: xmlWriter,
                                analyzerAssemblies: analyzerAssemblies.Select(f => f.AnalyzerAssembly),
                                name: "",
                                toolsVersion: new Version(15, 0),
                                description: null,
                                formatProvider: culture);
                        }
                    }
                }
            }

            return (analyzerAssemblies.Length > 0) ? CommandResult.Success : CommandResult.NotSuccess;
        }

        private static void WriteAnalyzerAssembly(AnalyzerAssemblyInfo analyzerAssemblyInfo, DiagnosticMap map)
        {
            AnalyzerAssembly analyzerAssembly = analyzerAssemblyInfo.AnalyzerAssembly;

            WriteLine(Verbosity.Normal);
            WriteLine($"  Location:             {analyzerAssemblyInfo.FilePath}", Verbosity.Normal);

            (int maxLength, int maxDigitLength) = CalculateMaxLength(analyzerAssembly, map);

            if (analyzerAssembly.HasAnalyzers)
            {
                WriteLine($"  DiagnosticAnalyzers:  {map.Analyzers.Length.ToString().PadLeft(maxDigitLength)}", Verbosity.Normal);

                foreach (KeyValuePair<string, ImmutableArray<DiagnosticAnalyzer>> kvp in analyzerAssembly.AnalyzersByLanguage.OrderBy(f => f.Key))
                {
                    WriteLine($"    {GetShortLanguageName(kvp.Key).PadRight(maxLength - 2)}{kvp.Value.Length.ToString().PadLeft(maxDigitLength)}", Verbosity.Normal);
                }

                WriteLine($"  SupportedDiagnostics: {map.SupportedDiagnostics.Length.ToString().PadLeft(maxDigitLength)}", Verbosity.Normal);

                foreach (KeyValuePair<string, ImmutableArray<DiagnosticDescriptor>> kvp in map.SupportedDiagnosticsByPrefix
                    .OrderBy(f => f.Key))
                {
                    WriteLine($"    {kvp.Key.PadRight(maxLength - 2)}{kvp.Value.Length.ToString().PadLeft(maxDigitLength)}", Verbosity.Normal);
                }
            }

            if (analyzerAssembly.HasFixers)
            {
                WriteLine($"  CodeFixProviders:     {map.Fixers.Length.ToString().PadLeft(maxDigitLength)}", Verbosity.Normal);

                foreach (KeyValuePair<string, ImmutableArray<CodeFixProvider>> kvp in analyzerAssembly.FixersByLanguage.OrderBy(f => f.Key))
                {
                    WriteLine($"    {GetShortLanguageName(kvp.Key).PadRight(maxLength - 2)}{kvp.Value.Length.ToString().PadLeft(maxDigitLength)}", Verbosity.Normal);
                }

                WriteLine($"  FixableDiagnosticIds: {map.FixableDiagnosticIds.Length}", Verbosity.Normal);

                foreach (KeyValuePair<string, ImmutableArray<string>> kvp in map.FixableDiagnosticIdsByPrefix
                    .OrderBy(f => f.Key))
                {
                    WriteLine($"    {kvp.Key.PadRight(maxLength - 2)}{kvp.Value.Length.ToString().PadLeft(maxDigitLength)}", Verbosity.Normal);
                }
            }

            if (ShouldWrite(Verbosity.Detailed))
            {
                if (analyzerAssembly.HasAnalyzers)
                {
                    WriteLine(Verbosity.Detailed);
                    WriteDiagnosticAnalyzers(map.Analyzers);
                }

                if (analyzerAssembly.HasFixers)
                {
                    WriteLine(Verbosity.Detailed);
                    WriteCodeFixProviders(map.Fixers);
                }

                WriteLine(Verbosity.Detailed);
                WriteDiagnostics(map, allProperties: false, useAssemblyQualifiedName: false);
            }
        }

        private static (int maxLength, int maxDigitLength) CalculateMaxLength(AnalyzerAssembly analyzerAssembly, DiagnosticMap map)
        {
            int maxLength = 22;
            int maxDigitLength = 0;

            if (analyzerAssembly.HasAnalyzers)
            {
                maxLength = Math.Max(maxLength, analyzerAssembly.AnalyzersByLanguage.Max(f => GetShortLanguageName(f.Key).Length + 3));
                maxLength = Math.Max(maxLength, map.SupportedDiagnosticsByPrefix.Max(f => f.Key.Length + 3));

                maxDigitLength = Math.Max(maxDigitLength, map.Analyzers.Length.ToString().Length);
                maxDigitLength = Math.Max(maxDigitLength, map.SupportedDiagnostics.Length.ToString().Length);
            }

            if (analyzerAssembly.HasFixers)
            {
                maxLength = Math.Max(maxLength, analyzerAssembly.FixersByLanguage.Max(f => GetShortLanguageName(f.Key).Length + 3));
                maxLength = Math.Max(maxLength, map.FixableDiagnosticIdsByPrefix.Max(f => f.Key.Length + 3));

                maxDigitLength = Math.Max(maxDigitLength, map.Fixers.Length.ToString().Length);
                maxDigitLength = Math.Max(maxDigitLength, map.FixableDiagnosticIds.Length.ToString().Length);
            }

            return (maxLength, maxDigitLength);
        }

        private static void WriteDiagnosticAnalyzers(IEnumerable<DiagnosticAnalyzer> analyzers)
        {
            WriteLine("  DiagnosticAnalyzers:", Verbosity.Detailed);

            foreach (DiagnosticAnalyzer analyzer in analyzers.OrderBy(f => f.GetType(), TypeComparer.NamespaceThenName))
            {
                Type type = analyzer.GetType();

                DiagnosticAnalyzerAttribute attribute = type.GetCustomAttribute<DiagnosticAnalyzerAttribute>();

                WriteLine($"    {type.FullName}", Verbosity.Detailed);

                if (ShouldWrite(Verbosity.Diagnostic))
                {
                    WriteLine($"      Languages:            {string.Join(", ", attribute.Languages.Select(f => GetShortLanguageName(f)).OrderBy(f => f))}", ConsoleColor.DarkGray, Verbosity.Diagnostic);
                    WriteLine($"      SupportedDiagnostics: {string.Join(", ", analyzer.SupportedDiagnostics.Select(f => f.Id).Distinct().OrderBy(f => f))}", ConsoleColor.DarkGray, Verbosity.Diagnostic);
                }
            }
        }

        private static void WriteCodeFixProviders(IEnumerable<CodeFixProvider> fixers)
        {
            WriteLine("  CodeFixProviders:", Verbosity.Detailed);

            foreach (CodeFixProvider fixer in fixers.OrderBy(f => f.GetType(), TypeComparer.NamespaceThenName))
            {
                Type type = fixer.GetType();

                ExportCodeFixProviderAttribute attribute = type.GetCustomAttribute<ExportCodeFixProviderAttribute>();

                WriteLine($"    {type.FullName}", Verbosity.Detailed);

                if (ShouldWrite(Verbosity.Diagnostic))
                {
                    WriteLine($"      Languages:            {string.Join(", ", attribute.Languages.Select(f => GetShortLanguageName(f)).OrderBy(f => f))}", ConsoleColor.DarkGray, Verbosity.Diagnostic);
                    WriteLine($"      FixableDiagnosticIds: {string.Join(", ", fixer.FixableDiagnosticIds.Distinct().OrderBy(f => f))}", ConsoleColor.DarkGray, Verbosity.Diagnostic);

                    Write("      FixAllProvider:       ", ConsoleColor.DarkGray, Verbosity.Diagnostic);

                    FixAllProvider fixAllProvider = fixer.GetFixAllProvider();

                    if (fixAllProvider != null)
                    {
                        WriteLine($"{fixAllProvider.GetType().FullName} ({string.Join(", ", fixAllProvider.GetSupportedFixAllScopes().Select(f => f.ToString()).OrderBy(f => f))})", ConsoleColor.DarkGray, Verbosity.Diagnostic);
                    }
                    else
                    {
                        WriteLine("-", ConsoleColor.DarkGray, Verbosity.Diagnostic);
                    }
                }
            }
        }

        private static void WriteDiagnostics(
            DiagnosticMap map,
            bool allProperties = true,
            bool useAssemblyQualifiedName = false)
        {
            WriteLine("  Diagnostics:", Verbosity.Detailed);

            foreach (KeyValuePair<string, DiagnosticDescriptor> kvp in map.DiagnosticsById.OrderBy(f => f.Key))
            {
                WriteDiagnostic(kvp.Key, kvp.Value);
            }

            void WriteDiagnostic(string diagnosticId, DiagnosticDescriptor descriptor)
            {
                if (descriptor == null)
                {
                    WriteLine($"    {diagnosticId}", Verbosity.Detailed);
                }
                else
                {
                    string title = descriptor.Title?.ToString();
                    string messageFormat = descriptor.MessageFormat?.ToString();

                    if (string.IsNullOrEmpty(title))
                        title = messageFormat;

                    WriteLine($"    {diagnosticId} {title}", Verbosity.Detailed);

                    if (allProperties
                        && ShouldWrite(Verbosity.Diagnostic))
                    {
                        if (title != messageFormat)
                            WriteLine($"      MessageFormat:       {messageFormat}", ConsoleColor.DarkGray, Verbosity.Diagnostic);

                        WriteLine($"      Category:            {descriptor.Category}", ConsoleColor.DarkGray, Verbosity.Diagnostic);
                        WriteLine($"      DefaultSeverity:     {descriptor.DefaultSeverity}", ConsoleColor.DarkGray, Verbosity.Diagnostic);
                        WriteLine($"      IsEnabledByDefault:  {descriptor.IsEnabledByDefault}", ConsoleColor.DarkGray, Verbosity.Diagnostic);

                        string description = descriptor.Description?.ToString();

                        if (!string.IsNullOrEmpty(description))
                            WriteLine($"      Description:         {description}", ConsoleColor.DarkGray, Verbosity.Diagnostic);

                        if (!string.IsNullOrEmpty(descriptor.HelpLinkUri))
                            WriteLine($"      HelpLinkUri:         {descriptor.HelpLinkUri}", ConsoleColor.DarkGray, Verbosity.Diagnostic);

                        string customTags = string.Join(", ", descriptor.CustomTags.OrderBy(f => f));

                        if (!string.IsNullOrEmpty(customTags))
                            WriteLine($"      CustomTags:          {customTags}", ConsoleColor.DarkGray, Verbosity.Diagnostic);
                    }
                }

                if (ShouldWrite(Verbosity.Diagnostic))
                {
                    if (map.AnalyzersById.TryGetValue(diagnosticId, out IEnumerable<DiagnosticAnalyzer> analyzers2))
                    {
                        Write("      DiagnosticAnalyzers: ", ConsoleColor.DarkGray, Verbosity.Diagnostic);

                        WriteTypes(analyzers2.Select(f => f.GetType()));
                    }

                    if (map.FixersById.TryGetValue(diagnosticId, out IEnumerable<CodeFixProvider> fixers2))
                    {
                        Write("      CodeFixProviders:    ", ConsoleColor.DarkGray, Verbosity.Diagnostic);

                        WriteTypes(fixers2.Select(f => f.GetType()));
                    }
                }
            }

            void WriteTypes(IEnumerable<Type> types)
            {
                using (IEnumerator<Type> en = types.OrderBy(f => f, TypeComparer.NamespaceThenName).GetEnumerator())
                {
                    if (en.MoveNext())
                    {
                        while (true)
                        {
                            string name = (useAssemblyQualifiedName) ? en.Current.AssemblyQualifiedName : en.Current.FullName;

                            WriteLine(name, ConsoleColor.DarkGray, Verbosity.Diagnostic);

                            if (en.MoveNext())
                            {
                                Write("                           ", ConsoleColor.DarkGray, Verbosity.Diagnostic);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static string GetShortLanguageName(string languageName)
        {
            switch (languageName)
            {
                case LanguageNames.CSharp:
                case LanguageNames.FSharp:
                    return languageName;
                case LanguageNames.VisualBasic:
                    return "VB";
#if DEBUG
                // bug in CodeCracker.CSharp.dll
                case "StaticConstructorExceptionCodeFixProvider":
                case "StringRepresentationCodeFixProvider":
                case "XmlDocumentationCodeFixProvider":
                    return languageName;
#endif
            }

            Debug.Fail(languageName);

            return languageName;
        }
    }
}
